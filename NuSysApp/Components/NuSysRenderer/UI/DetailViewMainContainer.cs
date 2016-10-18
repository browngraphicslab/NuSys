using System;
using System.Collections.Generic;
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
        /// the layout manager for the _mainTabContainer
        /// </summary>
        private StackLayoutManager _mainTabLayoutManager;

        public DetailViewMainContainer(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // create the _mainTabContainer
            _mainTabContainer = new TabContainerUIElement<string>(this, Canvas);

            // add the page to the _mainTabContainer, the page
            _pageContainer = new DetailViewPageContainer(this, Canvas);

            _mainTabLayoutManager = new StackLayoutManager();

            _mainTabContainer.setPage(_pageContainer);

            AddChild(_mainTabContainer);
            _mainTabLayoutManager.AddElement(_mainTabContainer);

            IsVisible = false;
        }

        /// <summary>
        /// Initializer method put events here
        /// </summary>
        /// <returns></returns>
        public override Task Load()
        {
            _mainTabContainer.TabContainerClosed += _mainTabContainer_TabContainerClosed;
            _mainTabContainer.OnCurrentTabChanged += _mainTabContainer_OnCurrentTabChanged;
            return base.Load();
        }

        /// <summary>
        /// fired when the mainTabContainer is closed (i.e. has no more tabs to display)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainTabContainer_TabContainerClosed(object sender, EventArgs e)
        {
            IsVisible = false;
        }

        /// <summary>
        /// Dispoe method remove events here
        /// </summary>
        public override void Dispose()
        {
            _mainTabContainer.OnCurrentTabChanged -= _mainTabContainer_OnCurrentTabChanged;
            _mainTabContainer.TabContainerClosed -= _mainTabContainer_TabContainerClosed;
            base.Dispose();
        }

        /// <summary>
        /// Invoked whenever the current tab changes in the detail viewer
        /// </summary>
        /// <param name="tabType"></param>
        private void _mainTabContainer_OnCurrentTabChanged(string tabType)
        {
            _pageContainer.ShowLibraryElement(tabType);
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


            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);
            _mainTabContainer.AddTab(libraryElementModelId, controller.LibraryElementModel.Title);

        }

        /// <summary>
        /// Updates all the layout managers
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _mainTabLayoutManager.SetMargins(BorderWidth);
            _mainTabLayoutManager.TopMargin = TopBarHeight;
            _mainTabLayoutManager.SetSize(Width, Height);
            _mainTabLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
            _mainTabLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _mainTabLayoutManager.ArrangeItems();

            base.Update(parentLocalToScreenTransform);
        }
    }
}
