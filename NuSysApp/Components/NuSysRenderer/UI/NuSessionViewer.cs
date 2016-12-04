using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NuSysApp.Network.Requests;

namespace NuSysApp
{
    public class NuSessionViewer : RectangleUIElement
    {
        private FloatingMenu _floatingMenu;

        //private ButtonUIElement _currCollDetailViewButton;

        private ButtonUIElement _chatButton;

        private ButtonUIElement _snapshotButton;

        private ChatBoxUIElement _chatBox;

        private ButtonUIElement _backToWaitingRoomButton;

        private UserBubbleContainerUIElement _userBubbleContainer;

        private DetailViewMainContainer _detailViewer;

        public BreadCrumbContainer TrailBox;


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

            TrailBox = new BreadCrumbContainer(this, Canvas);
            AddChild(TrailBox);

            _chatButton = new ButtonUIElement(this, canvas, new EllipseUIElement(this, canvas))
            {
                Width = 50,
                Height = 50,
                Background = Colors.Purple
            };
            AddChild(_chatButton);

            _snapshotButton = new ButtonUIElement(this, canvas, new EllipseUIElement(this, canvas))
            {
                Width = 50,
                Height = 50,
                Background = Colors.Purple
            };
            AddChild(_snapshotButton);

            _backToWaitingRoomButton = new ButtonUIElement(this, canvas, new RectangleUIElement(this, canvas))
            {
                Width = 50,
                Height = 100,
                SelectedBackground = Colors.Gray,
                SelectedBorder = Colors.LightGray,
                BorderWidth = 3,
                Bordercolor = Colors.Transparent,
                Background = Colors.Transparent
            };
            _backToWaitingRoomButton.ImageBounds = new Rect(_backToWaitingRoomButton.BorderWidth,
                _backToWaitingRoomButton.BorderWidth,
                _backToWaitingRoomButton.Width - 2*_backToWaitingRoomButton.BorderWidth,
                _backToWaitingRoomButton.Height - 2*_backToWaitingRoomButton.BorderWidth);

            AddChild(_backToWaitingRoomButton);

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
                MinHeight = 400,
                KeepAspectRatio = true
            };
            AddChild(_detailViewer);

            Canvas.SizeChanged += OnMainCanvasSizeChanged;
            //_currCollDetailViewButton.Tapped += OnCurrCollDetailViewButtonTapped;
            _snapshotButton.Tapped += SnapShotButtonTapped;
            _chatButton.Tapped += ChatButtonOnTapped;
            _backToWaitingRoomButton.Tapped += BackToWaitingRoomOnTapped;
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

        private void OnMainCanvasSizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            Width = (float) e.NewSize.Width;
            Height = (float) e.NewSize.Height;

            _floatingMenu.Transform.LocalPosition = new Vector2(Width / 4 - _floatingMenu.Width/2, Height / 4 - _floatingMenu.Height/2);
            //_currCollDetailViewButton.Transform.LocalPosition = new Vector2(Width - _currCollDetailViewButton.Width - 10, 10);
            _chatButton.Transform.LocalPosition = new Vector2(10, Height - _chatButton.Height - 10);
            _snapshotButton.Transform.LocalPosition = new Vector2(10, 10);
            _chatBox.Transform.LocalPosition = new Vector2(10, Height - _chatBox.Height - 70);
            _backToWaitingRoomButton.Transform.LocalPosition = new Vector2(10, Height/2 - _backToWaitingRoomButton.Height/2);
            _userBubbleContainer.Transform.LocalPosition = _chatButton.Transform.LocalPosition + new Vector2(_chatButton.Width + 10, Height - _userBubbleContainer.Height - 10);
            _detailViewer.Transform.LocalPosition = new Vector2(Width/2, 0);
            _detailViewer.Height = Height;
            _detailViewer.Width = Width;
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


            // set the image for the _backToWaitingRoomButton
            _backToWaitingRoomButton.Image = _backToWaitingRoomButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/back icon triangle.png"));
            
            base.Load();
        }

        public override void Dispose()
        {
            Canvas.SizeChanged -= OnMainCanvasSizeChanged;
            //_currCollDetailViewButton.Tapped -= OnCurrCollDetailViewButtonTapped;
            _snapshotButton.Tapped -= SnapShotButtonTapped;
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
            // clear all the users from the user bubble container
            _userBubbleContainer.ClearUsers();

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
