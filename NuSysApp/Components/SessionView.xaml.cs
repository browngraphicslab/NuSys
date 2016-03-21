using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

using NuSysApp.Util;

namespace NuSysApp
{
    public sealed partial class SessionView : Page
    {
        #region Private Members

        private int _penSize = Constants.InitialPenSize;
        private CortanaMode _cortanaModeInstance;
        private FreeFormViewer _activeFreeFormViewer;
        private Options _prevOptions = Options.SelectNode;

        private ContentImporter _contentImporter = new ContentImporter();

        public bool IsPenMode { get; private set; }

        public ChatPopupView ChatPopupWindow
        {
            get { return ChatPopup; }
        }

        public Rectangle LibraryDraggingRectangle
        {
            get { return LibraryDraggingNode; }
        }

        public Image GraphImage
        {
            get { return DraggingGraphImage; }
        }


        #endregion Private Members

        private int initChatNotifs;

        public SessionView()
        {
            this.InitializeComponent();

            CoreWindow.GetForCurrentThread().KeyDown += OnKeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += OnKeyUp;

            PointerEntered += OnPointerEntered;
            PointerExited += OnPointerExited;

            SizeChanged +=
                delegate(object sender, SizeChangedEventArgs args)
                {
                    Clip = new RectangleGeometry {Rect = new Rect(0, 0, args.NewSize.Width, args.NewSize.Height)};
                    Canvas.SetTop(xBtnPen, (args.NewSize.Height- xBtnPen.Height)/2);
                };

            xBtnPen.PointerPressed += delegate(object sender, PointerRoutedEventArgs args)
            {
                ActivatePenMode(true);
                args.Handled = true;
            };
            xBtnPen.PointerExited += delegate (object sender, PointerRoutedEventArgs args)
            {
                ActivatePenMode(false);            
            };

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {

            SessionController.Instance.NuSysNetworkSession.OnNewNetworkUser += delegate(NetworkUser user)
            {
                UserLabel b = new UserLabel(user);
                Users.Children.Add(b);
                user.OnUserRemoved += delegate { Users.Children.Remove(b); };
            };

            var l = WaitingRoomView.GetFirstLoadList();
            var firstId = WaitingRoomView.InitialWorkspaceId;
            if (firstId == null)
            {
                //await LoadEmptyWorkspace();
            }
            else
            {
                await LoadWorkspaceFromServer(l, WaitingRoomView.InitialWorkspaceId);
            }

            SessionController.Instance.SessionView = this;
            xDetailViewer.DataContext = new DetailViewerViewModel();

            await SessionController.Instance.InitializeRecog();

            SessionController.Instance.NuSysNetworkSession.AddNetworkUser(
                new NetworkUser(SessionController.Instance.NuSysNetworkSession.LocalIP) {Name = "Me"});
                //TODO have Trent fix this -trent

           // await Library.Reload();
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
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs eventArgs)
        {
            if (eventArgs.Pointer.PointerDeviceType == PointerDeviceType.Pen && xDetailViewer.Opacity < 0.1)
            {
                var source = (FrameworkElement) eventArgs.OriginalSource;

                _activeFreeFormViewer.SwitchMode(Options.SelectNode, false);
                _prevOptions = Options.SelectNode;
                IsPenMode = false;
            }
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs eventArgs)
        {
            if (eventArgs.Pointer.PointerDeviceType == PointerDeviceType.Pen && _prevOptions != Options.PenGlobalInk &&
                xDetailViewer.Opacity < 0.1)
            {
                var source = (FrameworkElement) eventArgs.OriginalSource;

                _activeFreeFormViewer.SwitchMode(Options.PenGlobalInk, false);
                _prevOptions = Options.PenGlobalInk;
                IsPenMode = true;
            }
        }

        private void OnKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (FocusManager.GetFocusedElement() is TextBox)
                return;

            if (args.VirtualKey == VirtualKey.Shift && _prevOptions != Options.PenGlobalInk &&
                xDetailViewer.Opacity < 0.1)
            {
                ActivatePenMode(true);
            }
        }

