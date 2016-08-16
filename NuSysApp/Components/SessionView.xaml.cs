using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NusysIntermediate;

namespace NuSysApp
{
    public sealed partial class SessionView : Page
    {
        #region Private Members

        private int _penSize = Constants.InitialPenSize;
        private CortanaMode _cortanaModeInstance;
        private FreeFormViewer _activeFreeFormViewer;
        private Options _prevOptions = Options.SelectNode;

        private IModable _modeInstance = null;
        public IModable ModeInstance => _modeInstance;


        //private ContentImporter _contentImporter = new ContentImporter();


        public bool IsPenMode { get; private set; }

        public ChatPopupView ChatPopupWindow
        {
            get { return ChatPopup; }
        }

        public LibraryDragElement LibraryDraggingRectangle
        {
            get { return LibraryDraggingNode; }
        }

        public Image GraphImage
        {
            get { return DraggingGraphImage; }
        }

        public SpeechToTextBox SpeechToTextBox => xSpeechToTextBox;

        #endregion Private Members

        private int _unseenChatMessagesNum;
        private bool _isChatVisible;
        public SessionView()
        {
            this.InitializeComponent();
            var bounds = Window.Current.Bounds;
            var height = bounds.Height;
            var width = bounds.Width;
            Canvas.SetLeft(xChatBox, width - 300 - 10);
            Canvas.SetTop(xChatBox, height - 375 - 10 - 50 - 10 - 7);
            Canvas.SetLeft(ChatBoxButton, width - 10 - 50);
            Canvas.SetTop(ChatBoxButton, height - 10 - 50);
            Canvas.SetLeft(ChatNotifs, width - 10 - 50 - 10);
            Canvas.SetTop(ChatNotifs, height - 10 - 50 - 10);

            
            _isChatVisible = false;
            _unseenChatMessagesNum = 0;

            CoreWindow.GetForCurrentThread().KeyDown += OnKeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += OnKeyUp;

            SessionController.Instance.SessionView = this;

            SizeChanged +=
                delegate (object sender, SizeChangedEventArgs args)
                {
                    Clip = new RectangleGeometry { Rect = new Rect(0, 0, args.NewSize.Width, args.NewSize.Height) };
                    if (_activeFreeFormViewer != null)
                    {
                        _activeFreeFormViewer.Width = args.NewSize.Width;
                        _activeFreeFormViewer.Height = args.NewSize.Height;
                    }

                };

            xWorkspaceTitle.IsActivated = true;

            Loaded += OnLoaded;

            MainCanvas.SizeChanged += Resize;
            //_glass = new MGlass(MainCanvas);
        }

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            SessionController.Instance.NuSysNetworkSession.OnNewNetworkUser += NewNetworkUser;

            var l = WaitingRoomView.GetFirstLoadList();
            var firstId = WaitingRoomView.InitialWorkspaceId;
            await LoadWorkspaceFromServer(l, WaitingRoomView.InitialWorkspaceId);

            xDetailViewer.DataContext = new DetailViewerViewModel();
            xSearchViewer.DataContext = new SearchViewModel();
            xSpeechToTextBox.DataContext = new SpeechToTextViewModel();
            xChatBox.DataContext = new ChatBoxViewModel();

            var xRegionEditorView = (RegionEditorTabView)xDetailViewer.FindName("xRegionEditorView");
            xRegionEditorView.DataContext = xDetailViewer.DataContext;


            await SessionController.Instance.InitializeRecog();

            foreach (var user in SessionController.Instance.NuSysNetworkSession.NetworkMembers.Values)
            {
                NewNetworkUser(user);
            }
            
