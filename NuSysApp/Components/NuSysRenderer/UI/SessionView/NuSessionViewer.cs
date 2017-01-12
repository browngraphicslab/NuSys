using System;
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
        public LibraryListUIElement Library => _floatingMenu.Library;

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
                FontSize = 55,
                TrimmingGranularity = CanvasTextTrimmingGranularity.Character,
                Width = 300,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center
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


            _snapshotButton = new EllipseButtonUIElement(this, canvas, UIDefaults.AccentStyle);
            AddChild(_snapshotButton);

            //custom button
            _backButton = new ButtonUIElement(this, canvas, new RectangleUIElement(this, canvas))
            {
                Width = 15,
                Height = 30,
                SelectedBackground = Constants.LIGHT_BLUE_TRANSLUCENT,
                BorderWidth =  0,
                Bordercolor = Colors.Transparent,
                Background = Colors.Transparent
            };
            _backButton.ImageBounds = new Rect(_backButton.BorderWidth,
                _backButton.BorderWidth,
                _backButton.Width,
                _backButton.Height);
            AddChild(_backButton);

            TrailBox = new BreadCrumbContainer(this, Canvas)
            {
                IsVisible = SessionController.Instance.SessionSettings.BreadCrumbsDocked
            };
            AddChild(TrailBox);

            _backToWaitingRoom = new RectangleButtonUIElement(this, canvas, UIDefaults.PrimaryStyle, "back to waiting room")
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
                IsVisible = false
            };
            AddChild(Chatbox);

            _detailViewer = new DetailViewMainContainer(this, Canvas)
            {
                Width = 500,
                Height = 500,
                MinWidth = 400,
                MinHeight = 600,
                KeepAspectRatio = true
            };
            AddChild(_detailViewer);

            // add presentation node buttons
            _previousNode = new EllipseButtonUIElement(this, canvas, UIDefaults.AccentStyle)
            {
                IsVisible = false
            };
            AddChild(_previousNode);
            _nextNode = new EllipseButtonUIElement(this, canvas, UIDefaults.AccentStyle)
            {
                IsVisible = false
            };
            AddChild(_nextNode);
            _currentNode = new EllipseButtonUIElement(this, canvas, UIDefaults.AccentStyle)
            {
                IsVisible = false,
            };
            AddChild(_currentNode);
            _exitPresentation = new EllipseButtonUIElement(this, canvas, UIDefaults.AccentStyle)
            {
                IsVisible = false
            };
            AddChild(_exitPresentation);

            UpdateUI();

            _titleBox.DoubleTapped += _titleBox_DoubleTapped;
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private void _titleBox_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currCollectionController = SessionController.Instance.ContentController.GetLibraryElementController(SessionController.Instance.CurrentCollectionLibraryElementModel.LibraryElementId);
            SessionController.Instance.NuSessionView.ShowDetailView(currCollectionController);
        }

        private void InstanceOnEnterNewCollectionCompleted(object sender, string s)
        {
            _titleBox.Text = SessionController.Instance.CurrentCollectionLibraryElementModel.Title;
        }

        public void UpdateUI()
        {

            _titleBox.Transform.LocalPosition =
            new Vector2(SessionController.Instance.NuSessionView.Width / 2 - _titleBox.Width / 2, 0);
            
            _settingsButton.Transform.LocalPosition = new Vector2(SessionController.Instance.NuSessionView.Width / 2 + _titleBox.Width / 2 - _settingsButton.Width / 2 + 50,
            _titleBox.Height / 2 - _settingsButton.Height / 2);
            _backButton.Transform.LocalPosition = new Vector2(SessionController.Instance.NuSessionView.Width / 2 - _titleBox.Width / 2 - _settingsButton.Width / 2 - 50,
                _titleBox.Height / 2 - _backButton.Height / 2);
            

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
            else
            {
                TrailBox.Transform.LocalPosition =
                    new Vector2(SessionController.Instance.NuSessionView.Width - TrailBox.Width, 0);
                _backToWaitingRoom.Transform.LocalPosition =
                    new Vector2(
                        _backButton.Transform.LocalPosition.X + _backButton.Width / 2 - _backToWaitingRoom.Width / 2,
                        _backButton.Transform.LocalPosition.Y + _backButton.Height + 15);
            }

            if (!SessionController.Instance.SessionSettings.BreadCrumbsDocked)
            {
                TrailBox.IsVisible = _backToWaitingRoom.IsVisible;
            }
            else
            {
                TrailBox.IsVisible = true;
            }


            _settingsMenu.Transform.LocalPosition = new Vector2(_settingsButton.Transform.LocalPosition.X + _settingsButton.Width / 2 - _settingsMenu.Width / 2, _settingsButton.Height + _settingsButton.Transform.LocalPosition.Y + 15);
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
            _chatButton.Image = _chatButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/icon_chat.png"));
            _chatButton.ImageBounds = new Rect(_chatButton.Width/4, _chatButton.Height/4, _chatButton.Width/2, _chatButton.Height/2);

            // set the image for the _snapshotButton
            _snapshotButton.Image = _snapshotButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/snapshot_icon.png"));
            _snapshotButton.ImageBounds = new Rect(_snapshotButton.Width/4, _snapshotButton.Height/4, _snapshotButton.Width/2, _snapshotButton.Height/2);

            //load and set the settings icon
            _settingsButton.Image = _settingsButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/gear.png"));
            _settingsButton.ImageBounds = new Rect(_settingsButton.Width/4, _settingsButton.Height/4, _settingsButton.Width/2, _settingsButton.Height/2);

            // set the image for the _backButton
            _backButton.Image = _backButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/back.png"));

            // set the images for presentation mode
            _nextNode.Image = _nextNode.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/presentation_forward.png"));
            _previousNode.Image = _previousNode.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/presentation_backward.png"));
            _currentNode.Image = _currentNode.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/node.png"));
            _exitPresentation.Image = _exitPresentation.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/trash can white.png"));

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
        }

        /// <summary>
        /// Fired whenever the waiting room button is clicked, returns the user to the waitin groom
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
        public async void ShowDetailView(LibraryElementController viewable, DetailViewTabType tabToOpenTo = DetailViewTabType.Home)
        {
            _detailViewer.ShowLibraryElement(viewable.LibraryElementModel.LibraryElementId);
        }

        /// <summary>
        /// Method to call to make the current workspace have the read-only ui.
        /// This should mainly hide things like the floating menu.
        /// </summary>
        public void MakeReadOnly()
        {
        }

        /// <summary>
        /// Method to call to undo the MakeReadOnly method and reapply the editable UI.
        /// </summary>
        public void MakeEditable()
        {
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
    }
}