        private async void OnKeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (FocusManager.GetFocusedElement() is TextBox)
                return;

            if (args.VirtualKey == VirtualKey.Shift && xDetailViewer.Opacity < 0.1)
            {
                ActivatePenMode(false);
            }
        }

        private void ActivatePenMode(bool val)
        {
     
            if (val)
            {
                if (IsPenMode)
                    return;
                _activeFreeFormViewer.SwitchMode(Options.PenGlobalInk, false);
                _prevOptions = Options.PenGlobalInk;
                IsPenMode = true;
                xBtnPen.Background = new SolidColorBrush(Colors.Firebrick);
                Debug.WriteLine("asdasdas");
            }
            else
            {
                if (!IsPenMode)
                    return;
                _activeFreeFormViewer.SwitchMode(Options.SelectNode, false);
                _prevOptions = Options.SelectNode;
                IsPenMode = false;
                xBtnPen.Background = new SolidColorBrush(Colors.IndianRed);
                Debug.WriteLine("asdasdas");
            }
            
        }

        public async Task LoadWorkspaceFromServer(IEnumerable<string> nodeStrings, string collectionId)
        {
            await
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(
                    new SubscribeToCollectionRequest(collectionId));

            SessionController.Instance.IdToControllers.Clear();

            var elementCollectionInstance = new CollectionElementModel(collectionId)
            {
                Title = "Instance title"
            };

            elementCollectionInstance.LibraryId = collectionId;

            var elementCollectionInstanceController = new ElementCollectionController(elementCollectionInstance);
            SessionController.Instance.IdToControllers[elementCollectionInstance.Id] = elementCollectionInstanceController;

            await OpenCollection(elementCollectionInstanceController);

            xDetailViewer.DataContext = new DetailViewerViewModel();

            foreach (var dict in nodeStrings)
            {
                var msg = new Message(dict);
                msg["creatorContentID"] = collectionId;
                var libraryId = msg.GetString("contentId");

                ElementType type;

                var libraryModel = SessionController.Instance.ContentController.Get(libraryId);
                type = libraryModel.Type;

                if (Constants.IsNode(type))
                {
                    await
                        SessionController.Instance.NuSysNetworkSession.ExecuteRequestLocally(new NewElementRequest(msg));
                        if (type == ElementType.Collection)
                        {
                            var messages = await SessionController.Instance.NuSysNetworkSession.GetCollectionAsElementMessages(libraryId);
                            foreach (var m in messages)
                            {
                                m["creatorContentID"] = libraryId;
                                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestLocally(new NewElementRequest(m));
                            }
                    }
                }
                if (type == ElementType.Link)
                {
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestLocally(new NewLinkRequest(msg));
                }
            }
        }

        public async Task OpenCollection(ElementCollectionController collectionController)
        {
            await DisposeCollectionView(_activeFreeFormViewer);
            if (_activeFreeFormViewer != null && mainCanvas.Children.Contains(_activeFreeFormViewer))
                mainCanvas.Children.Remove(_activeFreeFormViewer);

            var freeFormViewerViewModel = new FreeFormViewerViewModel(collectionController);

            _activeFreeFormViewer = new FreeFormViewer(freeFormViewerViewModel);
            mainCanvas.Children.Insert(0, _activeFreeFormViewer);

            _activeFreeFormViewer.DataContext = freeFormViewerViewModel;

            SessionController.Instance.ActiveFreeFormViewer = freeFormViewerViewModel;
            SessionController.Instance.SessionView = this;

            if (collectionController.LibraryElementModel.Title != null)
                xWorkspaceTitle.Text = collectionController.LibraryElementModel.Title;

            xWorkspaceTitle.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(150, 189, 204, 212));
            xWorkspaceTitle.FontFamily = new FontFamily("Fira Sans UltraLight");