            var presentationLinks = await SessionController.Instance.NuSysNetworkSession.GetPresentationLinks(firstId);
            foreach (var presentationlink in presentationLinks ?? new HashSet<PresentationLinkModel>())
            {
                Debug.Assert(presentationlink != null && presentationlink?.InElementId != null && presentationlink?.OutElementId != null);


                // If the two elements the presentation link connects aren't on the current workspace don't make the link
                if (SessionController.Instance.IdToControllers.ContainsKey(presentationlink.InElementId) &&
                    SessionController.Instance.IdToControllers.ContainsKey(presentationlink.OutElementId))
                {
                    UITask.Run( delegate
                    {
                        var vm = new PresentationLinkViewModel(presentationlink);
                        if (PresentationLinkViewModel.Models == null)
                        {
                            PresentationLinkViewModel.Models = new HashSet<PresentationLinkModel>();
                        }
                        PresentationLinkViewModel.Models.Add(presentationlink);
                        new PresentationLinkView(vm);
                    });

                }

            }

            
            
        
        }


        private void NewNetworkUser(NetworkUser user)
        {
            UITask.Run(delegate
            {
                UserLabel b = new UserLabel(user);
                Users.Children.Add(b);
                user.OnUserRemoved += delegate
                {
                    UITask.Run(delegate {
                        Users.Children.Remove(b);
                    });
                };
            });
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs eventArgs)
        {
            if (eventArgs.Pointer.PointerDeviceType == PointerDeviceType.Pen && _prevOptions == Options.PenGlobalInk)
            {

            }
        }

        private void OnKeyDown(CoreWindow sender, KeyEventArgs args)
        {

            if (args.VirtualKey == VirtualKey.Shift && _prevOptions != Options.PenGlobalInk)
            {
                FloatingMenu.ActivatePenMode(true);
            }
        }

