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
        /// the layout manager for the _mainTabContainer, essentailly makes sure that the _mainTabContainer fills the entire
        /// width and height of the base ResizeableWindowUIelement
        /// </summary>
        private StackLayoutManager _mainTabContainerLayoutManager;

        /// <summary>
        /// Dictionary of library element model ids to the currebt tab open for that id
        /// </summary>
        private Dictionary<string, DetailViewPageTabType> _libElemToCurrTabOpen;

        public DetailViewMainContainer(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // initialize the basic ui
            IsVisible = false;


            // create the _mainTabContainer
            _mainTabContainer = new TabContainerUIElement<string>(this, Canvas);

            // create the page container
            _pageContainer = new DetailViewPageContainer(this, Canvas);

            // the page displayed on the main tab container is a page container
            _mainTabContainer.SetPage(_pageContainer);

            // set the basic settings of the maintab layout maanger
            _mainTabContainerLayoutManager = new StackLayoutManager
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                TopMargin = TopBarHeight,
                LeftMargin = BorderWidth,
                RightMargin = BorderWidth,
                BottomMargin = BorderWidth
            };
            _mainTabContainerLayoutManager.AddElement(_mainTabContainer);
            AddChild(_mainTabContainer);

            // initialize the dictionary of library element model ids to detail view tabs which are open
            _libElemToCurrTabOpen = new Dictionary<string, DetailViewPageTabType>();

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

            // show the requested library element
            _pageContainer.ShowLibraryElement(libElemId, currPage);
        }

        /// <summary>
        /// Show a library element in the detail viewer
        /// </summary>
        /// <param name="libraryElementModelId"></param>
        public void ShowLibraryElement(string libraryElementModelId)
        {
            // if the detail viewer isn't currently visible then make it visible
            if (!IsVisible)
            {
                IsVisible = true;
            }

            // adds a tab to the detailviewer which should fire OnCurrentTabChanged in the _mainTabContainer,
            // which will cause the element to be displayed in the method _mainTabContainer_OnCurrentTabChanged()
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);
            _mainTabContainer.AddTab(libraryElementModelId, controller.LibraryElementModel.Title);

        }

        /// <summary>
        /// Updates all the layout managers
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // make the main tab container fill the width and height of the window
            _mainTabContainerLayoutManager.SetSize(Width, Height);
            _mainTabContainerLayoutManager.ArrangeItems();

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
