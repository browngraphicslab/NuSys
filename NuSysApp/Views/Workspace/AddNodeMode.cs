using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class AddNodeMode : AbstractWorkspaceViewMode
    {
        readonly NodeType _nodeType;

        public AddNodeMode(WorkspaceView view, NodeType nodeType) : base(view) {
            _nodeType = nodeType;
        }
        
        public override async Task Activate()
        {
            _view.IsRightTapEnabled = true;
            _view.RightTapped += OnRightTapped;
        }

        public override async Task Deactivate()
        {
            _view.IsRightTapEnabled = false;
            _view.RightTapped -= OnRightTapped;
        }

        private async void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            await AddNode(_view, e.GetPosition(_view), _nodeType);
            e.Handled = true;
        }

        public static void CheckFileType(string fileType)
        {
            if (fileType != "application.pdf" || fileType != "image/tiff" || fileType != "image/jpeg" ||
                fileType != "image/png" || fileType != "image/gif")
            {
                throw new Exception("The file format you selected is currently supported.");
            }
        }

        // This method is public because it's also used in CortanaMode.cs
        // TODO: this should be refactored!
        public static async Task AddNode(WorkspaceView view, Point pos, NodeType nodeType, object data = null)    {
            var vm = (WorkspaceViewModel)view.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(pos);

            if (nodeType == NodeType.Document || nodeType == NodeType.Image || nodeType == NodeType.PDF)
            {
                var storageFile = await FileManager.PromptUserForFile(Constants.AllFileTypes);
                if (storageFile == null) return;

                var fileType = storageFile.ContentType;
                try
                {
                    CheckFileType(fileType);
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
            }
            await NetworkConnector.Instance.RequestMakeNode(p.X.ToString(), p.Y.ToString(), nodeType.ToString(), data == null ? null : data.ToString());
            vm.ClearSelection();
        }
    }
}
