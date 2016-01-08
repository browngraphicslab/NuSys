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
using Windows.System;
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
        private Options _prevOptions = Options.SelectNode;

        private static List<AtomModel> addedModels;
        private static List<AtomModel> createdModels;

        public bool IsPenMode { get; private set; }

        #endregion Private Members

        public SessionView()
        {
            this.InitializeComponent();

            CoreWindow.GetForCurrentThread().KeyDown += OnKeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += OnKeyUp;

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

                /*
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
                */
            };
            
        }

        private void OnKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (FocusManager.GetFocusedElement() is TextBox)
                return;

            if (args.VirtualKey == VirtualKey.Shift && _prevOptions != Options.PenGlobalInk && xFullScreenViewer.Opacity < 0.1)
            {
                xFloatingMenu.SetActive(Options.PenGlobalInk);
                _prevOptions = Options.PenGlobalInk;
                IsPenMode = true;
            }

        }

        private void OnKeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (FocusManager.GetFocusedElement() is TextBox)
                return;

            if (args.VirtualKey == VirtualKey.Shift && xFullScreenViewer.Opacity < 0.1)
            {
                xFloatingMenu.SetActive(Options.SelectNode);
                _prevOptions = Options.SelectNode;
                IsPenMode = false;
            }
        }

        public async Task LoadWorksapce( IEnumerable<string> nodeStrings  )
        {
            //await LoadEmptyWorkspace();
            SessionController.Instance.Locks.Clear();
            SessionController.Instance.IdToSendables.Clear();

            var atomCreator = new AtomCreator();

            createdModels = new List<AtomModel>();
            foreach (var dict in nodeStrings)
            {
                var msg = new Message();
                await msg.Init(dict);
                var id = msg.GetString("id", "noId");
                await atomCreator.HandleCreateNewSendable(id, msg);
                var model = SessionController.Instance.IdToSendables[id] as AtomModel;
                if (model == null)
                    continue;

                createdModels.Add(model);
                await model.UnPack(msg);
                if (model is WorkspaceModel)
                {
                    var wsModel = SessionController.Instance.IdToSendables[id] as AtomModel;
                    await OpenWorkspace((WorkspaceModel) wsModel);
                }
               
                
            }

            addedModels = new List<AtomModel>();
            foreach (var model in createdModels)
            {
                if (!(model is WorkspaceModel) && !(model is InqCanvasModel))
                {

                    await CreateCreators(model);
                   
                }
            }
        }

        private async Task CreateCreators(AtomModel node)
        {
            Debug.WriteLine("CreateCreators");
            Debug.WriteLine(node.Id);
            foreach (var creator in node.Creators)
            {
                var creatorModel = (NodeContainerModel)SessionController.Instance.IdToSendables[creator];
                if (!addedModels.Contains(creatorModel))
                {
                    await CreateCreators(creatorModel);
                }
                await creatorModel.AddChild(node);
                addedModels.Add(node);
                //  Debug.WriteLine(node.Id);
            }
            if (node.Creators.Count == 0 && !addedModels.Contains(node))
            {
                var container = (NodeContainerModel)SessionController.Instance.ActiveWorkspace.Model;
                await container.AddChild(node);
                addedModels.Add(node);
                //  Debug.WriteLine(node.Id);
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
            
            var workspaceModel = new WorkspaceModel( SessionController.Instance.GenerateId() );
            SessionController.Instance.IdToSendables[workspaceModel.Id] = workspaceModel;
            OpenWorkspace(workspaceModel);
            
            xFullScreenViewer.DataContext = new FullScreenViewerViewModel();

            //  await xWorkspace.SetViewMode(new MultiMode(xWorkspace, new PanZoomMode(xWorkspace), new SelectMode(xWorkspace), new FloatingMenuMode(xWorkspace)));
            
        }

        public async Task OpenWorkspace(WorkspaceModel model)
        {
            if (_activeWorkspace != null && mainCanvas.Children.Contains(_activeWorkspace))
                mainCanvas.Children.Remove(_activeWorkspace);

            if (_activeWorkspace != null)
                xFloatingMenu.ModeChange -= _activeWorkspace.SwitchMode;

            var workspaceViewModel = new WorkspaceViewModel(model);

            _activeWorkspace = new WorkspaceView(workspaceViewModel);
            mainCanvas.Children.Insert(0, _activeWorkspace);
      
            _activeWorkspace.DataContext = workspaceViewModel;
            xFloatingMenu.ModeChange += _activeWorkspace.SwitchMode;

            SessionController.Instance.ActiveWorkspace = workspaceViewModel;
            SessionController.Instance.SessionView = this;

            if (model.Title != null)
                xWorkspaceTitle.Text = model.Title;

            xWorkspaceTitle.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(150, 189, 204, 212));
            xWorkspaceTitle.TextChanging += delegate
            {
                
                model.Title = xWorkspaceTitle.Text;
                Canvas.SetLeft(xWorkspaceTitle, mainCanvas.ActualWidth - xWorkspaceTitle.ActualWidth - 20);
            };
            Canvas.SetLeft(xWorkspaceTitle, mainCanvas.ActualWidth - xWorkspaceTitle.ActualWidth - 20);
            
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
            /*
            string text = await e.Data.GetView().GetTextAsync();
            var pos = e.GetPosition(this);
            var vm = (WorkspaceViewModel)this.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(pos);
            var props = new Dictionary<string, object>();
            props["width"] = "400";
            props["height"] = "300";
            await NetworkConnector.Instance.RequestMakeNode(p.X.ToString(), p.Y.ToString(), NodeType.Text.ToString(), text, null, props);
        */   
    }

        public FloatingMenuView FloatingMenu
        {
            get { return xFloatingMenu; }
        }

        public Canvas MainCanvas
        {
            get { return mainCanvas; }
        }
    }
}