using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Animation;
using Newtonsoft.Json;

namespace NuSysApp
{

    public sealed partial class SessionView : Page
    {
        #region Private Members

        private int _penSize = Constants.InitialPenSize;
        private bool _cortanaInitialized;
        private CortanaMode _cortanaModeInstance;

        #endregion Private Members

        public SessionView()
        {
            this.InitializeComponent();

            SizeChanged += delegate(object sender, SizeChangedEventArgs args)
            {
                Clip = new RectangleGeometry { Rect = new Rect(0, 0, args.NewSize.Width, args.NewSize.Height) };
            };

            var inqCanvasModel = new InqCanvasModel("WORKSPACE_ID");
            var inqCanvasViewModel = new InqCanvasViewModel(xWorkspace.InqCanvas, inqCanvasModel);
            xWorkspace.InqCanvas.ViewModel = inqCanvasViewModel;
            var workspaceModel = new WorkSpaceModel(inqCanvasModel);
            SessionController.Instance.IdToSendables["WORKSPACE_ID"] = workspaceModel;
            workspaceModel.InqModel = inqCanvasModel;
            var workspaceViewModel = new WorkspaceViewModel(workspaceModel);
            xWorkspace.DataContext = workspaceViewModel;

            SessionController.Instance.ActiveWorkspace = workspaceViewModel;
            SessionController.Instance.SessionView = this;
            xFullScreenViewer.DataContext = new FullScreenViewerViewModel();

          //  await xWorkspace.SetViewMode(new MultiMode(xWorkspace, new PanZoomMode(xWorkspace), new SelectMode(xWorkspace), new FloatingMenuMode(xWorkspace)));

            _cortanaInitialized = false;
            xFloatingMenu.SessionView = this;
            xFloatingMenu.ModeChange += xWorkspace.SwitchMode;

            Loaded += async delegate(object sender, RoutedEventArgs args)
            {

                var callback = new Action<string>(s =>
                {
             //       var nodeModel = (NodeModel) SessionController.Instance.IdToSendables[s];
                //    nodeModel.MoveToGroup(workspaceModel);
                });

                var props = new Dictionary<string,string>();
                props.Add("width","350");
                props.Add("height","200");

                NetworkConnector.Instance.RequestNewGroupTag("100300", "100100", "Lorem", null);
                NetworkConnector.Instance.RequestNewGroupTag("100500", "100100", "Ipsum", null);

                var pdf0 = await KnownFolders.PicturesLibrary.GetFileAsync("html.pdf");
                var pdf1 = await KnownFolders.PicturesLibrary.GetFileAsync("css.pdf");
                var img = await KnownFolders.PicturesLibrary.GetFileAsync("Native-American.jpg");

                var pdfs = new StorageFile[] { pdf0, pdf1 };

                var i = 0;
                foreach (var storageFile in pdfs)
                {
                    byte[] fileBytes = null;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }

                        var data = Convert.ToBase64String(fileBytes);
                        NetworkConnector.Instance.RequestMakeNode((100100 + (i * 300)).ToString(), "100300", NodeType.PDF.ToString(), data, null, new Dictionary<string, string>(props));
                    }
                }

                byte[] f = null;
                using (IRandomAccessStreamWithContentType stream = await img.OpenReadAsync())
                {
                    f = new byte[stream.Size];
                    using (DataReader reader = new DataReader(stream))
                    {
                        await reader.LoadAsync((uint)stream.Size);
                        reader.ReadBytes(f);
                    }

                    var data = Convert.ToBase64String(f);
                    NetworkConnector.Instance.RequestMakeNode((100500).ToString(), "100200", NodeType.Image.ToString(), data, null, new Dictionary<string, string>(props));
                }



                /*
                NetworkConnector.Instance.RequestMakeNode("100100", "100300", NodeType.Text.ToString(), "bla", null, props);
                NetworkConnector.Instance.RequestMakeNode("100300", "100300", NodeType.Text.ToString(), "bla", null, props);
                NetworkConnector.Instance.RequestMakeNode("100500", "100300", NodeType.Text.ToString(), "bla", null, props);
                NetworkConnector.Instance.RequestMakeNode("100700", "100300", NodeType.Text.ToString(), "bla", null, props);
                NetworkConnector.Instance.RequestMakeNode("100900", "100300", NodeType.Text.ToString(), "bla", null, props);
                */



            };
        }

        public void ShowFullScreen(NodeModel model)
        {
            var vm = (FullScreenViewerViewModel)xFullScreenViewer.DataContext;
            vm.SetNodeModel(model);
        }
 

        public void RemoveLoading()
        {
            //TODO remove a loading screen
        }
        
        private async void OnDrop(object sender, DragEventArgs e)
        {
            string text = await e.Data.GetView().GetTextAsync();
            var pos = e.GetPosition(this);
            var vm = (WorkspaceViewModel)this.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(pos);
            var props = new Dictionary<string, string>();
            props["width"] = "400";
            props["height"] = "300";
            await NetworkConnector.Instance.RequestMakeNode(p.X.ToString(), p.Y.ToString(), NodeType.Text.ToString(), text, null, props);
        }
    }
}