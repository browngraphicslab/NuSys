﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;

using NuSysApp.Components.NuSysRenderer.UI.Textbox;

using NusysIntermediate;

using NuSysApp.Network.Requests;
using ReverseMarkdown.Converters;
using WinRTXamlToolkit.Controls.DataVisualization;

namespace NuSysApp
{
    public class NuSessionViewer : RectangleUIElement
    {
        /// <summary>
        /// The floating menu has the library and the add elements to workspace buttons
        /// </summary>
        private FloatingMenu _floatingMenu;

        /// <summary>
        /// The chat button shows the chatbox when clicked
        /// </summary>
        private ButtonUIElement _chatButton;

        /// <summary>
        /// The snapshot button takes a snapshot of the workspace
        /// </summary>
        private ButtonUIElement _snapshotButton;

        /// <summary>
        /// button for the settings of the session
        /// </summary>
        private ButtonUIElement _settingsButton;

        /// <summary>
        /// Various windows that appear in read only mode.
        /// </summary>
        private ReadOnlyLinksWindow _readOnlyLinksWindow;

        private ReadOnlyMetadataWindow _readOnlyMetadataWindow;

        private ReadOnlyAliasesWindow _readOnlyAliasesWindow;

        /// <summary>
        /// The menu UI for the settings of the session
        /// </summary>
        private SessionSettingsMenu _settingsMenu;

        /// <summary>
        /// the chatbox that we use to send messages to eachother
        /// </summary>
        public ChatBoxUIElement Chatbox { get; }
        
        private ButtonUIElement _backButton;

        /// <summary>
        /// container that contains bubbles that show the currently logged in users
        /// </summary>
        private UserBubbleContainerUIElement _userBubbleContainer;

        /// <summary>
        ///  the detail viewer
        /// </summary>
        private DetailViewMainContainer _detailViewer;

        /// <summary>
        /// the trailbox of previious visited elements
        /// </summary>
        public BreadCrumbContainer TrailBox;

        /// <summary>
        ///  the tite bxo used to display the title of the workspace
        /// </summary>
        public TextboxUIElement _titleBox;

        /// <summary>
        /// Button tapped to exit presentation mode
        /// </summary>
        private ButtonUIElement _exitPresentation;

        /// <summary>
        /// button tapped to recenter on the current node in presentation mode
        /// </summary>
        private ButtonUIElement _currentNode;

        /// <summary>
        /// button tapped to go to the next node in presentation mode
        /// </summary>
        private ButtonUIElement _nextNode;

        /// <summary>
        /// button tapped to go to the previous node in presentation mode
        /// </summary>
        private ButtonUIElement _previousNode;

        /// <summary>
        /// the current mode we are in
        /// </summary>
        private IModable _modeInstance;

        /// <summary>
        /// the library list 
        /// </summary>
        public LibraryListUIElement Library { get; private set; }

        /// <summary>
        /// Rectangle used to display confirmation message to go back to the waiting room
        /// </summary>
        private RectangleButtonUIElement _backToWaitingRoom;

        /// <summary>
        /// true if the nusessionviewer has been loaded already
        /// </summary>
        private bool _loaded;

        /// <summary>
        /// Dynamic textbox used to display chat notifications
        /// </summary>
        private DynamicTextboxUIElement _chatButtonNotifications;

        /// <summary>
        /// The current number of chat notifications
        /// </summary>
        private int _numChatNotifications;
        
        /// <summary>
        /// Keeps track of the last opened library element so that the same one does not cause the windows to reopen.
        /// </summary>
        private LibraryElementController _readOnlyController;

        /// <summary>
        /// the controller of the current collection
        /// </summary>
        private LibraryElementController _currController;


