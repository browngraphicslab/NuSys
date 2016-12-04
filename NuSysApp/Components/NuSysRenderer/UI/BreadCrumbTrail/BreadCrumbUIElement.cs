using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static float DefaultWidth = 25;

        /// <summary>
        /// The default height of the breadcrumb ui element
        /// </summary>
        public static float DefaultHeight = 25;

        /// <summary>
        /// The default spacing between breadcrumb ui elements
        /// </summary>
        public static float DefaultSpacing = 25;

        public BreadCrumb Crumb { get; }

        public BreadCrumbUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BreadCrumb crumb) : base(parent, resourceCreator, GetShapeFromController(parent, resourceCreator, crumb))
        {
            Crumb = crumb;
            BorderWidth = 5;
            Width = DefaultWidth;
            Height = DefaultHeight;
            Background = Colors.Transparent;
            Bordercolor = crumb.Color;

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
