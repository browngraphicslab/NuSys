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
using NuSysApp.Components;

namespace NuSysApp
{

    public sealed partial class SessionView : Page
    {
        #region Private Members

        private int _penSize = Constants.InitialPenSize;
        private bool _cortanaInitialized;
        private CortanaMode _cortanaModeInstance;
        private WorkspaceView _activeWorkspace;

        #endregion Private Members

        public SessionView()
        {
            this.InitializeComponent();

            SizeChanged += delegate(object sender, SizeChangedEventArgs args)
            {
                Clip = new RectangleGeometry { Rect = new Rect(0, 0, args.NewSize.Width, args.NewSize.Height) };
            };



           

            Loaded += async delegate(object sender, RoutedEventArgs args)
            {
                await LoadEmptyWorkspace();

                SessionController.Instance.SessionView = this;
                xFullScreenViewer.DataContext = new FullScreenViewerViewModel();

                //  await xWorkspace.SetViewMode(new MultiMode(xWorkspace, new PanZoomMode(xWorkspace), new SelectMode(xWorkspace), new FloatingMenuMode(xWorkspace)));

                _cortanaInitialized = false;
                xFloatingMenu.SessionView = this;


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
            };
        }

        public async Task LoadWorksapce( IEnumerable<string> nodeStrings  )
        {
            await LoadEmptyWorkspace();
            var atomCreator = new AtomCreator();

            var createdModel = new List<AtomModel>();
            foreach (var dict in nodeStrings)
            {
                var msg = new Message();
                await msg.Init(dict);
                var id = msg.GetString("id", "noId");
                await atomCreator.HandleCreateNewSendable(id, msg);
                var model = SessionController.Instance.IdToSendables[id] as AtomModel;
                if (model != null) { 
                await model.UnPack(msg);
                createdModel.Add(model);
                }
            }

            foreach (var model in createdModel)
            {
                if (!(model is WorkSpaceModel) && !(model is InqCanvasModel) && model.Creator != null)
                {
                    var container = (NodeContainerModel) SessionController.Instance.IdToSendables[model.Creator];
                    container.AddChild(model);
                }
            }
        }


        public async Task LoadEmptyWorkspace()
        {
            SessionController.Instance.IdToSendables.Clear();

           

            if (_activeWorkspace != null)
            {
                xFloatingMenu.ModeChange -= _activeWorkspace.SwitchMode;
                var wsvm = (WorkspaceViewModel)_activeWorkspace.DataContext;
                wsvm.Dispose();
                mainCanvas.Children.Remove(_activeWorkspace);
                _activeWorkspace = null;
            }

             _activeWorkspace = new WorkspaceView();
            mainCanvas.Children.Insert(0, _activeWorkspace);

            var inqCanvasModel = new InqCanvasModel("WORKSPACE_ID");
            var inqCanvasViewModel = new InqCanvasViewModel(_activeWorkspace.InqCanvas, inqCanvasModel);
            _activeWorkspace.InqCanvas.ViewModel = inqCanvasViewModel;
            var workspaceModel = new WorkSpaceModel(inqCanvasModel);
            SessionController.Instance.IdToSendables["WORKSPACE_ID"] = workspaceModel;
            workspaceModel.InqModel = inqCanvasModel;
            var workspaceViewModel = new WorkspaceViewModel(workspaceModel);
            _activeWorkspace.DataContext = workspaceViewModel;

            SessionController.Instance.ActiveWorkspace = workspaceViewModel;
            SessionController.Instance.SessionView = this;
            xFullScreenViewer.DataContext = new FullScreenViewerViewModel();

            //  await xWorkspace.SetViewMode(new MultiMode(xWorkspace, new PanZoomMode(xWorkspace), new SelectMode(xWorkspace), new FloatingMenuMode(xWorkspace)));

            xFloatingMenu.ModeChange += _activeWorkspace.SwitchMode;
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