        public NuSessionViewer(BaseRenderItem parent, CanvasAnimatedControl canvas) : base(parent, canvas)
        {
            Background = Colors.Transparent;
            SessionController.Instance.NuSessionView = this; // set the session controller's getter for the NuSessionView

            _floatingMenu = new FloatingMenu(this, canvas);
            AddChild(_floatingMenu);

            SessionController.Instance.EnterNewCollectionCompleted += InstanceOnEnterNewCollectionCompleted;
            

            _settingsButton = new EllipseButtonUIElement(this, canvas)
            {
                Background = Colors.Transparent
            };
            AddChild(_settingsButton);

            _settingsMenu = new SessionSettingsMenu(this, canvas)
            {
                Width = 250,
                Height = 250,
                Background = Constants.LIGHT_BLUE,
                IsVisible =  false,
                KeepAspectRatio = false
            };
            AddChild(_settingsMenu);

            _titleBox = new TextboxUIElement(this, Canvas)
            {
                TextColor = Constants.ALMOST_BLACK,
                Background = Colors.Transparent,
                FontSize = 35,
                TrimmingGranularity = CanvasTextTrimmingGranularity.Character,
                Width = 300,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                FontFamily = UIDefaults.TitleFont
            };
            AddChild(_titleBox);

            _chatButton = new EllipseButtonUIElement(this, canvas, UIDefaults.AccentStyle);
            AddChild(_chatButton);

            // add the chatbutton notifications
            _chatButtonNotifications = new DynamicTextboxUIElement(this, Canvas)
            {
                IsVisible = false,
                Height = 25,
                Background = Colors.Red,
                TextColor = Colors.White,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center

            };
            AddChild(_chatButtonNotifications);


            _snapshotButton = new EllipseButtonUIElement(this, canvas, UIDefaults.AccentStyle, "snapshot");
            AddChild(_snapshotButton);

            //custom button
            _backButton = new ButtonUIElement(this, canvas, new RectangleUIElement(this, canvas))
            {
                Width = 15,
                Height = 30,
                SelectedBackground = Constants.LIGHT_BLUE_TRANSLUCENT,
                BorderWidth =  0,
                BorderColor = Colors.Transparent,
                Background = Colors.Transparent
            };
            _backButton.ImageBounds = new Rect(.1, .1, .8, .8);
            AddChild(_backButton);

            TrailBox = new BreadCrumbContainer(this, Canvas)
            {
                IsVisible = SessionController.Instance.SessionSettings.BreadCrumbsDocked
            };
            AddChild(TrailBox);

            _backToWaitingRoom = new RectangleButtonUIElement(this, canvas, UIDefaults.PrimaryStyle, "back to lobby")
            {
                Width = TrailBox.Width,
                IsVisible = false
            };
            AddChild(_backToWaitingRoom);

            // add the user bubble container before the chatbox so user bubble names do not overlap the bottom of the chatbox
            _userBubbleContainer = new UserBubbleContainerUIElement(this, canvas);
            AddChild(_userBubbleContainer);

            // add the chatbox after the user bubble container so user bubble names do not overlap the bottom of the chatbox
            Chatbox = new ChatBoxUIElement(this, canvas)
            {

                IsVisible = false,
            };
            AddChild(Chatbox);

            _detailViewer = new DetailViewMainContainer(this, Canvas)
            {
                Width = 500,
                Height = 500,
                MinWidth = 400,
                MinHeight = 600,
                KeepAspectRatio = false
            };
            AddChild(_detailViewer);


            // add presentation node buttons
            _previousNode = new EllipseButtonUIElement(this, canvas, UIDefaults.SecondaryStyle)
            {
                IsVisible = false
            };
            AddChild(_previousNode);
            _nextNode = new EllipseButtonUIElement(this, canvas, UIDefaults.SecondaryStyle)
            {
                IsVisible = false
            };
            AddChild(_nextNode);
            _currentNode = new EllipseButtonUIElement(this, canvas, UIDefaults.SecondaryStyle)
            {
                IsVisible = false,
            };
            AddChild(_currentNode);
            _exitPresentation = new EllipseButtonUIElement(this, canvas, UIDefaults.SecondaryStyle)
            {
                IsVisible = false
            };
            AddChild(_exitPresentation);

            UpdateUI();

            _titleBox.DoubleTapped += _titleBox_DoubleTapped;

            _readOnlyLinksWindow = new ReadOnlyLinksWindow(this, Canvas)
            {
                Background = Constants.LIGHT_BLUE,
                Height = 300,
                Width = 250
            };
            AddChild(_readOnlyLinksWindow);

            _readOnlyMetadataWindow = new ReadOnlyMetadataWindow(this, Canvas)
            {
                Background = Constants.LIGHT_BLUE,
                Height = 300,
                Width = 250
            };

            AddChild(_readOnlyMetadataWindow);
            _readOnlyMetadataWindow.Transform.LocalPosition = new Vector2(60, 450);

            _readOnlyAliasesWindow = new ReadOnlyAliasesWindow(this, Canvas)
            {
                Background = Constants.LIGHT_BLUE,
                Height = 300,
                Width = 250
            };
            AddChild(_readOnlyAliasesWindow);
            _readOnlyAliasesWindow.Transform.LocalPosition = new Vector2(60, 100);


            Canvas.SizeChanged += OnMainCanvasSizeChanged;
            //_currCollDetailViewButton.Tapped += OnCurrCollDetailViewButtonTapped;
            _snapshotButton.Tapped += SnapShotButtonTapped; 
            _chatButton.Tapped += ChatButtonOnTapped;
            _backButton.Tapped += BackTapped;
            _backToWaitingRoom.Tapped += BackToWaitingRoomOnTapped;
            _settingsButton.Tapped += SettingsButtonOnTapped;

            SessionController.Instance.OnModeChanged += Instance_OnModeChanged;
        }