        private async void OnKeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Shift)
            {
                FloatingMenu.ActivatePenMode(false);
            }

            if (_modeInstance != null && (args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.Up))
            {
                if (_modeInstance.Next())
                {
                    _modeInstance.MoveToNext();
                    SetModeButtons();
                }
            }

            if (_modeInstance != null && (args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.Down))
            {
                if (_modeInstance.Previous())
                {
                    _modeInstance.MoveToPrevious();
                    SetModeButtons();
                }

            }

            if (_modeInstance != null && args.VirtualKey == VirtualKey.Space)
            {
                _modeInstance.GoToCurrent();
            }

            if (_modeInstance != null && args.VirtualKey == VirtualKey.Escape)
            {
                ExitMode();
            }
        }

        /// <summary>
        /// Shows the box with elements that have the passed in tag
        /// </summary>
        /// <param name="text"></param>
        public void ShowRelatedElements(string tag)
        {
            if ((_modeInstance != null) && (_modeInstance.Mode == ModeType.EXPLORATION))
            {
                var exp = _modeInstance as ExplorationMode;
                exp.ShowRelatedElements(tag);
            }
        }

        public void EnterPresentationMode(ElementViewModel em)
        {
            Debug.Assert(em != null);
            _modeInstance = new PresentationMode(em);

            // change the proper visibilities
            xFloatingMenu.Visibility = Visibility.Collapsed;
            this.xDetailViewer.CloseDv();


            // center the buttons, make them visibile
            var buttonMargin = 10;
            var top = mainCanvas.ActualHeight - PreviousNode.Height - buttonMargin;
            var buttonWidth = PreviousNode.Width;
            var left = (mainCanvas.ActualWidth - buttonMargin) / 2.0 - (2 * buttonWidth) - buttonMargin;
            var buttonDiff = buttonWidth + buttonMargin;
            foreach (var button in new List<Button> { PreviousNode, NextNode, CurrentNode, xPresentation })
            {
                Canvas.SetLeft(button, left);
                Canvas.SetTop(button, top);
                left += buttonDiff;
                button.Visibility = Visibility.Visible;
            }

            // set the buttons
            SetModeButtons();
        }

        public void EnterExplorationMode(ElementViewModel em)
        {
            Debug.Assert(em != null);
            _modeInstance = new ExplorationMode(em);

            /* uncomment this and go into exploration mode for a good time :)
            var myCanvasPic = new ImageBrush() {ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/mapOfParis.jpg"))};
            FreeFormViewer.CanvasColor = myCanvasPic;
            */

            // change the proper visibilities
            xFloatingMenu.Visibility = Visibility.Collapsed;
            this.xDetailViewer.CloseDv();

            // center the buttons, make them visibile
            var buttonMargin = 10;
            var top = mainCanvas.ActualHeight - PreviousNode.Height - buttonMargin;
            var buttonWidth = PreviousNode.Width;
            var left = (mainCanvas.ActualWidth - buttonMargin) / 2.0 - (2 * buttonWidth) - buttonMargin;
            var buttonDiff = buttonWidth + buttonMargin;
            foreach (var button in new List<Button> { PreviousNode, NextNode, CurrentNode, xPresentation })
            {
                Canvas.SetLeft(button, left);
                Canvas.SetTop(button, top);
                left += buttonDiff;
                button.Visibility = Visibility.Visible;
            }

            // set the buttons
            SetModeButtons();
        }

        public void ExploreSelectedObject(ElementViewModel elementViewModel)
        {

            // Only explore if we are in exploration mode
            if (_modeInstance == null || _modeInstance.Mode != ModeType.EXPLORATION)
            {
                return;
            }
            var exp = _modeInstance as ExplorationMode;

            if (elementViewModel == null)
            {
                return; // we can't explore something that doesn't exist
            }

            exp?.MoveTo(elementViewModel);
            SetModeButtons();
        }

        /// <summary>
        /// Gets a data context passed in when an element is clicked
        /// </summary>
        /// <param name="datacontext"></param>
        public void ExploreSelectedObject(object dataContext)
        {

            // Only explore if we are in exploration mode
            if (_modeInstance == null || _modeInstance.Mode != ModeType.EXPLORATION)
            {
                return;
            }
            var exp = _modeInstance as ExplorationMode;

            if (dataContext == null)
            {
                return; // we can't explore something that doesn't exist
            }

            // take care of exploring presentation links in this if statement
            if (dataContext is PresentationLinkViewModel)
            {
                var presLink = dataContext as PresentationLinkViewModel;

                var atom1 = presLink.Model.InElementViewModel;
                var atom2 = presLink.Model.OutElementViewModel;

                // if atom1 is currently selected move to atom2
                if (SessionController.Instance.ActiveFreeFormViewer.Selections.Contains(atom1))
                {
                    exp?.MoveTo(atom2);
                }
                // else if atom2 is currently selected move to atom1
                else if (SessionController.Instance.ActiveFreeFormViewer.Selections.Contains(atom2))
                {
                    exp?.MoveTo(atom1);
                }
                else
                {
                    // if we aren't connected to a link just move to a random side
                    Random rng = new Random();
                    exp?.MoveTo(rng.NextDouble() > .5 ? atom1 : atom2);
                }
            }
            // take care of exploring links in this statement
            else if (dataContext is LinkViewModel)
            {
                var link = dataContext as LinkViewModel;
                Debug.Assert(SessionController.Instance.IdToControllers.ContainsKey(link.LinkModel.InAtomId) &&
                             SessionController.Instance.IdToControllers.ContainsKey(link.LinkModel.OutAtomId));

                var inElementId = link.Controller.InElement.Id;
                Debug.Assert(inElementId != null);
                var elementViewModels = SessionController.Instance.ActiveFreeFormViewer.AllContent.Where(elementVM => elementVM.Id == inElementId).ToList();
                Debug.Assert(elementViewModels != null);
                Debug.Assert(elementViewModels.Count == 1); // we shouldn't have multiple
                var atom1 = elementViewModels.First();

                var outElementId = link.Controller.OutElement.Id;
                Debug.Assert(outElementId != null);
                elementViewModels = SessionController.Instance.ActiveFreeFormViewer.AllContent.Where(elementVM => elementVM.Id == outElementId).ToList();
                Debug.Assert(elementViewModels != null);
                Debug.Assert(elementViewModels.Count == 1); // we shouldn't have multiple
                var atom2 = elementViewModels.First();

                // if atom1 is currently selected move to atom2
                if (SessionController.Instance.ActiveFreeFormViewer.Selections.Contains(atom1))
                {
                    exp?.MoveTo(atom2);
                }
                // else if atom2 is currently selected move to atom1
                else if (SessionController.Instance.ActiveFreeFormViewer.Selections.Contains(atom2))
                {
                    exp?.MoveTo(atom1);
                }
                else
                {
                    // if we aren't connected to a link just move to a random side
                    Random rng = new Random();
                    exp?.MoveTo(rng.NextDouble() > .5 ? atom1 : atom2);
                }

            }
            // explore element view models, but not the freeformviewer, that represents a click on the background
            else if (dataContext is ElementViewModel && !(dataContext is FreeFormViewerViewModel))
            {
                exp?.MoveTo(dataContext as ElementViewModel);
            }

            SetModeButtons();
        }

        public void ExitMode()
        {
           
            _modeInstance.ExitMode();
            _modeInstance = null;
            NextNode.Visibility = Visibility.Collapsed;
            PreviousNode.Visibility = Visibility.Collapsed;
            CurrentNode.Visibility = Visibility.Collapsed;
            xPresentation.Visibility = Visibility.Collapsed;
            xFloatingMenu.Visibility = Visibility.Visible;

            //FreeFormViewer.CanvasColor = new SolidColorBrush(Colors.White);

        }



        private void Presentation_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender == xPresentation)
            {
                ExitMode();
                return;
            }

            if (sender == NextNode)
            {

                _modeInstance.MoveToNext();
            }

            /*
                if (!IsPenMode)
                    return;
                _activeFreeFormViewer.SwitchMode(Options.SelectNode, false);
                _prevOptions = Options.SelectNode;
                IsPenMode = false;
                xBtnPen.BorderBrush = new SolidColorBrush(Constants.color4);
                PenCircle.Background = new SolidColorBrush(Constants.color4);
                */

            if (sender == PreviousNode)
            {
                _modeInstance.MoveToPrevious();
            }

            if (sender == CurrentNode)
            {
                _modeInstance.GoToCurrent();
            }

            // only show next and prev buttons if next and prev nodes exist
            SetModeButtons();
            
        }
       
        private void SetModeButtons()
        {
         
            if (_modeInstance.Next())
            {
                NextNode.Opacity = 1;
                NextNode.Click -= Presentation_OnClick;
                NextNode.Click += Presentation_OnClick;
            }
            else
            {
                NextNode.Opacity = 0.6;
                NextNode.Click -= Presentation_OnClick;
            }
            if (_modeInstance.Previous())
            {
                PreviousNode.Opacity = 1;
                PreviousNode.Click -= Presentation_OnClick;
                PreviousNode.Click += Presentation_OnClick;
            }
            else
            {
                PreviousNode.Opacity = 0.6;
                PreviousNode.Click -= Presentation_OnClick;
            }
        }

        
           

        public async Task LoadWorkspaceFromServer(IEnumerable<ElementModel> elements, string collectionId)
        {
            WaitingRoomView.InitialWorkspaceId = collectionId;

            xLoadingGrid.Visibility = Visibility.Visible;
            


            //await
            // SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(
            //new SubscribeToCollectionRequest(collectionId));

            foreach (var controller in SessionController.Instance.IdToControllers.Values)
            {
                controller.Dispose();
            }

            SessionController.Instance.IdToControllers.Clear();

            var elementCollectionInstance = new CollectionElementModel("Fake Instance ID")
            {
                Title = "Instance title",
                LocationX = -Constants.MaxCanvasSize / 2.0,
                LocationY = -Constants.MaxCanvasSize / 2.0,
                CenterX = -Constants.MaxCanvasSize / 2.0,
                CenterY = -Constants.MaxCanvasSize / 2.0,
                Zoom = 1,
            };

            elementCollectionInstance.LibraryId = collectionId;

            var elementCollectionInstanceController = new ElementCollectionController(elementCollectionInstance);
            SessionController.Instance.IdToControllers[elementCollectionInstance.Id] = elementCollectionInstanceController;

            await OpenCollection(elementCollectionInstanceController);

            xDetailViewer.DataContext = new DetailViewerViewModel();

            var dict = new Dictionary<string, ElementModel>();
            foreach (var element in elements)
            {
                dict[element.Id] = element;
            }

            await Task.Run(async delegate {
                await MakeCollection(new Dictionary<string, ElementModel>(dict));
            });


            Debug.WriteLine("done joining collection: " + collectionId);

            xLoadingGrid.Visibility = Visibility.Collapsed;
        }


        public async Task MakeCollection(Dictionary<string, ElementModel> elementsLeft)
        {
            var made = new HashSet<string>();
            while (elementsLeft.Any())
            {
                await MakeElement(made, elementsLeft, elementsLeft.First().Value);
            }
        }


        public async Task MakeElement(HashSet<string> made, Dictionary<string, ElementModel> elementsLeft, ElementModel element)
        {
            Debug.WriteLine("making element: " + element.Id);
            var libraryModel = SessionController.Instance.ContentController.GetLibraryElementModel(element.LibraryId);
            if (libraryModel == null)
            {
                elementsLeft.Remove(element.Id);
                return;
            }
            var type = libraryModel.Type;

            SessionController.Instance.AddElement(element);

            ///add element
            elementsLeft.Remove(element.Id);
            made.Add(element.Id);
        }

        public async Task OpenCollection(ElementCollectionController collectionController)
        {
            await DisposeCollectionView(_activeFreeFormViewer);
            if (_activeFreeFormViewer != null && mainCanvas.Children.Contains(_activeFreeFormViewer))
            {
                mainCanvas.Children.Remove(_activeFreeFormViewer);
                SessionController.Instance.OnModeChanged -= _activeFreeFormViewer.ChangeMode;
            }



            var freeFormViewerViewModel = new FreeFormViewerViewModel(collectionController);
            // Add the adornment if this collection has a shape
            /*
            if (freeFormViewerViewModel.Model.ShapePoints != null)
            {
                freeFormViewerViewModel.AtomViewList.Add(
                   new AdornmentView(freeFormViewerViewModel.Model.ShapePoints));
            }
            */
            
        

             _activeFreeFormViewer = new FreeFormViewer(freeFormViewerViewModel);
            SessionController.Instance.OnModeChanged += _activeFreeFormViewer.ChangeMode;

            _activeFreeFormViewer.Width = ActualWidth;
            _activeFreeFormViewer.Height = ActualHeight;
            mainCanvas.Children.Insert(0, _activeFreeFormViewer);

            _activeFreeFormViewer.DataContext = freeFormViewerViewModel;

            SessionController.Instance.ActiveFreeFormViewer = freeFormViewerViewModel;
            SessionController.Instance.SessionView = this;

            if (collectionController?.LibraryElementModel?.Title != null)
            {
                xWorkspaceTitle?.SetText(collectionController.LibraryElementModel.Title);
            }

            xWorkspaceTitle.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(150, 189, 204, 212));
            xWorkspaceTitle.FontFamily = new FontFamily("Fira Sans UltraLight");

            xWorkspaceTitle.TextChanged += UpdateTitle;
            xWorkspaceTitle.DropCompleted += UpdateTitle;

            freeFormViewerViewModel.Controller.LibraryElementController.TitleChanged += TitleChanged;

            ChatPopup.Visibility = Visibility.Collapsed;
        }

        private void Resize(object sender, SizeChangedEventArgs e)
        {
            Users.Height = mainCanvas.ActualHeight - xWorkspaceTitle.ActualHeight;
            Canvas.SetLeft(Users, 5);
            Canvas.SetTop(Users, xWorkspaceTitle.ActualHeight);
            Canvas.SetTop(ChatPopup, mainCanvas.ActualHeight - 70 - ChatPopup.ActualHeight);
            Canvas.SetLeft(ChatPopup, 5);
            Canvas.SetLeft(ChatButton, 5);
            Canvas.SetTop(ChatButton, mainCanvas.ActualHeight - 70);
            //Canvas.SetLeft(ChatNotifs, 37);
            //Canvas.SetTop(ChatNotifs, mainCanvas.ActualHeight - 67);
            //Canvas.SetLeft(SnapshotButton, MainCanvas.ActualWidth - 65);
            //Canvas.SetTop(SnapshotButton, MainCanvas.ActualHeight - 65);
        }
        private void UpdateTitle(object sender, object args)
        {
            var model = ((FreeFormViewerViewModel)_activeFreeFormViewer.DataContext).Model;
            SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementController.TitleChanged -= TitleChanged;
            SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementController.SetTitle(xWorkspaceTitle.Text);
            SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementController.TitleChanged += TitleChanged;
            xWorkspaceTitle.TextChanged += TitleChanged;
            model.Title = xWorkspaceTitle.Text;
            xWorkspaceTitle.FontFamily = new FontFamily("Fira Sans UltraLight");
        }

        private void TitleChanged(object source, string title)
        {
            if (xWorkspaceTitle.Text != title)
            {
                xWorkspaceTitle.SetText(title);
            }
        }

        public async void ShowDetailView(LibraryElementController viewable, DetailViewTabType tabToOpenTo = DetailViewTabType.Home)
        {
            // don't edit if we are in exploration or presentation mode
            if (SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION ||
                SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.PRESENTATION)
            {
                return;
            }
            if (viewable is RegionLibraryElementController)
            {
                await xDetailViewer.ShowElement(viewable as RegionLibraryElementController, tabToOpenTo);

            }
            else if (viewable is LibraryElementController)
            {
                await xDetailViewer.ShowElement(viewable as LibraryElementController, tabToOpenTo);
            }
        }

        public async void OpenFile(ElementViewModel vm)
        {
            String token = vm.Controller.LibraryElementController.GetMetadata("Token")?.ToString();

            if (String.IsNullOrEmpty(token) ||
                (!String.IsNullOrEmpty(token) &&
                 !Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token)))
            {
                return;
            }

            string ext = System.IO.Path.GetExtension(vm.Controller.LibraryElementController.GetMetadata("FilePath")?.ToString());
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

        public DetailViewerView DetailViewerView
        {
            get { return xDetailViewer; }
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
            oldFreeFormViewer?.Dispose();
        }

        private void ChatButton_OnClick(object sender, RoutedEventArgs e)
        {
            //initChatNotifs = ChatPopup.getTexts().Count;
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
        public Grid OuterMost { get { return xOuterMost; } }
        public FreeFormViewer FreeFormViewer { get { return _activeFreeFormViewer; } }
        private async void SnapshotButton_OnClick(object sender, RoutedEventArgs e)
        {
            await StaticServerCalls.CreateSnapshot();
        }
        private void CurrentCollectionDV_OnClick(object sender, RoutedEventArgs e)
        {
            xDetailViewer.ShowElement(SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementController);
        }

        internal SearchView SearchView
        {
            get { return xSearchViewer; }
        }
        /// <summary>
        /// Removes the related list box from the sv
        /// </summary>
        public void RemoveRelatedListBox()
        {
            if (_modeInstance != null && _modeInstance.Mode == ModeType.EXPLORATION)
            {
                var exp = _modeInstance as ExplorationMode;
                exp.HideRelatedListBox();
            }
            
        }

        /// <summary>
        /// Notifies the client of a new message by updating the red "unseen messages" label on top of the chat box icon
        /// </summary>
        public void IncrementUnseenMessage()
        {
            if (ChatNotifs.Visibility.Equals(Visibility.Collapsed))
            {
                ChatNotifs.Visibility = Visibility.Visible;
            }
            _unseenChatMessagesNum++;
            NotifNumber.Text = "" + _unseenChatMessagesNum;
        }

        /// <summary>
        /// Retreives the chat box instance
        /// </summary>
        /// <returns></returns>
        public ChatBoxView GetChatBox()
        {
            return xChatBox;
        }

        private void ChatBoxButton_OnClick(object sender, RoutedEventArgs e)
        {
            _isChatVisible = !_isChatVisible;
            if (_isChatVisible)
            {
                xChatBox.Visibility = Visibility.Visible;
                _unseenChatMessagesNum = 0;
                NotifNumber.Text = "" + _unseenChatMessagesNum;
                ChatNotifs.Visibility = Visibility.Visible;
            }
            else
            {
                xChatBox.Visibility = Visibility.Collapsed;
            }
        }
    }
}