            xWorkspaceTitle.KeyUp += UpdateTitle;
            xWorkspaceTitle.DropCompleted += UpdateTitle;
            //xWorkspaceTitle.Paste += UpdateTitle;

            freeFormViewerViewModel.Controller.TitleChanged += TitleChanged;
            Canvas.SetLeft(xWorkspaceTitle, mainCanvas.ActualWidth - xWorkspaceTitle.ActualWidth - 50);
            //Canvas.SetLeft(xRecord, mainCanvas.ActualWidth - xRecord.ActualWidth*2);
            Users.Height = mainCanvas.ActualHeight - xWorkspaceTitle.ActualHeight;
            Canvas.SetLeft(Users, 65);
            Canvas.SetTop(Users, xWorkspaceTitle.ActualHeight);
            Canvas.SetTop(ChatPopup, mainCanvas.ActualHeight - 70 - ChatPopup.ActualHeight);
            Canvas.SetLeft(ChatPopup, 5);
            Canvas.SetLeft(ChatButton, 5);
            Canvas.SetTop(ChatButton, mainCanvas.ActualHeight - 70);
            Canvas.SetLeft(ChatNotifs, 37);
            Canvas.SetTop(ChatNotifs, mainCanvas.ActualHeight - 67);
            //overlayCanvas.Width = mainCanvas.ActualWidth;
            //overlayCanvas.Height = mainCanvas.ActualHeight;
            Canvas.SetTop(xSearchWindowView, 25);
            Canvas.SetLeft(xSearchWindowView, 50);


            ChatPopup.Visibility = Visibility.Collapsed;
        }

        private void UpdateTitle(object sender, object args)
        {
            var model = ((FreeFormViewerViewModel) _activeFreeFormViewer.DataContext).Model;
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

        public void SearchView()
        {
            Canvas.SetTop(xSearchWindowView, 25);
            Canvas.SetLeft(xSearchWindowView, 50);
        }

        public void SearchHide(object sender, TappedRoutedEventArgs e)
        {
            xSearchWindowView.Visibility = Visibility.Collapsed;
        }


        public void ShowDetailView(ElementController controller)
        {
            var vm = (DetailViewerViewModel) xDetailViewer.DataContext;
            vm.ShowElement(controller);
            vm.MakeTagList();
        }

        public async void OpenFile(ElementViewModel vm)
        {
            String token = vm.Model.GetMetaData("Token")?.ToString();

            if (String.IsNullOrEmpty(token) ||
                (!String.IsNullOrEmpty(token) &&
                 !Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token)))
            {
                return;
            }

            string ext = System.IO.Path.GetExtension(vm.Model.GetMetaData("FilePath").ToString());
            StorageFolder toWriteFolder = NuSysStorages.OpenDocParamsFolder;

            if (Constants.WordFileTypes.Contains(ext))
            {
                using (
                    StreamWriter writer = new StreamWriter(await NuSysStorages.FirstTimeWord.OpenStreamForWriteAsync()))
                {
                    await writer.WriteLineAsync(token);
                }
            }
            else if (Constants.PowerpointFileTypes.Contains(ext))
            {
                using (
                    StreamWriter writer =
                        new StreamWriter(await NuSysStorages.FirstTimePowerpoint.OpenStreamForWriteAsync()))
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

        private async Task DisposeCollectionView(FreeFormViewer oldFreeFormViewer)
        {
        }

        private void ChatButton_OnClick(object sender, RoutedEventArgs e)
        {
            initChatNotifs = ChatPopup.getTexts().Count;
            ChatPopup.Visibility = ChatPopup.Visibility == Visibility.Collapsed
                ? Visibility.Visible
                : Visibility.Collapsed;
            if (ChatPopup.Visibility == Visibility.Visible)
            {
                Canvas.SetTop(ChatPopup, mainCanvas.ActualHeight - 70 - ChatPopup.ActualHeight);
                Canvas.SetLeft(ChatPopup, 5);
                ChatPopup.ClearNewTexts();
            }
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
    }
}