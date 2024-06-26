﻿using NuSysApp.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using NusysIntermediate;

namespace NuSysApp
{
    public class AddNodeMode : AbstractWorkspaceViewMode
    {
        readonly NusysConstants.ElementType _elementType;
        private bool _isDragging;
        private PseudoNode _tempNode;
        private Point _startPos;
        private bool _isFixed;
        private int i; // TODO Remove

        public AddNodeMode(FreeFormViewer view, NusysConstants.ElementType elementType, bool isFixed) : base(view) {
            _elementType = elementType;
            _tempNode = new PseudoNode();
            _isFixed = isFixed;
        }
        
        public override async Task Activate()
        {
            _view.IsRightTapEnabled = true;
            _view.ManipulationMode = ManipulationModes.All;

            _view.ManipulationStarted += OnManipulationStarted;
            _view.ManipulationDelta += OnManipulationDelta;
            _view.ManipulationCompleted += OnManipulationCompleted;
            _view.ManipulationInertiaStarting += OnManipulationInertiaStarting;
        }

        private async void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (!(e.OriginalSource is FreeFormViewer))
            {
             //   e.Handled = true;
                return;
            }
            _tempNode.Width = 1;
            _tempNode.Height = 1;
            _startPos = new Point(e.Position.X, e.Position.Y);
            Canvas.SetLeft(_tempNode, _startPos.X);
            Canvas.SetTop(_tempNode, _startPos.Y);
            SessionController.Instance.SessionView.MainCanvas.Children.Add(_tempNode);         
            _isDragging = true;
         //   e.Handled = true;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (_isDragging) {
                var translation = e.Cumulative.Translation;
                if (translation.X > 0)
                    _tempNode.Width = translation.X;
                if (translation.Y > 0)
                    _tempNode.Height = translation.Y;
            }
        //    e.Handled = true;
        }

        private void OnManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 20.0 * 96.0 / Constants.MaxCanvasSize;

       //     e.Handled =  true;
        } 
        
        private async void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (_isDragging) { 
                SessionController.Instance.SessionView.MainCanvas.Children.Remove(_tempNode);

                var wvm = (FreeFormViewerViewModel) _view.DataContext;
                var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(0, 0, _tempNode.Width, _tempNode.Height));
                await AddNode((FreeFormViewer)_view, _startPos, new Size(r.Width, r.Height), _elementType);
            }
            _isDragging = false;
         //   e.Handled = true;
        }

        public override async Task Deactivate()
        {
            _view.IsRightTapEnabled = false;
            _view.ManipulationMode = ManipulationModes.None;
            _view.ManipulationStarted -= OnManipulationStarted;
            _view.ManipulationDelta -= OnManipulationDelta;
            _view.ManipulationCompleted -= OnManipulationCompleted;
            _view.ManipulationInertiaStarting -= OnManipulationInertiaStarting;
            _isDragging = false;
        }

        public static void CheckFileType(string fileType)
        {
            if (fileType != "application/pdf" && fileType != "image/tiff" && fileType != "image/jpeg" &&
                fileType != "image/png") //TO-DO: allow other types we support that haven't been added here yet
            {
                throw new Exception("The file format you selected is currently supported.");
            }
        }

        // TODO: this should be refactored!
        private async Task AddNode(FreeFormViewer view, Point pos, Size size, NusysConstants.ElementType elementType, object data = null)    {
            var vm = (FreeFormViewerViewModel)view.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(pos);

            var dict = new Message();
            Dictionary<string, object> metadata;
            if (elementType == NusysConstants.ElementType.Word || elementType == NusysConstants.ElementType.Powerpoint || elementType == NusysConstants.ElementType.Image || elementType == NusysConstants.ElementType.PDF || elementType == NusysConstants.ElementType.Video)
            {
                var storageFiles = await FileManager.PromptUserForFiles(Constants.AllFileTypes);
                if (storageFiles == null)
                {
                    return;
                }
                foreach (var storageFile in storageFiles)
                {
                    if (storageFile == null) return;

                    var fileType = storageFile.FileType.ToLower();
                    dict["title"] = storageFile.DisplayName;


                    var token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFile);

                    try
                    {
                        //       CheckFileType(fileType); TODO readd
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("The file format you selected is currently unsupported");
                        return;
                    }

                    if (Constants.ImageFileTypes.Contains(fileType))
                    {
                        elementType = NusysConstants.ElementType.Image;

                        data = Convert.ToBase64String(await MediaUtil.StorageFileToByteArray(storageFile));
                    }

                    if (Constants.WordFileTypes.Contains(fileType))
                    {
                        metadata = new Dictionary<string, object>();
                        metadata["FilePath"] = storageFile.Path;
                        metadata["Token"] = token.Trim();

                        dict["metadata"] = metadata;

                        elementType = NusysConstants.ElementType.Word;

                        //data = File.ReadAllBytes(storageFile.Path);
                    }

                    if (Constants.PowerpointFileTypes.Contains(fileType))
                    {
                        metadata = new Dictionary<string, object>();
                        metadata["FilePath"] = storageFile.Path;
                        metadata["Token"] = token.Trim();

                        dict["metadata"] = metadata;

                        elementType = NusysConstants.ElementType.Powerpoint;

                        //data = File.ReadAllBytes(storageFile.Path);
                    }

                    if (Constants.PdfFileTypes.Contains(fileType))
                    {
                        elementType = NusysConstants.ElementType.PDF;
                        IRandomAccessStream s = await storageFile.OpenReadAsync();

                        byte[] fileBytes = null;
                        using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                        {
                            fileBytes = new byte[stream.Size];
                            using (DataReader reader = new DataReader(stream))
                            {
                                await reader.LoadAsync((uint)stream.Size);
                                reader.ReadBytes(fileBytes);
                            }
                        }

                        data = Convert.ToBase64String(fileBytes);
                    }
                    if (Constants.VideoFileTypes.Contains(fileType))
                    {
                        elementType = NusysConstants.ElementType.Video;
                        IRandomAccessStream s = await storageFile.OpenReadAsync();

                        byte[] fileBytes = null;
                        using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                        {
                            fileBytes = new byte[stream.Size];
                            using (DataReader reader = new DataReader(stream))
                            {
                                await reader.LoadAsync((uint)stream.Size);
                                reader.ReadBytes(fileBytes);
                            }
                        }

                        data = Convert.ToBase64String(fileBytes);
                    }
                    if (Constants.AudioFileTypes.Contains(fileType))
                    {
                        elementType = NusysConstants.ElementType.Audio;
                        IRandomAccessStream s = await storageFile.OpenReadAsync();

                        byte[] fileBytes = null;
                        using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                        {
                            fileBytes = new byte[stream.Size];
                            using (DataReader reader = new DataReader(stream))
                            {
                                await reader.LoadAsync((uint)stream.Size);
                                reader.ReadBytes(fileBytes);
                            }
                        }

                        data = Convert.ToBase64String(fileBytes);
                    }
                }
                var contentId = SessionController.Instance.GenerateId();

                metadata = new Dictionary<string, object>();
                metadata["node_creation_date"] = DateTime.Now;

                metadata["node_type"] = elementType + "Node";
                Random rd = new Random(); //TODO remove
                metadata["random_id"] = (int)rd.Next(100, 200);
                metadata["random_id2"] = (int)rd.Next(1, 100);

                dict = new Message();
                dict["width"] = size.Width.ToString();
                dict["height"] = size.Height.ToString();
                dict["type"] = elementType.ToString();
                dict["x"] = p.X;
                dict["y"] = p.Y;
                dict["contentId"] = contentId;
                dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId;
                dict["metadata"] = metadata;
                dict["autoCreate"] = true;

                var request = new NewElementRequest(dict);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(new CreateNewLibraryElementRequest(contentId, data == null ? "" : data.ToString(), elementType, dict.ContainsKey("title") ? dict["title"].ToString() : null));
                //await SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequest(new NewContentSystemRequest(contentId, data == null ? "" : data.ToString()), NetworkClient.PacketType.TCP, null, true);

                vm.ClearSelection();
                //   vm.ClearMultiSelection();

            }
        
        }
    }
}
