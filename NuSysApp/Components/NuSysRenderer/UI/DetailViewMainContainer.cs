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
    public class DetailViewRenderItem : RectangleUIElement
    {
        /// <summary>
        /// The main tab container of the detail view
        /// </summary>
        private TabContainerUIElement<string> _mainTabContainer;

        private DetailViewPageContainer _pageContainer;


        public DetailViewRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // create the _mainTabContainer
            _mainTabContainer = new TabContainerUIElement<string>(this, Canvas);

            // add the page to the _mainTabContainer, the page
            _pageContainer = new DetailViewPageContainer(this, Canvas);

            _mainTabContainer.setPage(_pageContainer);

            AddChild(_mainTabContainer);

            IsVisible = false;
        }

        /// <summary>
        /// Initializer method put events here
        /// </summary>
        /// <returns></returns>
        public override Task Load()
        {
            _mainTabContainer.OnCurrentTabChanged += _mainTabContainer_OnCurrentTabChanged;
            return base.Load();
        }

        /// <summary>
        /// Dispoe method remove events here
        /// </summary>
        public override void Dispose()
        {
            _mainTabContainer.OnCurrentTabChanged -= _mainTabContainer_OnCurrentTabChanged;
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

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _mainTabContainer.Width = Width;
            _mainTabContainer.Height = Height;
            base.Update(parentLocalToScreenTransform);
        }
    }
}