        /// <summary>
        /// shows the search result pop up for when the search results have completed loading
        /// </summary>
        public void ShowSearchResultPopup(List<LibraryElementController> elements, string searchTerm)
        {
            var popup = new SearchResultsPopup(this, Canvas, elements, searchTerm);
            AddChild(popup);
        }

        /// <summary>
        /// shows popup that tells user they cannot put a private element on a public collection.
        /// this way when they do that they don't just end up wondering why nothing happened (gotta have that visual feedback yo).
        /// </summary>
        public void ShowPrivateOnPublicPopup()
        {
            var popup = new CenteredPopup(this, Canvas, "You cannot put a private element on a collection that is not private.");
            AddChild(popup);
        }

        public void ShowDraggingCollectionToCollectionPopup()
        {
            var popup = new CenteredPopup(this, Canvas, "You cannot put this collection on itself.");
            AddChild(popup);
        }
        /// <summary>

        /// show both the breadcrumb trail window and the back to waiting room button
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void BackTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // toggle the back to waiting room rectangle visibility
            _backToWaitingRoom.IsVisible = !_backToWaitingRoom.IsVisible;

            // if the bread crumb trail is going to be viewed in the back to waiting room rect
            // assign it the same visibility as the back to waiting room rect
            if (!SessionController.Instance.SessionSettings.BreadCrumbsDocked)
            {
                TrailBox.IsVisible = _backToWaitingRoom.IsVisible;
            }
        }

        /// Called whenever the mode is changed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="mode"></param>
        private  void Instance_OnModeChanged(object source, Options mode)
        {
            switch (mode)
            {
                case Options.PanZoomOnly:
                    _floatingMenu.IsVisible = true;
                    if (!SessionController.Instance.SessionSettings.BreadCrumbsDocked)
                    {
                        TrailBox.IsVisible = _backToWaitingRoom.IsVisible;
                    }
                    else
                    {
                        TrailBox.IsVisible = true;
                    }
                    MakeEditable();
                    break;
                case Options.Presentation:
                    _detailViewer.IsVisible = false;
                    _floatingMenu.IsVisible = false;
                    if (!SessionController.Instance.SessionSettings.BreadCrumbsDocked)
                    {
                        TrailBox.IsVisible = _backToWaitingRoom.IsVisible;
                    }
                    else
                    {
                        TrailBox.IsVisible = false;
                    }
                    break;
                case Options.ReadOnly:
                    MakeReadOnly();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

           
        }

        private void CameraCenteredOnElement(object sender, LibraryElementController e)
        {
            if (e == _readOnlyController)
            {
                return;
            }
            _readOnlyLinksWindow.UpdateList(e);
            _readOnlyAliasesWindow.UpdateList(e);
            _readOnlyMetadataWindow.UpdateList(e);
            _readOnlyLinksWindow.IsVisible = true;
            _readOnlyMetadataWindow.IsVisible = true;
            _readOnlyAliasesWindow.IsVisible = true;
            _readOnlyController = e;
        }

        /// <summary>
        /// open detail view for present collection if you double tap the title box
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _titleBox_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currCollectionController = SessionController.Instance.ContentController.GetLibraryElementController(SessionController.Instance.CurrentCollectionLibraryElementModel.LibraryElementId);
            SessionController.Instance.NuSessionView.ShowDetailView(currCollectionController);
        }

