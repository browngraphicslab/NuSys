﻿using System.Threading.Tasks;
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
using NuSysApp.Util;
using System.IO;

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

        private LibraryView _library;

        public bool IsPenMode { get; private set; }
        public ChatPopupView ChatPopupWindow {
            get { return ChatPopup; }
        }

        public Rectangle LibraryDraggingRectangle
        {
            get { return LibraryDraggingNode; }
        }
        public LibraryView Library
        {
            get { return _library; }
        }

        #endregion Private Members

        private int initChatNotifs;

        public SessionView()
        {
            this.InitializeComponent();

            CoreWindow.GetForCurrentThread().KeyDown += OnKeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += OnKeyUp;

            PointerEntered += delegate (object o, PointerRoutedEventArgs eventArgs)
            {
                if (eventArgs.Pointer.PointerDeviceType == PointerDeviceType.Pen &&_prevOptions != Options.PenGlobalInk && xFullScreenViewer.Opacity < 0.1)
                {
                    var source = (FrameworkElement)eventArgs.OriginalSource;
                    if (source.DataContext is FloatingMenuViewModel)
                        return;

                    xFloatingMenu.SetActive(Options.PenGlobalInk);
                    _prevOptions = Options.PenGlobalInk;
                    IsPenMode = true;
                }
            };

            PointerExited += delegate (object o, PointerRoutedEventArgs eventArgs)
            {
                if (eventArgs.Pointer.PointerDeviceType == PointerDeviceType.Pen && xFullScreenViewer.Opacity < 0.1)
                {
                    var source = (FrameworkElement)eventArgs.OriginalSource;
                    if (source.DataContext is FloatingMenuViewModel)
                        return;

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

                //_library = new LibraryView(new LibraryBucketViewModel());

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
                await SessionController.Instance.NuSysNetworkSession.Init();
                await SessionController.Instance.InitializeRecog();
               
                SessionController.Instance.NuSysNetworkSession.AddNetworkUser(new NetworkUser(SessionController.Instance.NuSysNetworkSession.LocalIP) {Name="Me"});

                await ((LibraryBucketViewModel)_library.DataContext).InitializeLibrary();
                ChatPopup.OnNewTextsChanged += delegate(int newTexts)
                {
                    if (newTexts > 0)
                    {
                        ChatNotifs.Opacity = 1;
                        NotifNumber.Text = newTexts.ToString();
                    }
                    else
                    {
                        ChatNotifs.Opacity = 0;
                    }
                    
                };
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
            SessionController.Instance.IdToSendables.Clear();
            
            createdModels = new List<AtomModel>();
            var l = nodeStrings.ToList();
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

            foreach (var model in createdModels)
            {
                if (!(model is InqCanvasModel))
                {
                    await SessionController.Instance.RecursiveCreate(model);  
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
            workspaceModel.Title = "New Workspace";
            SessionController.Instance.IdToSendables[workspaceModel.Id] = workspaceModel;
            OpenWorkspace(workspaceModel);
            
            xFullScreenViewer.DataContext = new FullScreenViewerViewModel();
        }

        public async Task OpenWorkspace(WorkspaceModel model)
        {
            await DisposeWorspace(_activeWorkspace);
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
            xWorkspaceTitle.FontFamily = new FontFamily("Fira Sans UltraLight");

            xWorkspaceTitle.KeyUp += UpdateTitle;
            xWorkspaceTitle.DropCompleted += UpdateTitle;
            xWorkspaceTitle.Paste += UpdateTitle;

            model.TitleChanged += TitleChanged;
            Canvas.SetLeft(xWorkspaceTitle, mainCanvas.ActualWidth - xWorkspaceTitle.ActualWidth - 50);
            Canvas.SetLeft(xRecord, mainCanvas.ActualWidth - xRecord.ActualWidth*2);
            Canvas.SetTop(xMediaRecorder, mainCanvas.ActualHeight - xMediaRecorder.ActualHeight);
            Canvas.SetLeft(xMediaRecorder, mainCanvas.ActualWidth - xMediaRecorder.ActualWidth);
            Users.Height = mainCanvas.ActualHeight - xWorkspaceTitle.ActualHeight;
            Canvas.SetLeft(Users, 65);
            Canvas.SetTop(Users, xWorkspaceTitle.ActualHeight);
            Canvas.SetTop(ChatPopup, mainCanvas.ActualHeight - 70-ChatPopup.ActualHeight);
            Canvas.SetLeft(ChatPopup, 5);
            Canvas.SetLeft(ChatButton, 5);
            Canvas.SetTop(ChatButton, mainCanvas.ActualHeight - 70);
            Canvas.SetLeft(ChatNotifs, 37);
            Canvas.SetTop(ChatNotifs, mainCanvas.ActualHeight - 67);
            //overlayCanvas.Width = mainCanvas.ActualWidth;
            //overlayCanvas.Height = mainCanvas.ActualHeight;
            Canvas.SetTop(xSearchWindowView, 25);
            Canvas.SetLeft(xSearchWindowView, 50);
            _library = new LibraryView(new LibraryBucketViewModel());
            Canvas.SetTop(_library, 140);
            Canvas.SetLeft(_library, mainCanvas.ActualWidth - 450);
            Canvas.SetTop(LibraryMaximizer, 70);
            Canvas.SetLeft(LibraryMaximizer, mainCanvas.ActualWidth - 50);
            _library.Visibility = Visibility.Collapsed;
            ChatPopup.Visibility = Visibility.Collapsed;
            mainCanvas.Children.Add(_library);
        }

        private void UpdateTitle(object sender, object args)
        {
            var model = ((WorkspaceViewModel)_activeWorkspace.DataContext).Model;
            model.Title = xWorkspaceTitle.Text;
            var m = new Message();
            m["id"] = model.Id;
            m["title"] = model.Title;
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new SendableUpdateRequest(m),
                NetworkClient.PacketType.UDP);
            xWorkspaceTitle.FontFamily = new FontFamily("Fira Sans UltraLight");
        }
        private void TitleChanged(object source, string title)
        {
            if (xWorkspaceTitle.Text != title)
            {
                xWorkspaceTitle.Text = title;
            }
        }

        public void ShowRecorder()
        {
            xMediaRecorder.Show();
        }

        public void HideRecorder()
        {
            xMediaRecorder.Hide();
        }

        public void SearchView()
        {
            Canvas.SetTop(xSearchWindowView, 25);
            Canvas.SetLeft(xSearchWindowView, 50);
        }

        public void SearchHide(object sender, TappedRoutedEventArgs e)
        {
            xSearchWindowView.Visibility = Visibility.Collapsed;
        }


        public void ShowFullScreen(AtomModel model)
        {
            var vm = (FullScreenViewerViewModel)xFullScreenViewer.DataContext;
            vm.SetNodeModel(model);
            vm.MakeTagList();
        }

        public async void OpenFile(NodeViewModel vm)
        {
            String token = vm.Model.GetMetaData("Token")?.ToString();

            if (String.IsNullOrEmpty(token) || (!String.IsNullOrEmpty(token) && !Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token)))
            {
                return;
            }

            string ext = System.IO.Path.GetExtension(vm.Model.GetMetaData("FilePath").ToString());
            StorageFolder toWriteFolder = NuSysStorages.OpenDocParamsFolder;

            if (Constants.WordFileTypes.Contains(ext))
            {
                using (StreamWriter writer = new StreamWriter(await NuSysStorages.FirstTimeWord.OpenStreamForWriteAsync()))
                {
                    await writer.WriteLineAsync(token);
                }
            }
            else if (Constants.PowerpointFileTypes.Contains(ext))
            {
                using (StreamWriter writer = new StreamWriter(await NuSysStorages.FirstTimePowerpoint.OpenStreamForWriteAsync()))
                {
                    await writer.WriteLineAsync(token);
                }
            }

            await AccessList.OpenFile(token);
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

        private async Task DisposeWorspace(WorkspaceView oldWorkspaceView)
        {
            
        }

        private void ChatButton_OnClick(object sender, RoutedEventArgs e)
        {
            initChatNotifs = ChatPopup.getTexts().Count;
            ChatPopup.Visibility = ChatPopup.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            if (ChatPopup.Visibility == Visibility.Visible)
            {
                Canvas.SetTop(ChatPopup, mainCanvas.ActualHeight - 70 - ChatPopup.ActualHeight);
                Canvas.SetLeft(ChatPopup, 5);
                ChatPopup.ClearNewTexts();
            }
        }

        private void LibraryMaximizer_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _library.ToggleVisiblity();
        }

        private void MenuVisibility(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (FloatingMenu.Visibility == Visibility.Collapsed)
            {
                Point pos = e.GetPosition(mainCanvas);
                Canvas.SetTop(FloatingMenu, pos.Y);
                Canvas.SetLeft(FloatingMenu, pos.X);
                FloatingMenu.Visibility = Visibility.Visible;
            }
            else
            {
                FloatingMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void UIElement_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
        }
    }
}