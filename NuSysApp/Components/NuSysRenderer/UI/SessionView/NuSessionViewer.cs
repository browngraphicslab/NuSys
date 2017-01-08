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

        public ChatBoxUIElement Chatbox { get; }

        private ButtonUIElement _backToWaitingRoomButton;

        private UserBubbleContainerUIElement _userBubbleContainer;

        private DetailViewMainContainer _detailViewer;

        public BreadCrumbContainer TrailBox;

        public TextboxUIElement _titleBox;

        public NuSessionViewer(BaseRenderItem parent, CanvasAnimatedControl canvas) : base(parent, canvas)
        {
            Background = Colors.Transparent;
            SessionController.Instance.NuSessionView = this; // set the session controller's getter for the NuSessionView

            _floatingMenu = new FloatingMenu(this, canvas);
            AddChild(_floatingMenu);

            //_currCollDetailViewButton = new ButtonUIElement(this, canvas, new EllipseUIElement(this, canvas))
            //{
            //    Width = 50,
            //    Height = 50,
            //    Background = Colors.Transparent,
            //    BorderWidth = 3,
            //    SelectedBorder = Colors.LightGray,
            //    Bordercolor = Colors.Transparent
            //};
            //AddChild(_currCollDetailViewButton);

            SessionController.Instance.EnterNewCollectionCompleted += InstanceOnEnterNewCollectionCompleted;
            TrailBox = new BreadCrumbContainer(this, Canvas);
            AddChild(TrailBox);

            _settingsButton = new EllipseButtonUIElement(this, canvas)
            {
                Background = Colors.Transparent
            };
            //AddChild(_settingsButton);

            _settingsMenu = new SessionSettingsMenu(this, canvas)
            {
                Width = 250,
                Height = 250,
                Background = Constants.LIGHT_BLUE,
                IsVisible =  false,
                KeepAspectRatio = false
            };
            //AddChild(_settingsMenu);

            _chatButton = new EllipseButtonUIElement(this, canvas, UIDefaults.AccentStyle);
            AddChild(_chatButton);

            _snapshotButton = new EllipseButtonUIElement(this, canvas, UIDefaults.AccentStyle);
            AddChild(_snapshotButton);

            //custom button
            _backToWaitingRoomButton = new ButtonUIElement(this, canvas, new RectangleUIElement(this, canvas))
            {
                Width = 15,
                Height = 30,
                SelectedBackground = Colors.Gray,
                BorderWidth =  0,
                Bordercolor = Colors.Transparent,
                Background = Colors.Transparent
            };
            _backToWaitingRoomButton.ImageBounds = new Rect(_backToWaitingRoomButton.BorderWidth,
                _backToWaitingRoomButton.BorderWidth,
                _backToWaitingRoomButton.Width,
                _backToWaitingRoomButton.Height);

            //AddChild(_backToWaitingRoomButton);

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

            var sct = new ScrollingCanvasTester(this, ResourceCreator);
            sct.Transform.LocalPosition = new Vector2(300, 300);
            AddChild(sct);

            Canvas.SizeChanged += OnMainCanvasSizeChanged;
            //_currCollDetailViewButton.Tapped += OnCurrCollDetailViewButtonTapped;
            _snapshotButton.Tapped += SnapShotButtonTapped; 
            _chatButton.Tapped += ChatButtonOnTapped;
            _backToWaitingRoomButton.Tapped += BackToWaitingRoomOnTapped;
            _settingsButton.Tapped += SettingsButtonOnTapped;

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
            if (GetChildren().Contains(_titleBox))
            {
                RemoveChild(_titleBox);
            }
            _titleBox = new TextboxUIElement(this, Canvas)
            {
                Text = SessionController.Instance.CurrentCollectionLibraryElementModel.Title,
                TextColor = Constants.ALMOST_BLACK,
                Background = Colors.Transparent,
                FontSize = 55,
                TrimmingGranularity = CanvasTextTrimmingGranularity.Character,
                Width = 300,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center
            };
            AddChild(_titleBox);
            _titleBox.Transform.LocalPosition =
                new Vector2(SessionController.Instance.NuSessionView.Width / 2 - _titleBox.Width / 2, 0);
            _titleBox.DoubleTapped += _titleBox_DoubleTapped;


            _settingsButton.Transform.LocalPosition = new Vector2(SessionController.Instance.NuSessionView.Width/2 + _titleBox.Width/2 - _settingsButton.Width/2 + 50, 
                _titleBox.Height/2 - _settingsButton.Height/2);
            AddChild(_settingsButton);
            _backToWaitingRoomButton.Transform.LocalPosition = new Vector2(SessionController.Instance.NuSessionView.Width / 2 - _titleBox.Width / 2 - _settingsButton.Width / 2 - 50,
                _titleBox.Height / 2 - _backToWaitingRoomButton.Height / 2);
            AddChild(_backToWaitingRoomButton);
            _settingsMenu.Transform.LocalPosition = new Vector2(_settingsButton.Transform.LocalPosition.X + _settingsButton.Width/2 - _settingsMenu.Width/2,
                _settingsButton.Height + _settingsButton.Transform.LocalPosition.Y + 15);
            AddChild(_settingsMenu);
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
                Chatbox.Transform.LocalPosition = new Vector2(10, Height - Chatbox.Height- 70);
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
            Chatbox.Transform.LocalPosition = new Vector2(10, Height - Chatbox.Height - 70);
            _backToWaitingRoomButton.Transform.LocalPosition = new Vector2(10, Height/2 - _backToWaitingRoomButton.Height/2);
            _userBubbleContainer.Transform.LocalPosition = _chatButton.Transform.LocalPosition + new Vector2(_chatButton.Width + 10, Height - _userBubbleContainer.Height - 10);
            _detailViewer.Transform.LocalPosition = new Vector2(Width/2, 0);
            _detailViewer.Height = Height;
            _detailViewer.Width = Width/2;
            TrailBox.Transform.LocalPosition = new Vector2(Width - TrailBox.Width, 0);

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

            // set the image for the _backToWaitingRoomButton
            _backToWaitingRoomButton.Image = _backToWaitingRoomButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/back.png"));
            
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
