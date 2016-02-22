using NuSysApp.Util;
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

namespace NuSysApp
{
    public class AddNodeMode : AbstractWorkspaceViewMode
    {
        readonly ElementType _elementType;
        private bool _isDragging;
        private PseudoNode _tempNode;
        private Point _startPos;
        private bool _isFixed;

        public AddNodeMode(WorkspaceView view, ElementType elementType, bool isFixed) : base(view) {
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
            if (!(e.OriginalSource is WorkspaceView))
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

                var wvm = (WorkspaceViewModel) _view.DataContext;
                var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(0, 0, _tempNode.Width, _tempNode.Height));
                await AddNode(_view, _startPos, new Size(r.Width, r.Height), _elementType);
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
        private async Task AddNode(WorkspaceView view, Point pos, Size size, ElementType elementType, object data = null)    {
            var vm = (WorkspaceViewModel)view.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(pos);

            var dict = new Message();
            Dictionary<string, object> metadata;
            if (elementType == ElementType.Document || elementType == ElementType.Word || elementType == ElementType.Powerpoint || elementType == ElementType.Image || elementType == ElementType.PDF ||  elementType == ElementType.Video)
            {
                var storageFile = await FileManager.PromptUserForFile(Constants.AllFileTypes);
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
                    elementType = ElementType.Image;
                    
                    data = Convert.ToBase64String(await MediaUtil.StorageFileToByteArray(storageFile));
                }

                if (Constants.WordFileTypes.Contains(fileType))
                {
                    metadata = new Dictionary<string, object>();
                    metadata["FilePath"] = storageFile.Path;
                    metadata["Token"] = token.Trim();

                    dict["metadata"] = metadata;

                    elementType = ElementType.Word;

                    //data = File.ReadAllBytes(storageFile.Path);
                }

                if (Constants.PowerpointFileTypes.Contains(fileType))
                {
                    metadata = new Dictionary<string, object>();
                    metadata["FilePath"] = storageFile.Path;
                    metadata["Token"] = token.Trim();

                    dict["metadata"] = metadata;

                    elementType = ElementType.Powerpoint;

                    //data = File.ReadAllBytes(storageFile.Path);
                }

                if (Constants.PdfFileTypes.Contains(fileType))
                {
                    elementType = ElementType.PDF;
                    IRandomAccessStream s = await storageFile.OpenReadAsync();

                    byte[] fileBytes = null;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync()){
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream)){
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }

                    data = Convert.ToBase64String(fileBytes);
                }
                if (Constants.VideoFileTypes.Contains(fileType))
                {
                    elementType = ElementType.Video;
                    IRandomAccessStream s = await storageFile.OpenReadAsync();

                    byte[] fileBytes = null;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync()){
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream)){
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }

                    data = Convert.ToBase64String(fileBytes);
                }
                if (Constants.AudioFileTypes.Contains(fileType))
                {
                    elementType = ElementType.Audio;
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

            dict = new Message();
            dict["width"] = size.Width.ToString();
            dict["height"] = size.Height.ToString();
            dict["nodeType"] = elementType.ToString();
            dict["x"] = p.X;
            dict["y"] = p.Y;
            dict["contentId"] = contentId;
            dict["metadata"] = metadata;
            dict["autoCreate"] = true;
            dict["creator"] = SessionController.Instance.ActiveWorkspace.Id;

            var request = new NewNodeRequest(dict);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewContentRequest(contentId, data == null ? "" : data.ToString(), elementType.ToString(), dict.ContainsKey("title") ? dict["title"].ToString() : null));
            //await SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequest(new NewContentSystemRequest(contentId, data == null ? "" : data.ToString()), NetworkClient.PacketType.TCP, null, true);

            vm.ClearSelection();
            vm.ClearMultiSelection();

            if (!_isFixed) { 
                SessionController.Instance.SessionView.FloatingMenu.Reset();
            }           
        }
    }
}
