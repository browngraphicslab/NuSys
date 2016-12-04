using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class BreadCrumbContainer : RectangleUIElement
    {
        /// <summary>
        /// List of bread crumbs which are currently on the trail
        /// </summary>
        private List<BreadCrumb> _breadCrumbs;

        /// <summary>
        /// The handle we use to scroll through the breadcrumb trail
        /// </summary>
        private RectangleUIElement _scrollHandle;

        public BreadCrumbContainer(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _breadCrumbs = new List<BreadCrumb>();
            Height = 100;
            Width = 300;
            Background = Colors.DimGray;
            BorderWidth = 3;
            Bordercolor = Colors.Black;

            _scrollHandle = new RectangleUIElement();

        }

        public void AddBreadCrumb(LibraryElementController controller, CollectionLibraryElementController parentCollectionController)
        {
            _breadCrumbs.Add(new BreadCrumb(controller, parentCollectionController));
        }

        /// <summary>
        /// Computer the 
        /// </summary>
        private void RecomputeSize()
        {
            // calculate the total width needed to display all the breadcrumbs
            var totalWidth = _breadCrumbs.Count*BreadCrumbUIElement.DefaultWidth + 
                                (_breadCrumbs.Count + 1)*BreadCrumbUIElement.DefaultSpacing;

            // calculate 
            var ratio = Width/totalWidth;
        }

        public void ReRender()
        {
            
        }
    }
}
