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
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using NusysIntermediate;
using NuSysApp.Network.Requests;

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
        private bool _isInitialized;
        public IModable ModeInstance => _modeInstance;


        //private ContentImporter _contentImporter = new ContentImporter();

        /// <summary>
        /// Change this to control whether the session loads as readonly or not
        /// </summary>
        public bool IsReadonly { get; set; }

        public bool IsPenMode { get; private set; }

        //public ChatPopupView ChatPopupWindow
        //{
        //    get { return ChatPopup; }
        //}

        public LibraryDragElement LibraryDraggingRectangle
        {
            get { return LibraryDraggingNode; }
        }


        /// <summary>
        /// Gets the instance of the speech to text box on the main canvas
        /// </summary>
        public SpeechToTextBox SpeechToTextBox => xSpeechToTextBox;

        /// <summary>
        /// Gets the instance of the FileAddedAclsPopup from the main canvas
        /// </summary>
        public FileAddedAclsPopup FileAddedAclsPopup => xFileAddedAclesPopup;

        private string _prevCollectionLibraryId;

        #endregion Private Members


        public SessionView()
        {
            this.InitializeComponent();
            var bounds = Window.Current.Bounds;
 
            CoreWindow.GetForCurrentThread().KeyDown += OnKeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += OnKeyUp;

            SessionController.Instance.SessionView = this;

            _activeFreeFormViewer = new FreeFormViewer();
            _activeFreeFormViewer.Width = ActualWidth;
            _activeFreeFormViewer.Height = ActualHeight;
            mainCanvas.Children.Insert(0, _activeFreeFormViewer);


            xLoadingGrid.PointerPressed += XLoadingGridOnPointerPressed;
        }

        public void SetPreviousCollection(string collectionLibraryId)
        {
            _prevCollectionLibraryId = collectionLibraryId;
            PrevCollection.Visibility = _prevCollectionLibraryId != null ? Visibility.Visible : Visibility.Collapsed;
        }
        private void XLoadingGridOnPointerPressed(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            SessionController.Instance.LoadCapturedState();
        }

        /// <summary>
        /// Makes a workspace readonly by showing the readonly menu and modifying the modes
        /// </summary>
        public void MakeWorkspaceReadonly()
        {
            // toggle visibility and activity of some ui elements
            xFloatingMenu.Visibility = Visibility.Collapsed;
            xReadonlyFloatingMenu.Visibility = Visibility.Collapsed;
            //xCurrentCollectionDVButton.Visibility = Visibility.Collapsed;
            this.IsReadonly = true;
        }

        /// <summary>
        /// Reverts a workspace back to editable by modifying ui elements and the session mode
        /// </summary>
        public void MakeWorkspaceEditable()
        {
            // toggle visibility of some ui elements
            xFloatingMenu.Visibility = Visibility.Visible;
            xReadonlyFloatingMenu.Visibility = Visibility.Collapsed;
           // xCurrentCollectionDVButton.Visibility = Visibility.Visible;
            this.IsReadonly = false;
        }

        public async Task Init()
        {
            if (!_isInitialized)
            {

                SizeChanged += delegate(object sender, SizeChangedEventArgs args)
                {
                    Clip = new RectangleGeometry {Rect = new Rect(0, 0, args.NewSize.Width, args.NewSize.Height)};
                    if (_activeFreeFormViewer != null)
                    {
                        _activeFreeFormViewer.Width = args.NewSize.Width;
                        _activeFreeFormViewer.Height = args.NewSize.Height;
                    }
                };

                //MainCanvas.SizeChanged += Resize;

                //SessionController.Instance.NuSysNetworkSession.OnNewNetworkUser += NewNetworkUser;
                //SessionController.Instance.NuSysNetworkSession.OnNetworkUserDropped += DropNetworkUser;



                xDetailViewer.DataContext = new DetailViewerViewModel();
                xSearchViewer.DataContext = new SearchViewModel();
                xSpeechToTextBox.DataContext = new SpeechToTextViewModel();
                xFileAddedAclesPopup.DataContext = new FileAddedAclsPopupViewModel();
                //xChatBox.DataContext = new ChatBoxViewModel();

                var xRegionEditorView = (RegionEditorTabView) xDetailViewer.FindName("xRegionEditorView");
                xRegionEditorView.DataContext = xDetailViewer.DataContext;


                await SessionController.Instance.InitializeRecog();

                //foreach (var user in SessionController.Instance.NuSysNetworkSession.NetworkMembers.Values)
                //{
                //    NewNetworkUser(user);
                //}

                xFloatingMenu.Library.Init();
            }
            _isInitialized = true;
            var collectionId = WaitingRoomView.InitialWorkspaceId;
            await SessionController.Instance.EnterCollection(collectionId);

       
        }


        //private async void NewNetworkUser(NetworkUser user)
        //{
        //    await UITask.Run(delegate
        //    {
        //        UserLabel b = new UserLabel(user);
        //        Users.Children.Add(b);
        //    });
        //    Resize(null, null);
        //}

        /// <summary>
        /// the private event handler for the NusysNetworkSession dropping a networkuser. 
        /// The string will be the User ID of  the network user dropped.
        /// </summary>
        /// <param name="userId"></param>
        //private void DropNetworkUser(string userId)
        //{
        //    UITask.Run(delegate
        //    {
        //        foreach (var child in Users.Children)
        //        {
        //            var user = child as UserLabel;
        //            Debug.Assert(user != null);
        //            if (user.UserId == userId)
        //            {
        //                Users.Children.Remove(user);
        //                break;
        //            }
        //        }
        //    });
        //}

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

            //if (_modeInstance != null && (args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.Up))
            //{
            //    if (_modeInstance.Next())
            //    {
            //        _modeInstance.MoveToNext();
            //        SetModeButtons();
            //    }
            //}

            //if (_modeInstance != null && (args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.Down))
            //{
            //    if (_modeInstance.Previous())
            //    {
            //        _modeInstance.MoveToPrevious();
            //        SetModeButtons();
            //    }

            //}

            //if (_modeInstance != null && args.VirtualKey == VirtualKey.Space)
            //{
            //    _modeInstance.GoToCurrent();
            //}

            //if (_modeInstance != null && args.VirtualKey == VirtualKey.Escape)
            //{
            //    ExitMode();
            //}
        }

        /// <summary>
        /// Shows the box with elements that have the passed in tag
        /// </summary>
        /// <param name="text"></param>
        public void ShowRelatedElements(string tag)
        {
            var exp = _modeInstance as ExplorationMode;
            if (exp != null)
            {
                exp.ShowRelatedElements(tag);
            }
        }

        //public void EnterPresentationMode(ElementViewModel em)
        //{
        //    Debug.Assert(em != null);

        //    // Don't do anything if we're already in presentation mode
        //    if (_modeInstance?.Mode == ModeType.PRESENTATION)
        //    {
        //        return;
        //    }
        //    _modeInstance = new PresentationMode(em);
        //    SessionController.Instance.SwitchMode(Options.Presentation);

        //    // change the proper visibilities
        //    xFloatingMenu.Visibility = Visibility.Collapsed;
        //    xReadonlyFloatingMenu.Visibility = Visibility.Collapsed;
        //    this.xDetailViewer.CloseDv();


        //    // center the buttons, make them visibile
        //    var buttonMargin = 10;
        //    var top = mainCanvas.ActualHeight - PreviousNode.Height - buttonMargin;
        //    var buttonWidth = PreviousNode.Width;
        //    var left = (mainCanvas.ActualWidth - buttonMargin) / 2.0 - (2 * buttonWidth) - buttonMargin;
        //    var buttonDiff = buttonWidth + buttonMargin;
        //    foreach (var button in new List<Button> { PreviousNode, NextNode, CurrentNode, xPresentation })
        //    {
        //        Canvas.SetLeft(button, left);
        //        Canvas.SetTop(button, top);
        //        left += buttonDiff;
        //        button.Visibility = Visibility.Visible;
        //    }

        //    // set the buttons
        //    SetModeButtons();
        //}

        //public void EnterExplorationMode(ElementViewModel em = null)
        //{
        //    //Debug.Assert(em != null);
        //    _modeInstance = new ExplorationMode(em);

        //    /* uncomment this and go into exploration mode for a good time :)
        //    var myCanvasPic = new ImageBrush() {ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/mapOfParis.jpg"))};
        //    FreeFormViewer.CanvasColor = myCanvasPic;
        //    */

        //    // change the proper visibilities
        //    xFloatingMenu.Visibility = Visibility.Collapsed;
        //    xReadonlyFloatingMenu.Visibility = Visibility.Collapsed;
        //    this.xDetailViewer.CloseDv();

        //    // center the buttons, make them visibile
        //    var buttonMargin = 10;
        //    var top = mainCanvas.ActualHeight - PreviousNode.Height - buttonMargin;
        //    var buttonWidth = PreviousNode.Width;
        //    var left = (mainCanvas.ActualWidth - buttonMargin) / 2.0 - (2 * buttonWidth) - buttonMargin;
        //    var buttonDiff = buttonWidth + buttonMargin;
        //    foreach (var button in new List<Button> { PreviousNode, NextNode, CurrentNode, xPresentation })
        //    {
        //        Canvas.SetLeft(button, left);
        //        Canvas.SetTop(button, top);
        //        left += buttonDiff;
        //        button.Visibility = Visibility.Visible;
        //    }

        //    // set the buttons
        //    SetModeButtons();


        //}

        //public void ExploreSelectedObject(ElementViewModel elementViewModel)
        //{
        //    var exp = _modeInstance as ExplorationMode;

        //    if (elementViewModel == null)
        //    {
        //        return; // we can't explore something that doesn't exist
        //    }

        //    exp?.MoveTo(elementViewModel);
        //    SetModeButtons();
        //}



        ///// <summary>
        ///// Gets a data context passed in when an element is clicked
        ///// </summary>
        ///// <param name="datacontext"></param>
        //public void ExploreSelectedObject(object dataContext)
        //{
        //    var exp = _modeInstance as ExplorationMode;

        //    if (dataContext == null)
        //    {
        //        return; // we can't explore something that doesn't exist
        //    }

        //    // take care of exploring presentation links in this if statement
        //    if (dataContext is PresentationLinkViewModel)
        //    {
        //        var presLink = dataContext as PresentationLinkViewModel;

        //        var atom1 = PresentationMode.GetElementViewModelFromId(presLink.Model.InElementId);
        //        var atom2 = PresentationMode.GetElementViewModelFromId(presLink.Model.OutElementId);

        //        // if atom1 is currently selected move to atom2
        //        if (SessionController.Instance.ActiveFreeFormViewer.Selections.Contains(atom1))
        //        {
        //            exp?.MoveTo(atom2);
        //        }
        //        // else if atom2 is currently selected move to atom1
        //        else if (SessionController.Instance.ActiveFreeFormViewer.Selections.Contains(atom2))
        //        {
        //            exp?.MoveTo(atom1);
        //        }
        //        else
        //        {
        //            // if we aren't connected to a link just move to a random side
        //            Random rng = new Random();
        //            exp?.MoveTo(rng.NextDouble() > .5 ? atom1 : atom2);
        //        }
        //    }
        //    // take care of exploring links in this statement
        //    else if (dataContext is LinkViewModel)
        //    {
        //        var link = dataContext as LinkViewModel;
        //        Debug.Assert(SessionController.Instance.IdToControllers.ContainsKey(link.LinkModel.InAtomId) &&
        //                     SessionController.Instance.IdToControllers.ContainsKey(link.LinkModel.OutAtomId));

        //        var inElementId = link.Controller.InElement.Id;
        //        Debug.Assert(inElementId != null);
        //        var elementViewModels = SessionController.Instance.ActiveFreeFormViewer.AllContent.Where(elementVM => elementVM.Id == inElementId).ToList();
        //        Debug.Assert(elementViewModels != null);
        //        Debug.Assert(elementViewModels.Count == 1); // we shouldn't have multiple
        //        var atom1 = elementViewModels.First();

        //        var outElementId = link.Controller.OutElement.Id;
        //        Debug.Assert(outElementId != null);
        //        elementViewModels = SessionController.Instance.ActiveFreeFormViewer.AllContent.Where(elementVM => elementVM.Id == outElementId).ToList();
        //        Debug.Assert(elementViewModels != null);
        //        Debug.Assert(elementViewModels.Count == 1); // we shouldn't have multiple
        //        var atom2 = elementViewModels.First();

        //        // if atom1 is currently selected move to atom2
        //        if (SessionController.Instance.ActiveFreeFormViewer.Selections.Contains(atom1))
        //        {
        //            exp?.MoveTo(atom2);
        //        }
        //        // else if atom2 is currently selected move to atom1
        //        else if (SessionController.Instance.ActiveFreeFormViewer.Selections.Contains(atom2))
        //        {
        //            exp?.MoveTo(atom1);
        //        }
        //        else
        //        {
        //            // if we aren't connected to a link just move to a random side
        //            Random rng = new Random();
        //            exp?.MoveTo(rng.NextDouble() > .5 ? atom1 : atom2);
        //        }

        //    }
        //    // explore element view models, but not the freeformviewer, that represents a click on the background
        //    else if (dataContext is ElementViewModel && !(dataContext is FreeFormViewerViewModel))
        //    {
        //        exp?.MoveTo(dataContext as ElementViewModel);
        //    }

        //    SetModeButtons();
        //}

        public void ShowBlockingScreen(bool visible)
        {
            xLoadingGrid.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Exits either presentation or exploration mode by modifying the proper UI elements
        /// </summary>
        //public void ExitMode()
        //{

        //    _modeInstance.ExitMode();
        //    _modeInstance = null;
        //    NextNode.Visibility = Visibility.Collapsed;
        //    PreviousNode.Visibility = Visibility.Collapsed;
        //    CurrentNode.Visibility = Visibility.Collapsed;
        //    xPresentation.Visibility = Visibility.Collapsed;

        //    // Make sure to make appropriate changes based on whether or not we are in read only mode
        //    if (this.IsReadonly)
        //    {
        //        xReadonlyFloatingMenu.Visibility = Visibility.Visible;
        //        xReadonlyFloatingMenu.DeactivateAllButtons();
        //        SessionController.Instance.SwitchMode(Options.PanZoomOnly);
        //    }
        //    else
        //    {
        //        xFloatingMenu.Visibility = Visibility.Visible;
        //    }



        //}



        //private void Presentation_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (sender == xPresentation)
        //    {
        //        ExitMode();
        //        if (IsReadonly)
        //        {
        //            xReadonlyFloatingMenu.Visibility = Visibility.Visible;
        //        }
        //        return;
        //    }

        //    if (sender == NextNode)
        //    {

        //        _modeInstance.MoveToNext();
        //    }

        //    /*
        //        if (!IsPenMode)
        //            return;
        //        _activeFreeFormViewer.SwitchMode(Options.SelectNode, false);
        //        _prevOptions = Options.SelectNode;
        //        IsPenMode = false;
        //        xBtnPen.BorderBrush = new SolidColorBrush(Constants.color4);
        //        PenCircle.Background = new SolidColorBrush(Constants.color4);
        //        */

        //    if (sender == PreviousNode)
        //    {
        //        _modeInstance.MoveToPrevious();
        //    }

        //    if (sender == CurrentNode)
        //    {
        //        _modeInstance.GoToCurrent();
        //    }

        //    // only show next and prev buttons if next and prev nodes exist
        //    SetModeButtons();

        //}

        //private void SetModeButtons()
        //{

        //    if (_modeInstance.Next())
        //    {
        //        NextNode.Opacity = 1;
        //        NextNode.Click -= Presentation_OnClick;
        //        NextNode.Click += Presentation_OnClick;
        //    }
        //    else
        //    {
        //        NextNode.Opacity = 0.6;
        //        NextNode.Click -= Presentation_OnClick;
        //    }
        //    if (_modeInstance.Previous())
        //    {
        //        PreviousNode.Opacity = 1;
        //        PreviousNode.Click -= Presentation_OnClick;
        //        PreviousNode.Click += Presentation_OnClick;
        //    }
        //    else
        //    {
        //        PreviousNode.Opacity = 0.6;
        //        PreviousNode.Click -= Presentation_OnClick;
        //    }
        //}

        

        //public void ToggleVisualLinks(object sender, RoutedEventArgs e)
        //{
        //    if (SessionController.Instance.LinksController.AreBezierLinksVisible)
        //    {
        //        SessionController.Instance.LinksController.ChangeVisualLinkVisibility(false);
        //    }
        //    else
        //    {
        //        SessionController.Instance.LinksController.ChangeVisualLinkVisibility(true);
        //    }
        //}

        

        //private void Resize(object sender, SizeChangedEventArgs e)
        //{
        //    UITask.Run(() =>
        //    {
        //        //Users.Height = 50;
        //        //Canvas.SetLeft(Users, 75);
        //        //Canvas.SetTop(Users, mainCanvas.ActualHeight - 61);
        //        //Canvas.SetTop(ChatPopup, mainCanvas.ActualHeight - 70 - ChatPopup.ActualHeight);
        //        //Canvas.SetLeft(ChatPopup, 5);
        //        ////Canvas.SetLeft(ChatButton, 5);
        //        ////Canvas.SetTop(ChatButton, mainCanvas.ActualHeight - 70);
        //        //Canvas.SetLeft(xReadonlyFloatingMenu, mainCanvas.ActualWidth/2 - xReadonlyFloatingMenu.ActualWidth/2);
        //        //Canvas.SetTop(xReadonlyFloatingMenu, mainCanvas.ActualHeight - xReadonlyFloatingMenu.ActualHeight - 20);
        //        //Canvas.SetLeft(xChatBox, 15);
        //        //Canvas.SetTop(xChatBox, mainCanvas.ActualHeight - 375 - 10 - 50 - 10 - 7);

        //        //Canvas.SetTop(BtnBack, (mainCanvas.ActualHeight - BtnBack.ActualHeight)/2);
        //    });
        //}

        /// <summary>
        /// Takes in the library element controller of whatever you want to show in the detail view. (this is the current method)
        /// </summary>
        /// <param name="viewable"></param>
        /// <param name="tabToOpenTo"></param>
        public async void ShowDetailView(LibraryElementController viewable, DetailViewTabType tabToOpenTo = DetailViewTabType.Home)
        {

            FreeFormViewer.DetailViewer.ShowLibraryElement(viewable.LibraryElementModel.LibraryElementId);

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

        public ReadonlyFloatingMenuView ReadonlyFloatingMenu
        {
            get { return xReadonlyFloatingMenu; }
        }

        public Canvas MainCanvas
        {
            get { return mainCanvas; }
        }

        public DetailViewerView DetailViewerView
        {
            get { return xDetailViewer; }
        }

        //private void ChatButton_OnClick(object sender, RoutedEventArgs e)
        //{
        //    //initChatNotifs = ChatPopup.getTexts().Count;
        //    ChatPopup.Visibility = ChatPopup.Visibility == Visibility.Collapsed
        //        ? Visibility.Visible
        //        : Visibility.Collapsed;
        //    if (ChatPopup.Visibility == Visibility.Visible)
        //    {
        //        Canvas.SetTop(ChatPopup, mainCanvas.ActualHeight - 70 - ChatPopup.ActualHeight);
        //        Canvas.SetLeft(ChatPopup, 5);
        //        ChatPopup.ClearNewTexts();
        //    }
        //}


        //private void MenuVisibility(object sender, DoubleTappedRoutedEventArgs e)
        //{
        //    if (FloatingMenu.Visibility == Visibility.Collapsed)
        //    {
        //        Point pos = e.GetPosition(mainCanvas);
        //        Canvas.SetTop(FloatingMenu, pos.Y);
        //        Canvas.SetLeft(FloatingMenu, pos.X);
        //        FloatingMenu.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        FloatingMenu.Visibility = Visibility.Collapsed;
        //    }
        //}
        public Grid OuterMost { get { return xOuterMost; } }
        public FreeFormViewer FreeFormViewer { get { return _activeFreeFormViewer; } }

        private async void PrevCollectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            await SessionController.Instance.EnterCollection(_prevCollectionLibraryId);
            PrevCollection.Visibility = Visibility.Collapsed;
        }
        //private async void SnapshotButton_OnClick(object sender, RoutedEventArgs e)
        //{
        //    //await StaticServerCalls.CreateSnapshot();
        //    CreateSnapshotOfCollectionRequest request = new CreateSnapshotOfCollectionRequest(SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementController.LibraryElementModel.LibraryElementId);
        //    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
        //    request.AddSnapshotCollectionLocally();
        //}
        //private void CurrentCollectionDV_OnClick(object sender, RoutedEventArgs e)
        //{
        //    xDetailViewer.ShowElement(SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementController);
        //}

        internal SearchView SearchView
        {
            get { return xSearchViewer; }
        }
        /// <summary>
        /// Removes the related list box from the sv
        /// </summary>
        //public void RemoveRelatedListBox()
        //{
        //    var exp = _modeInstance as ExplorationMode;
        //    exp.HideRelatedListBox();

        //}

        /// <summary>
        /// Retreives the chat box instance, returns null if the chatbox has not been instantiated yet
        /// </summary>
        /// <returns></returns>
        //public ChatBoxView GetChatBox()
        //{
        //    return xChatBox;
        //}
        /// <summary>
        /// Changes the workspace to readonly if it is editable. Changes the workspace to editable if it is readonly.
        /// </summary>
        public void ToggleReadonly()
        {
            if (this.IsReadonly)
            {
                this.MakeWorkspaceEditable();
            }
            else
            {
                this.MakeWorkspaceReadonly();
            }
        }

        //private async void GoBackToWaitingRoom_OnClick(object sender, RoutedEventArgs e)
        //{
        //    SessionController.Instance.ClearControllersForCollectionExit();

        //    await WaitingRoomView.Instance.ShowWaitingRoom();
            
        //}

        /// <summary>
        /// method called to clear all the user labels currently on the session view;
        /// </summary>
        //public void ClearUsers()
        //{
        //    Users?.Children?.Clear();
        //    WaitingRoomView.Instance.ClearUsers();
        //}
    }
}