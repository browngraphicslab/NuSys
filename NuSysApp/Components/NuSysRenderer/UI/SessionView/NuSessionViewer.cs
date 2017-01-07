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
using NuSysApp.Network.Requests;
using ReverseMarkdown.Converters;
using WinRTXamlToolkit.Controls.DataVisualization;

namespace NuSysApp
{
    public class NuSessionViewer : RectangleUIElement
    {
        private FloatingMenu _floatingMenu;

        //private ButtonUIElement _currCollDetailViewButton;

        private ButtonUIElement _chatButton;

        private ButtonUIElement _snapshotButton;

        /// <summary>
        /// button for the settings of the session
        /// </summary>
        private ButtonUIElement _settingsButton;

        /// <summary>
        /// The menu UI for the settings of the session
        /// </summary>
        private SessionSettingsMenu _settingsMenu;

        private ChatBoxUIElement _chatBox;

        private ButtonUIElement _backButton;

        private UserBubbleContainerUIElement _userBubbleContainer;

        private DetailViewMainContainer _detailViewer;

        public BreadCrumbContainer TrailBox;

        public TextboxUIElement _titleBox;

        public FilterMenu FilterMenu => _floatingMenu.FilterMenu;

        private RectangleButtonUIElement _backToWaitingRoom;

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
            _chatBox = new ChatBoxUIElement(this, canvas)
            {
                IsVisible = false
            };
            AddChild(_chatBox);

            _detailViewer = new DetailViewMainContainer(this, Canvas)
            {
                Width = 500,
                Height = 500,
                MinWidth = 400,
                MinHeight = 600,
                KeepAspectRatio = true
            };
            AddChild(_detailViewer);

            UpdateUI();

            Canvas.SizeChanged += OnMainCanvasSizeChanged;
            //_currCollDetailViewButton.Tapped += OnCurrCollDetailViewButtonTapped;
            _snapshotButton.Tapped += SnapShotButtonTapped; 
            _chatButton.Tapped += ChatButtonOnTapped;
            _backButton.Tapped += BackTapped;
            _backToWaitingRoom.Tapped += BackToWaitingRoomOnTapped;
            _settingsButton.Tapped += SettingsButtonOnTapped;
        }

        /// <summary>
        /// show both the breadcrumb trail window and the back to waiting room button
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void BackTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_backToWaitingRoom.IsVisible)
            {
                _backToWaitingRoom.IsVisible = false;
                TrailBox.IsVisible = false;
            }
            else
            {
                _backToWaitingRoom.IsVisible = true;
                TrailBox.IsVisible = true;
            }

            if (SessionController.Instance.SessionSettings.BreadCrumbsDocked)
            {
                TrailBox.IsVisible = true;
            }
        }

        private void _titleBox_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currCollectionController =
                SessionController.Instance.ContentController.GetLibraryElementController(
                    SessionController.Instance.CurrentCollectionLibraryElementModel.LibraryElementId);
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
            _titleBox.DoubleTapped += _titleBox_DoubleTapped;
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


            _settingsMenu.Transform.LocalPosition = new Vector2(_settingsButton.Transform.LocalPosition.X + _settingsButton.Width / 2 - _settingsMenu.Width / 2,
                _settingsButton.Height + _settingsButton.Transform.LocalPosition.Y + 15);

         
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
            _chatBox.IsVisible = !_chatBox.IsVisible;
            if (_chatBox.IsVisible)
            {
                _chatBox.Height = Math.Min(Height - 100, _chatBox.Height);
                _chatBox.Width = Math.Min(Width - 100, _chatBox.Width);
                _chatBox.Transform.LocalPosition = new Vector2(10, Height - _chatBox.Height- 70);
            }

            
        }

        /// <summary>
        /// Fired when the button for opening the current collection in the detail view is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnCurrCollDetailViewButtonTapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            var currWorkspaceController = SessionController.Instance.ContentController.GetLibraryElementController(SessionController.Instance.ActiveFreeFormViewer.LibraryElementId);
            ShowDetailView(currWorkspaceController);
        }

        private void OnMainCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Width = (float) e.NewSize.Width;
            Height = (float) e.NewSize.Height;            

            _floatingMenu.Transform.LocalPosition = new Vector2(Width / 4 - _floatingMenu.Width/2, Height / 4 - _floatingMenu.Height/2);
            //_currCollDetailViewButton.Transform.LocalPosition = new Vector2(Width - _currCollDetailViewButton.Width - 10, 10);
            _chatButton.Transform.LocalPosition = new Vector2(10, Height - _chatButton.Height - 10);
            _snapshotButton.Transform.LocalPosition = new Vector2(10, 10);
            _settingsButton.Transform.LocalPosition = new Vector2(80, 10);
            _chatBox.Transform.LocalPosition = new Vector2(10, Height - _chatBox.Height - 70);
            _backButton.Transform.LocalPosition = new Vector2(10, Height/2 - _backButton.Height/2);
            _userBubbleContainer.Transform.LocalPosition = _chatButton.Transform.LocalPosition + new Vector2(_chatButton.Width + 10, Height - _userBubbleContainer.Height - 10);
            _detailViewer.Transform.LocalPosition = new Vector2(Width/2, 0);
            _detailViewer.Height = Height;
            _detailViewer.Width = Width/2;

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
            // set the image for the _currCollDetailViewbutton
            //_currCollDetailViewButton.Image = _currCollDetailViewButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/info.png"));
            //_currCollDetailViewButton.ImageBounds = new Rect(_currCollDetailViewButton.Width/4, _currCollDetailViewButton.Height/4, _currCollDetailViewButton.Width/2, _currCollDetailViewButton.Height/2);

            // set the image for the _chatButton
            _chatButton.Image = _chatButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/icon_chat.png"));
            _chatButton.ImageBounds = new Rect(_chatButton.Width / 4, _chatButton.Height / 4, _chatButton.Width / 2, _chatButton.Height / 2);

            // set the image for the _snapshotButton
            _snapshotButton.Image = _snapshotButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/snapshot_icon.png"));
            _snapshotButton.ImageBounds = new Rect(_snapshotButton.Width / 4, _snapshotButton.Height / 4, _snapshotButton.Width / 2, _snapshotButton.Height / 2);

            //load and set the settings icon
            _settingsButton.Image = _settingsButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/gear.png"));
            _settingsButton.ImageBounds = new Rect(_settingsButton.Width / 4, _settingsButton.Height / 4, _settingsButton.Width / 2, _settingsButton.Height / 2);

            // set the image for the _backButton
            _backButton.Image = _backButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/back.png"));
            
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
            //_currCollDetailViewButton.Tapped -= OnCurrCollDetailViewButtonTapped;
            _snapshotButton.Tapped -= SnapShotButtonTapped;
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
    }
}
