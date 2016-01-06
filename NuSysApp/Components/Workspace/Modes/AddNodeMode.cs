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
        readonly NodeType _nodeType;
        private bool _isDragging;
        private PseudoNode _tempNode;
        private Point _startPos;
        private bool _isFixed;

        public AddNodeMode(WorkspaceView view, NodeType nodeType, bool isFixed) : base(view) {
            _nodeType = nodeType;
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
                await AddNode(_view, _startPos, new Size(r.Width, r.Height), _nodeType);
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
        private async Task AddNode(WorkspaceView view, Point pos, Size size, NodeType nodeType, object data = null)    {
            var vm = (WorkspaceViewModel)view.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(pos);

            if (nodeType == NodeType.Document || nodeType == NodeType.Image || nodeType == NodeType.PDF ||  nodeType == NodeType.Video)
            {
                var storageFile = await FileManager.PromptUserForFile(Constants.AllFileTypes);
                if (storageFile == null) return;

                var fileType = storageFile.ContentType;
                try
                {
             //       CheckFileType(fileType); TODO readd
                }
                catch (Exception e)
                {
                    Debug.WriteLine("The file format you selected is currently unsupported");
                    return;
                }

                if (Constants.ImageFileTypes.Contains(storageFile.FileType))
                {
                    nodeType = NodeType.Image;
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

                if (Constants.PdfFileTypes.Contains(storageFile.FileType))
                {
                    nodeType = NodeType.PDF;
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
                if (Constants.VideoFileTypes.Contains(storageFile.FileType))
                {
                    nodeType = NodeType.Video;
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
            }



            var contentId = SessionController.Instance.ContentController.Add(data == null ? "" :data.ToString());
            var dict = new Dictionary<string, object>();
            dict["width"] = size.Width.ToString();
            dict["height"] = size.Height.ToString();
            //await NetworkConnector.Instance.RequestMakeNode(p.X.ToString(), p.Y.ToString(), nodeType.ToString(), contentId, null, dict);
            vm.ClearSelection();
            vm.ClearMultiSelection();

            if (!_isFixed) { 
            
                // TODO: re-add
                SessionController.Instance.SessionView.FloatingMenu.Reset();
            }           
        }
    }
}
