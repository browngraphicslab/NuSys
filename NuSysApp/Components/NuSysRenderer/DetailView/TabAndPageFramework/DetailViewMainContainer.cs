using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
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
        /// Dictionary of library element ids to the currTabOpen. So that if we were on the regions tab in one page
        /// and we click to another tab, when we return we open to the regions tab again
        /// </summary>
        private Dictionary<string, DetailViewPageTabType> _libElemToCurrTabOpen;

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
            };
            AddChild(_mainTabContainer);

            // add the pageContainer as the page to the main tab container
            _pageContainer = new DetailViewPageContainer(this, Canvas);
            _mainTabContainer.SetPage(_pageContainer); // adds the pageContainer as a child of the mainTabContainer as a side effect

            // dictionary of library element ids
            _libElemToCurrTabOpen = new Dictionary<string, DetailViewPageTabType>();

            // setup the mainTabLayoutManager so that the mainTabContainer fills the entire detail viewer window
            _mainTabLayoutManager = new StackLayoutManager
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            _mainTabLayoutManager.SetMargins(BorderWidth);
            _mainTabLayoutManager.TopMargin = TopBarHeight;
            _mainTabLayoutManager.AddElement(_mainTabContainer);

            TopBarColor = Constants.MED_BLUE;

            // detail view defaults to invisible. visible on click
            IsVisible = false;

            // add events
            _mainTabContainer.TabContainerClosed += _mainTabContainer_TabContainerClosed;
            _mainTabContainer.OnCurrentTabChanged += _mainTabContainer_OnCurrentTabChanged;
            _mainTabContainer.OnTabRemoved += _mainTabContainer_OnTabRemoved;
            _pageContainer.OnPageTabChanged += PageContainerOnPageTabChanged;
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

            // if the detail viewer isn't currently visible then make it visible
            if (!IsVisible)
            {
                IsVisible = true;
            }


            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);
            _mainTabContainer.AddTab(libraryElementModelId, controller.LibraryElementModel.Title);
            controller.Deleted += Controller_Deleted;

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

            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// hides the detail view so we can no longer see it
        /// </summary>
        public void HideDetailView()
        {
            IsVisible = false;
        }
    }
}