        /// <summary>
        /// need to set the title box text here, since otherwise it will throw an exception
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="s"></param>
        private void InstanceOnEnterNewCollectionCompleted(object sender, string s)
        {
            _titleBox.Text = SessionController.Instance.CurrentCollectionLibraryElementModel.Title;

            if (SessionController.Instance.CurrentCollectionLibraryElementModel.AccessType ==
                NusysConstants.AccessType.ReadOnly)
            {
                var currCollectionController = SessionController.Instance.ContentController.GetLibraryElementController(
                    SessionController.Instance.CurrentCollectionLibraryElementModel.LibraryElementId);
                _readOnlyMetadataWindow.UpdateList(currCollectionController);
                _readOnlyAliasesWindow.UpdateList(currCollectionController);
                _readOnlyLinksWindow.UpdateList(currCollectionController);
            }

            // if we were already in a workspace then the curr controller was set previously so remove the event here
            if (_currController != null)
            {
                _currController.TitleChanged -= _currController_TitleChanged;
            }

            // set the curr controller to the new controller, and add the title changed event
            _currController = SessionController.Instance.ContentController.GetLibraryElementController(
                SessionController.Instance.CurrentCollectionLibraryElementModel.LibraryElementId);
            _currController.TitleChanged += _currController_TitleChanged; ;
        }

        /// <summary>
        /// Called when the controller for the current collection's title changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _currController_TitleChanged(object sender, string e)
        {
            _titleBox.Text = e;
        }

        /// <summary>
        /// updates UI that is dependent on the position of the titlebox
        /// </summary>
        public void UpdateUI()
        {
            // update title box position
            _titleBox.Transform.LocalPosition =
            new Vector2(SessionController.Instance.NuSessionView.Width / 2 - _titleBox.Width / 2, 0);
            
            //update settings and back button position to be on either side of the title box
            _settingsButton.Transform.LocalPosition = new Vector2(SessionController.Instance.NuSessionView.Width / 2 + _titleBox.Width / 2 - _settingsButton.Width / 2 + 50,
            _titleBox.Height / 2 - _settingsButton.Height / 2);
            _backButton.Transform.LocalPosition = new Vector2(SessionController.Instance.NuSessionView.Width / 2 - _titleBox.Width / 2 - _settingsButton.Width / 2 - 50,
                _titleBox.Height / 2 - _backButton.Height / 2);
            
            //if the breadcrumb trail is not docked, then update it to be aligned with the back button, and update the back to waiting room button
            if (!SessionController.Instance.SessionSettings.BreadCrumbsDocked)
            {
                TrailBox.Transform.LocalPosition =
                    new Vector2(_backButton.Transform.LocalPosition.X + _backButton.Width/2 - TrailBox.Width/2,
                        _backButton.Transform.LocalPosition.Y + _backButton.Height + 15);
                _backToWaitingRoom.Transform.LocalPosition =
                    new Vector2(
                        _backButton.Transform.LocalPosition.X + _backButton.Width/2 - _backToWaitingRoom.Width/2,
                        _backButton.Transform.LocalPosition.Y + _backButton.Height + TrailBox.Height + 30);
            }
            //if the breadcrumb trail is docked, update it to be right aligned, and update the back to waiting room button to be aligned with the back button
            else
            {
                TrailBox.Transform.LocalPosition =
                    new Vector2(SessionController.Instance.NuSessionView.Width - TrailBox.Width, 0);
                _backToWaitingRoom.Transform.LocalPosition =
                    new Vector2(
                        _backButton.Transform.LocalPosition.X + _backButton.Width / 2 - _backToWaitingRoom.Width / 2,
                        _backButton.Transform.LocalPosition.Y + _backButton.Height + 15);
            }

            //trail box is visible dependent on the back to waiting room button
            if (!SessionController.Instance.SessionSettings.BreadCrumbsDocked)
            {
                TrailBox.IsVisible = _backToWaitingRoom.IsVisible;
            }
            else
            {
                TrailBox.IsVisible = true;
            }

            //set position of settings menu
            _settingsMenu.Transform.LocalPosition = new Vector2(_settingsButton.Transform.LocalPosition.X + _settingsButton.Width / 2 - _settingsMenu.Width / 2, _settingsButton.Height + _settingsButton.Transform.LocalPosition.Y + 15);

        }



