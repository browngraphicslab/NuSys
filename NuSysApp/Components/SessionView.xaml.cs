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
using Windows.Networking.NetworkOperators;
using Windows.Devices.Input;
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

    //    private static List<AtomModel> addedModels;
        private static List<AtomModel> createdModels;
        private ContentImporter _contentImporter = new ContentImporter();

        public bool IsPenMode { get; private set; }

        #endregion Private Members

        public SessionView()
        {
            this.InitializeComponent();

            CoreWindow.GetForCurrentThread().KeyDown += OnKeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += OnKeyUp;

            PointerEntered += delegate (object o, PointerRoutedEventArgs eventArgs)
            {
                if (eventArgs.Pointer.PointerDeviceType == PointerDeviceType.Pen &&_prevOptions != Options.PenGlobalInk && xFullScreenViewer.Opacity < 0.1)
                {
                    xFloatingMenu.SetActive(Options.PenGlobalInk);
                    _prevOptions = Options.PenGlobalInk;
                    IsPenMode = true;
                }
            };

            PointerExited += delegate (object o, PointerRoutedEventArgs eventArgs)
            {
                if (eventArgs.Pointer.PointerDeviceType == PointerDeviceType.Pen && xFullScreenViewer.Opacity < 0.1)
                {
                    xFloatingMenu.SetActive(Options.SelectNode);
                    _prevOptions = Options.SelectNode;
                    IsPenMode = false;
                }
            };

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

                await SessionController.Instance.NuSysNetworkSession.Init();
                await SessionController.Instance.InitializeRecog();
                SessionController.Instance.NuSysNetworkSession.OnNewNetworkUser += delegate (NetworkUser user) 
                {
                    var list = SessionController.Instance.NuSysNetworkSession.NetworkMembers.Values;
                    UserLabel b = new UserLabel(user);
                    Users.Children.Add(b);
                    user.OnUserRemoved += delegate
                    {
                        Users.Children.Remove(b);
                    };
                };
                SessionController.Instance.NuSysNetworkSession.AddNetworkUser(new NetworkUser(SessionController.Instance.NuSysNetworkSession.LocalIP) {Name="Me"});
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

            
            createdModels = new List<AtomModel>();
            foreach (var dict in nodeStrings)
            {
                var msg = new Message(dict);
                var id = msg.GetString("id");
                var type = (AtomModel.AtomType) Enum.Parse(typeof(AtomModel.AtomType), msg.GetString("type"));
                if (type == AtomModel.AtomType.Node)
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestLocally(new NewNodeRequest(msg));
                if (type == AtomModel.AtomType.Link)
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestLocally(new NewLinkRequest(msg));
                
                var model = SessionController.Instance.IdToSendables[id] as AtomModel;
                if (model == null)
                    continue;

                createdModels.Add(model);
                await model.UnPack(msg);

                if (model is WorkspaceModel)
                {
                    var wsModel = SessionController.Instance.IdToSendables[id] as AtomModel;
                    await OpenWorkspace((WorkspaceModel)wsModel);
                }
            }

            var addedModels = new List<AtomModel>();
            foreach (var model in createdModels)
            {
                if (!(model is InqCanvasModel))
                {
                    await SessionController.Instance.RecursiveCreate(model, addedModels);  
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
            
            var workspaceModel = new WorkspaceModel( SessionController.Instance.GenerateId() );
            SessionController.Instance.IdToSendables[workspaceModel.Id] = workspaceModel;
            OpenWorkspace(workspaceModel);
            
            xFullScreenViewer.DataContext = new FullScreenViewerViewModel();
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
                Canvas.SetLeft(xWorkspaceTitle, mainCanvas.ActualWidth - xWorkspaceTitle.ActualWidth - 50);
            };
            Canvas.SetLeft(xWorkspaceTitle, mainCanvas.ActualWidth - xWorkspaceTitle.ActualWidth - 50);
            Canvas.SetLeft(xRecord, mainCanvas.ActualWidth - xRecord.ActualWidth);
            Canvas.SetTop(xMediaRecorder, mainCanvas.ActualHeight - xMediaRecorder.ActualHeight);
            Canvas.SetLeft(xMediaRecorder, mainCanvas.ActualWidth - xMediaRecorder.ActualWidth);
            Users.Height = mainCanvas.ActualHeight - xWorkspaceTitle.ActualHeight;
            Canvas.SetLeft(Users, mainCanvas.ActualWidth - Users.ActualWidth);
            Canvas.SetTop(Users, xWorkspaceTitle.ActualHeight);
        }

        public void ShowRecorder()
        {
            xMediaRecorder.Show();
        }

        public void HideRecorder()
        {
            xMediaRecorder.Hide();
        }


        public void ShowFullScreen(AtomModel model)
        {
            var vm = (FullScreenViewerViewModel)xFullScreenViewer.DataContext;
            vm.SetNodeModel(model);
            vm.MakeTagList();
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
            //await NetworkConnector.Instance.RequestMakeNode(p.X.ToString(), p.Y.ToString(), NodeType.Text.ToString(), text, null, props);
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

        private async void OnRecordClick(object sender, RoutedEventArgs e)
        {
            var session = SessionController.Instance;
            if (!session.IsRecording)
            {
                await session.TranscribeVoice();

                //var vm = (WorkspaceViewModel)DataContext;
                //((TextNodeModel)vm.Model).Text = session.SpeechString;
                xWorkspaceTitle.Text = session.SpeechString;
            }
            else
            {
                //var vm = this.DataContext as WorkspaceViewModel;
            }
        }
    }
}