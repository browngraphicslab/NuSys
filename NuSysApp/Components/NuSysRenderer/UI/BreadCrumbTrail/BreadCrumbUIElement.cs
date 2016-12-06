using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{ 
    class BreadCrumbUIElement : ButtonUIElement
    {
        /// <summary>
        /// The default width of the breadcrumb ui element
        /// </summary>
        public static float DefaultWidth = 100;

        /// <summary>
        /// The default height of the breadcrumb ui element
        /// </summary>
        public static float DefaultHeight = 100;

        /// <summary>
        /// The default spacing between breadcrumb ui elements
        /// </summary>
        public static float DefaultSpacing = 25;

        public BreadCrumb Crumb { get; }

        public BreadCrumbUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BreadCrumb crumb) : base(parent, resourceCreator, GetShapeFromController(parent, resourceCreator, crumb))
        {
            Crumb = crumb;
            Width = DefaultWidth;
            Height = DefaultHeight;
            Image = crumb.Icon;
            BorderWidth = 5;
            Bordercolor = crumb.Color;
            ImageBounds = new Rect(0, 0, Width, Height);

        }

        /// <summary>
        /// Static method to set the shape of the bread crumb based on the type of the passed in controller
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        private static BaseInteractiveUIElement GetShapeFromController(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BreadCrumb crumb)
        {
            if (crumb.IsCollection)
            {
                return new RectangleUIElement(parent, resourceCreator);
            }

            return new EllipseUIElement(parent, resourceCreator);
        }
    }
}