        /// <summary>
        /// Method to call to make the current workspace have the read-only ui.
        /// This should mainly hide things like the floating menu.
        /// </summary>
        public void MakeReadOnly()
        {
            _readOnlyLinksWindow.IsVisible = true;
            _readOnlyAliasesWindow.IsVisible = true;
            _readOnlyMetadataWindow.IsVisible = true;
            _detailViewer.HideDetailView();
            _detailViewer.DisableDetailView();
            _floatingMenu.HideFloatingMenu();
            _floatingMenu.IsVisible = false;
            _readOnlyLinksWindow.Transform.LocalPosition = new Vector2(SessionController.Instance.NuSessionView.Width - _readOnlyLinksWindow.Width - 20, 100);
            SessionController.Instance.SessionView.FreeFormViewer.CanvasPanned += CanvasPanned;
            SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.CameraOnCentered += CameraCenteredOnElement;
            if (Library != null)
            {
                Library.IsVisible = false;
            }
        }

        /// <summary>
        /// Method to call to undo the MakeReadOnly method and reapply the editable UI.
        /// </summary>
        public void MakeEditable()
        {
            _readOnlyLinksWindow.IsVisible = false;
            _readOnlyAliasesWindow.IsVisible = false;
            _readOnlyMetadataWindow.IsVisible = false;
            _detailViewer.EnableDetailView();
            _floatingMenu.ShowFloatingMenu();

            SessionController.Instance.SessionView.FreeFormViewer.CanvasPanned -= CanvasPanned;
            SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.CameraOnCentered -= CameraCenteredOnElement;
            _detailViewer.EnableDetailView();
        }

