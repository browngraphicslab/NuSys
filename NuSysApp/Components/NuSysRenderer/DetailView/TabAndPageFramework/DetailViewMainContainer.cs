using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class DetailViewMainContainer : ResizeableWindowUIElement
    {
        /// <summary>
        /// The main tab container of the detail view
        /// </summary>
        private TabContainerUIElement<string> _mainTabContainer;

        /// <summary>
        /// The page displayed in the main tab container
        /// </summary>
        private DetailViewPageContainer _pageContainer;

        /// <summary>
        /// the layout manager for the _mainTabContainer, currently expands the mainTabContainer to fill the entire detail viewer window
        /// </summary>
        private StackLayoutManager _mainTabLayoutManager;

        /// <summary>
        /// The boolean for siabling the detail view. When true, the detail view will not open.
        /// </summary>
        private bool _disabled;

        /// <summary>
        /// Dictionary of library element ids to the currTabOpen. So that if we were on the regions tab in one page
        /// and we click to another tab, when we return we open to the regions tab again
        /// </summary>
        private Dictionary<string, DetailViewPageTabType> _libElemToCurrTabOpen;

        /// <summary>
        /// button that floats along side of detail view. you click it to close the detail view.
        /// </summary>
        private EllipseButtonUIElement _closeButton;

        /// <summary>
        /// stack layout manager to arrange the positions of user bubbles on the side of the detail view
        /// </summary>
        private StackLayoutManager _userLayoutManager;

        /// <summary>
        /// true if the main container has been loaded
        /// </summary>
        private bool _loaded;

        public DetailViewMainContainer(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // create the _mainTabContainer, this is the tabs at the top of the page which represent different elements open in the detail view
            // the page of the mainTabContainer is the _pageContainer, which will dynamically be updated to display the correct item
            // in the detail viewer when a tab on the mainTabContainer is clicked
            _mainTabContainer = new TabContainerUIElement<string>(this, Canvas)
            {
                TabSpacing = 5,
                Width = Width,
                Height = 40,
                TabHeight = 40,
                Underlined = true
            };
            AddChild(_mainTabContainer);

            TopBarColor = Constants.MED_BLUE;
            TopBarHeight = 10;

            // add the pageContainer as the page to the main tab container
            _pageContainer = new DetailViewPageContainer(this, Canvas);
            _pageContainer.Width = Width;
            _pageContainer.Height = Height - TopBarHeight;

            _mainTabContainer.SetPage(_pageContainer); // adds the pageContainer as a child of the mainTabContainer as a side effect

            // dictionary of library element ids
            _libElemToCurrTabOpen = new Dictionary<string, DetailViewPageTabType>();

            _closeButton = new EllipseButtonUIElement(this, Canvas, UIDefaults.SecondaryStyle)
            {
                Height = 30,
                Width = 30,
                ImageBounds = new Rect(7.5,7.5,15,15)
            };
            AddChild(_closeButton);
            _closeButton.Transform.LocalPosition = new Vector2(-40, 80);

            // setup the mainTabLayoutManager so that the mainTabContainer fills the entire detail viewer window
            _mainTabLayoutManager = new StackLayoutManager
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            _mainTabLayoutManager.SetMargins(15,BorderWidth,15,BorderWidth);
            _mainTabLayoutManager.TopMargin = TopBarHeight;
            _mainTabLayoutManager.AddElement(_mainTabContainer);

            // detail view defaults to invisible. visible on click
            IsVisible = false;
            IsSnappable = true;

            // add events
            _mainTabContainer.TabContainerClosed += _mainTabContainer_TabContainerClosed;
            _mainTabContainer.OnCurrentTabChanged += _mainTabContainer_OnCurrentTabChanged;
            _mainTabContainer.OnTabRemoved += _mainTabContainer_OnTabRemoved;
            _pageContainer.OnPageTabChanged += PageContainerOnPageTabChanged;

            _closeButton.Tapped += CloseButtonOnTapped;
        }

        /// <summary>
        /// shows the users on the current element visible in the detail view
        /// </summary>
        private void CreateUserBubbles(string libraryElementId)
        {
            var users = SessionController.Instance.UserController.GetUsersOfLibraryElement(libraryElementId);
            if (_userLayoutManager != null)
            {
                _userLayoutManager.ClearStack(this);
            }
            else
            {
                _userLayoutManager = new StackLayoutManager(StackAlignment.Vertical);
                _userLayoutManager.Spacing = 10;
                _userLayoutManager.BottomMargin = 10;
                _userLayoutManager.LeftMargin = -40;
                _userLayoutManager.ItemWidth = 30;
                _userLayoutManager.ItemHeight = 30;
            }

            float height = 0;

            foreach (var user in users)
            {
                var userId = SessionController.Instance.NuSysNetworkSession.NetworkMembers[user];
                var displayName = SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary[user].ToUpper();
                var userBubble = new EllipseButtonUIElement(this, Canvas, UIDefaults.Bubble, displayName[0].ToString())
                {
                    Background = userId.Color
                };
                AddChild(userBubble);
                _userLayoutManager.AddElement(userBubble);
                height += userBubble.Height + _userLayoutManager.Spacing;
            }

            _userLayoutManager.TopMargin = Height - height - 10;
        }

        private void CloseButtonOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            HideDetailView();
        }

        public override async Task Load()
        {
            _closeButton.Image = _closeButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/x white.png"));
            base.Load();
            _loaded = true;
        }

        /// <summary>
        /// Fired whenever a new tabtype is shown on the page container
        /// </summary>
        /// <param name="page"></param>
        private void PageContainerOnPageTabChanged(string libraryElementId, DetailViewPageTabType page)
        {
            // set the element in the dictionary to the new page
            Debug.Assert(_libElemToCurrTabOpen.ContainsKey(libraryElementId));
            _libElemToCurrTabOpen[libraryElementId] = page;
        }

        /// <summary>
        /// Fired whenver a tab is removed from the main container
        /// </summary>
        /// <param name="libElemId"></param>
        private void _mainTabContainer_OnTabRemoved(string libElemId)
        {
            // remove the tab from the dictionary
            _libElemToCurrTabOpen.Remove(libElemId);
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libElemId);
            if (controller != null)
            {
                controller.Deleted -= Controller_Deleted;
                controller.TitleChanged -= Controller_TitleChanged;
            }

        }

        /// <summary>
        /// fired when the mainTabContainer is closed (i.e. has no more tabs to display)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainTabContainer_TabContainerClosed(object sender, EventArgs e)
        {
            HideDetailView();
        }

        /// <summary>
        /// Dispoe method remove events here
        /// </summary>
        public override void Dispose()
        {
            _mainTabContainer.TabContainerClosed -= _mainTabContainer_TabContainerClosed;
            _mainTabContainer.OnCurrentTabChanged -= _mainTabContainer_OnCurrentTabChanged;
            _mainTabContainer.OnTabRemoved -= _mainTabContainer_OnTabRemoved;
            _pageContainer.OnPageTabChanged -= PageContainerOnPageTabChanged;
            _closeButton.Tapped -= CloseButtonOnTapped;
            base.Dispose();
        }

        /// <summary>
        /// Invoked whenever the current tab changes in the detail viewer
        /// </summary>
        /// <param name="libElemId"></param>
        private void _mainTabContainer_OnCurrentTabChanged(string libElemId)
        {
            // try to get the curr page that is open from the dictionary
            DetailViewPageTabType currPage;
            _libElemToCurrTabOpen.TryGetValue(libElemId, out currPage);

            // if it is not in the dictionary, then set the currPage to the home page
            if (currPage == null)
            {
                currPage = new DetailViewPageTabType(DetailViewPageType.Home);
                _libElemToCurrTabOpen.Add(libElemId, currPage);
            }

            _pageContainer.ShowLibraryElement(libElemId, currPage);

            CreateUserBubbles(libElemId);
        }

        /// <summary>
        /// Show a library element in the detail viewer
        /// </summary>
        /// <param name="libraryElementModelId"></param>
        public void ShowLibraryElement(string libraryElementModelId)
        {

            if (SessionController.Instance.CurrentMode != Options.PanZoomOnly)
            {
                return;
            }

            if (_disabled || !_loaded)
            {
                return;
            }

            // if the detail viewer isn't currently visible then make it visible
            if (!IsVisible)
            {
                IsVisible = true;
            }


            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);
            _mainTabContainer.AddTab(libraryElementModelId, controller.LibraryElementModel.Title);
            controller.TitleChanged += Controller_TitleChanged;
            controller.Deleted += Controller_Deleted;

        }

        private void Controller_TitleChanged(object sender, string e)
        {
            var controller = sender as LibraryElementController;
            Debug.Assert(controller != null);
            _mainTabContainer.UpdateTabTitle(controller.LibraryElementModel.LibraryElementId, e);
        }

        private void Controller_Deleted(object sender)
        {
            var controller = sender as LibraryElementController;
            Debug.Assert(controller != null);
            _mainTabContainer.RemoveTab(controller.LibraryElementModel.LibraryElementId);
        }

        /// <summary>
        /// Updates all the layout managers
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // set the size to the new width and height, then arrange items
            // this makes the mainTabContainer fill the entire window
            _mainTabLayoutManager.SetSize(Width, Height);
            _mainTabLayoutManager.ArrangeItems();

            if (_userLayoutManager != null)
            {
                _userLayoutManager.ArrangeItems();
            }

            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// hides the detail view so we can no longer see it
        /// </summary>
        public void HideDetailView()
        {
            IsVisible = false;
        }

        public void DisableDetailView()
        {
            _disabled = true;
        }

        public void EnableDetailView()
        {
            _disabled = false;
        }
    }
}