        /// <summary>
        /// the event handler for when the settings button is tapped.  should simply toggle the settings menu
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void SettingsButtonOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _settingsMenu.IsVisible = !_settingsMenu.IsVisible;
        }

        /// <summary>
        /// Fired whenever the chat button is tapped on
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ChatButtonOnTapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            Chatbox.IsVisible = !Chatbox.IsVisible;
            if (Chatbox.IsVisible)
            {
                Chatbox.Height = Math.Min(Height - 100, Chatbox.Height);
                Chatbox.Width = Math.Min(Width - 100, Chatbox.Width);
                Chatbox.Transform.LocalPosition = new Vector2(10, Height - Chatbox.Height - 70);
                _chatButtonNotifications.IsVisible = false;
                _numChatNotifications = 0;
            }
        }

        private void OnMainCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Width = (float) e.NewSize.Width;
            Height = (float) e.NewSize.Height;

            _floatingMenu.Transform.LocalPosition = new Vector2(Width/4 - _floatingMenu.Width/2, Height/4 - _floatingMenu.Height/2);
            //_currCollDetailViewButton.Transform.LocalPosition = new Vector2(Width - _currCollDetailViewButton.Width - 10, 10);
            _chatButton.Transform.LocalPosition = new Vector2(10, Height - _chatButton.Height - 10);
            _chatButtonNotifications.Transform.LocalPosition = new Vector2((float) (_chatButton.Transform.LocalX + _chatButton.Width/2 + _chatButton.Width/2 * Math.Cos(.25 * Math.PI)), (float) (_chatButton.Transform.LocalY + _chatButton.Height/2 - _chatButton.Height/2 * Math.Sin(.25 * Math.PI) - _chatButtonNotifications.Height));
            _snapshotButton.Transform.LocalPosition = new Vector2(10, 10);
            _settingsButton.Transform.LocalPosition = new Vector2(80, 10);
            _backButton.Transform.LocalPosition = new Vector2(10, Height/2 - _backButton.Height/2);
            Chatbox.Transform.LocalPosition = new Vector2(10, Height - Chatbox.Height - 70);
            _backToWaitingRoom.Transform.LocalPosition = new Vector2(10, Height/2 - _backToWaitingRoom.Height/2);
            _userBubbleContainer.Transform.LocalPosition = new Vector2(_chatButton.Transform.LocalPosition.X + _chatButton.Width + 10, Height - _userBubbleContainer.Height - 10);
            _detailViewer.Transform.LocalPosition = new Vector2(Width/2, 0);
            _detailViewer.Height = Height;
            _detailViewer.Width = Width/2;
            _detailViewer.MaxWidth = Width - 60;

            // center the presentation mode buttons
            var buttonMargin = 10;
            var top = Height - _previousNode.Height - buttonMargin;
            var buttonWidth = _previousNode.Width;
            float left = (float) ((Width - buttonMargin)/2.0 - 2*buttonWidth - buttonMargin);
            var buttonDiff = buttonWidth + buttonMargin;
            foreach (var button in new List<ButtonUIElement> {_previousNode, _nextNode, _currentNode, _exitPresentation})
            {
                button.Transform.LocalPosition = new Vector2(left, top);
                left += buttonDiff;
            }

            UpdateUI();
        }

        /// <summary>
        /// This load call and consequently all load calls for children of the nusessionviewer is fired every time the user enters a new collection,
        /// sometimes this can lead to undesired behavior for instance when objects are instantaited twice, or when images are loaded twice. To avoid
        /// this behavior null check objects and images in the load method of children of the nusessionviewer
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            if (_loaded)
            {
                return;
            }

            // set the image for the _chatButton
            _chatButton.Image = _chatButton.Image ?? await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/icon_chat.png"));
            _chatButton.ImageBounds = new Rect(.25, .25, .5, .5);

            // set the image for the _snapshotButton
            _snapshotButton.Image = _snapshotButton.Image ?? await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/snapshot_icon.png"));
            _snapshotButton.ImageBounds = new Rect(.25, .25, .5, .5);

            //load and set the settings icon
            _settingsButton.Image = _settingsButton.Image ?? await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/gear.png"));
            _settingsButton.ImageBounds = new Rect(.25, .25, .5, .5);

            // set the image for the _backButton
            _backButton.Image = _backButton.Image ?? await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/back.png"));

            // set the images for presentation mode
            _nextNode.Image = _nextNode.Image ?? await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/presentation_forward.png"));
            _previousNode.Image = _previousNode.Image ?? await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/presentation_backward.png"));
            _currentNode.Image = _currentNode.Image ?? await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/return to node.png"));
            _exitPresentation.Image = _exitPresentation.Image ?? await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/x white.png"));


            // created here because it must be created after the create resources method is called on the main canvas animated control
            if (Library == null)
            {
                Library = new LibraryListUIElement(this, Canvas);
                Library.KeepAspectRatio = false;
                AddChild(Library);
                Library.Transform.LocalPosition = new Vector2(_floatingMenu.Transform.LocalX + _floatingMenu.Width, _floatingMenu.Transform.LocalY + _floatingMenu.Height);
            }
            Library.IsVisible = false;

            _loaded = true;

            base.Load();
        }

        public override void Dispose()
        {
            Debug.Assert(_settingsButton != null);
            if (_settingsButton != null)
            {
                _settingsButton.Tapped -= SettingsButtonOnTapped;
            }
            if (_titleBox != null)
            {
                _titleBox.DoubleTapped -= _titleBox_DoubleTapped;
            }
            Canvas.SizeChanged -= OnMainCanvasSizeChanged;
            _snapshotButton.Tapped -= SnapShotButtonTapped;
            _chatButton.Tapped -= ChatButtonOnTapped;
            _backButton.Tapped -= BackTapped;
            _backToWaitingRoom.Tapped -= BackToWaitingRoomOnTapped;
            SessionController.Instance.OnModeChanged -= Instance_OnModeChanged;
            SessionController.Instance.EnterNewCollectionCompleted -= InstanceOnEnterNewCollectionCompleted;
            SessionController.Instance.SessionView.FreeFormViewer.CanvasPanned -= CanvasPanned;
            SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.CameraOnCentered -= CameraCenteredOnElement;
            SessionController.Instance.SessionView.FreeFormViewer.CanvasPanned -= TempReadOnlyCanvasPanned;
            _detailViewer.NewLibraryElementShown -= DetailViewerOnNewLibraryElementShown;
            base.Dispose();
        }

        /// <summary>
        /// Fired whenever the snapshot button is pressed, takes a snapshot of the current workspace
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private async void SnapShotButtonTapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            CreateSnapshotOfCollectionRequest request = new CreateSnapshotOfCollectionRequest(SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementController.LibraryElementModel.LibraryElementId);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddSnapshotCollectionLocally();
            var popup = new CenteredPopup(this, Canvas, "Your snapshot has been added, called " + SessionController.Instance.CurrentCollectionLibraryElementModel.Title + " Snapshot.");
            AddChild(popup);
            
        }

        /// <summary>
        /// Fired whenever the waiting room button is clicked, returns the user to the waiting room
        /// </summary>
        private async void BackToWaitingRoomOnTapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            _backToWaitingRoom.IsVisible = false;
            // for now this has to be done on the ui thread because it deals with xaml elements
            UITask.Run(async () =>
            {
                SessionController.Instance.ClearControllersForCollectionExit();

                await WaitingRoomView.Instance.ShowWaitingRoom();
            });

        }

        /// <summary>
        /// Takes in the library element controller of whatever you want to show in the detail view. (this is the current method)
        /// </summary>
        /// <param name="viewable"></param>
        /// <param name="tabToOpenTo"></param>
        public void ShowDetailView(LibraryElementController viewable, DetailViewTabType tabToOpenTo = DetailViewTabType.Home)
        {
            Debug.Assert(viewable != null);
            Debug.Assert(!viewable.LibraryElementModel.ViewInReadOnly());
            if (viewable.LibraryElementModel.ViewInReadOnly()) //if we don't have access rights to this, return
            {
                return;
            }
            _detailViewer.ShowLibraryElement(viewable.LibraryElementModel.LibraryElementId);
        }

        /// <summary>
        /// Call this method to enter presentation mode
        /// </summary>
        /// <param name="elementViewModel"></param>
        public void EnterPresentationMode(ElementViewModel elementViewModel)
        {
            Debug.Assert(elementViewModel != null);

            // Don't do anything if we're already in presentation mode
            if (_modeInstance?.Mode == ModeType.PRESENTATION)
            {
                return;
            }
            _modeInstance = new PresentationMode(elementViewModel);
            SessionController.Instance.SwitchMode(Options.Presentation);
            _nextNode.IsVisible = true;
            _previousNode.IsVisible = true;
            _currentNode.IsVisible = true;
            _exitPresentation.IsVisible = true;

            _exitPresentation.Tapped += Presentation_OnClick;
            _currentNode.Tapped += Presentation_OnClick;

            // set the buttons
            SetModeButtons();
        }

        /// <summary>
        /// public method to essentially reset the z-index of children windows.
        /// This will allow the focused window to be the top window.
        /// </summary>
        /// <param name="window"></param>
        public void MakeTopWindow(WindowUIElement window)
        {
            Debug.Assert(window?.Parent != null);
            Debug.Assert(window.Parent == this);
            Debug.Assert(_children.Contains(window));
            if(window.Parent == this && _children.Contains(window))
            {
                _children.Remove(window);
                _children.Add(window);
            }

        }

        /// <summary>
        /// Method called when a presentation button is clicked
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void Presentation_OnClick(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (item == _exitPresentation)
            {
                ExitMode();
                //if (IsReadonly)
                //{
                //    xReadonlyFloatingMenu.Visibility = Visibility.Visible;
                //}
                return;
            }

            if (item == _nextNode)
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

            if (item == _previousNode)
            {
                _modeInstance.MoveToPrevious();
            }

            if (item == _currentNode)
            {
                _modeInstance.GoToCurrent();
            }

            // only show next and prev buttons if next and prev nodes exist
            SetModeButtons();
        }

        /// <summary>
        /// Set the active or inactive state of presentation buttons
        /// </summary>
        private void SetModeButtons()
        {
            if (_modeInstance.Next())
            {
                //_nextNode.Opacity = 1;
                _nextNode.Tapped -= Presentation_OnClick;
                _nextNode.Tapped += Presentation_OnClick;
            }
            else
            {
                //_nextNode.Opacity = 0.6;
                _nextNode.Tapped -= Presentation_OnClick;
            }
            if (_modeInstance.Previous())
            {
                //_previousNode.Opacity = 1;
                _previousNode.Tapped -= Presentation_OnClick;
                _previousNode.Tapped += Presentation_OnClick;
            }
            else
            {
                //_previousNode.Opacity = 0.6;
                _previousNode.Tapped -= Presentation_OnClick;
            }
        }

        /// <summary>
        /// Exits either presentation or exploration mode by modifying the proper UI elements
        /// </summary>
        public void ExitMode()
        {
            _modeInstance.ExitMode();
            _modeInstance = null;
            _nextNode.IsVisible = false;
            _previousNode.IsVisible = false;
            _currentNode.IsVisible = false;
            _exitPresentation.IsVisible = false;

            //// Make sure to make appropriate changes based on whether or not we are in read only mode
            //if (this.IsReadonly)
            //{
            //    xReadonlyFloatingMenu.Visibility = Visibility.Visible;
            //    xReadonlyFloatingMenu.DeactivateAllButtons();
            //    SessionController.Instance.SwitchMode(Options.PanZoomOnly);
            //}
            //else
            //{
            //    xFloatingMenu.Visibility = Visibility.Visible;
            //}

            if (SessionController.Instance.CurrentMode == Options.Presentation)
            {
                _exitPresentation.Tapped -= Presentation_OnClick;
                _currentNode.Tapped -= Presentation_OnClick;
            }

            SessionController.Instance.SwitchMode(Options.PanZoomOnly);
        }

        /// <summary>
        /// Displays the passed in notification in the chat icon
        /// </summary>
        /// <param name="notification"></param>
        public void IncrementChatNotifications()
        {
            if (!Chatbox.IsVisible)
            {
                _numChatNotifications += 1;
                _chatButtonNotifications.Text = _numChatNotifications.ToString();
                _chatButtonNotifications.IsVisible = true;
            }
        }

        /// <summary>
        /// This is a listener for the Canvas Panned event. In read only mode if the windows are only made 
        /// visible on focus, this hides the various windows.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasPanned(object sender, bool e)
        {
            if (SessionController.Instance.SessionSettings.ReadOnlyModeWindowsVisible ==
                ReadOnlyViewingMode.VisibleOnFocus)
            {
                _readOnlyLinksWindow.IsVisible = false;
                _readOnlyAliasesWindow.IsVisible = false;
                _readOnlyMetadataWindow.IsVisible = false;
            }

        }

        /// <summary>
        /// Method to call in a public collection when an item is double tapped
        /// </summary>
        /// <param name="elementToFocus"></param>
        public void ShowReadOnlyWindows(ElementModel elementToFocus)
        {
            Debug.Assert(elementToFocus?.LibraryId != null);
            var libraryElement = SessionController.Instance.ContentController.GetLibraryElementController(elementToFocus.LibraryId);

            _readOnlyAliasesWindow.UpdateList(libraryElement);
            _readOnlyLinksWindow.UpdateList(libraryElement);
            _readOnlyMetadataWindow.UpdateList(libraryElement);

            _readOnlyLinksWindow.IsVisible = true;
            _readOnlyAliasesWindow.IsVisible = true;
            _readOnlyMetadataWindow.IsVisible = true;

            _detailViewer.HideDetailView();

            _detailViewer.NewLibraryElementShown -= DetailViewerOnNewLibraryElementShown;
            _detailViewer.NewLibraryElementShown += DetailViewerOnNewLibraryElementShown;

            SessionController.Instance.SessionView.FreeFormViewer.CanvasPanned -= TempReadOnlyCanvasPanned;
            SessionController.Instance.SessionView.FreeFormViewer.CanvasPanned += TempReadOnlyCanvasPanned;
        }

        /// <summary>
        /// Event handler whenever read-only windows are visible used to track the detail viewer's new elements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="libraryElementController"></param>
        private void DetailViewerOnNewLibraryElementShown(object sender, LibraryElementController libraryElementController)
        {
            GetOutOfTempReadOnly();
        }

        /// <summary>
        /// Event handler for when the canvas is panned during a public-collection-readonly-element viewing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TempReadOnlyCanvasPanned(object sender, bool e)
        {

            GetOutOfTempReadOnly();
        }

        /// <summary>
        /// private method to be called to hide the temp read only windows
        /// </summary>
        private void GetOutOfTempReadOnly()
        {
            SessionController.Instance.SessionView.FreeFormViewer.CanvasPanned -= TempReadOnlyCanvasPanned;
            _detailViewer.NewLibraryElementShown -= DetailViewerOnNewLibraryElementShown;

            _readOnlyLinksWindow.IsVisible = false;
            _readOnlyAliasesWindow.IsVisible = false;
            _readOnlyMetadataWindow.IsVisible = false;
        }
    }
}
