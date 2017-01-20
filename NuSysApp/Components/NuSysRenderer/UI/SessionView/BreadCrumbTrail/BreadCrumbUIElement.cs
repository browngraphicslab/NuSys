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
using Microsoft.Graphics.Canvas.Text;
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

        /// <summary>
        /// The breadcrumb associated with this breadcrumb ui element
        /// </summary>
        public BreadCrumb Crumb { get; }

        /// <summary>
        /// Textbox ui element used to display the title to the user
        /// </summary>
        private TextboxUIElement _titleBox;

        public BreadCrumbUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BreadCrumb crumb) : base(parent, resourceCreator, GetShapeFromController(parent, resourceCreator, crumb))
        {
            // initialize the default ui
            Crumb = crumb;
            Width = DefaultWidth;
            Height = DefaultHeight;
            Image = crumb.Icon;
            BorderWidth = 5;
            BorderColor = crumb.Color;
            ImageBounds = new Rect(0, 0, 1, 1);

            // initialize the title box
            _titleBox = new TextboxUIElement(this, ResourceCreator)
            {
                Background = Colors.Transparent,
                FontSize = 10,
                TrimmingSign = CanvasTrimmingSign.Ellipsis,
                TrimmingGranularity = CanvasTextTrimmingGranularity.Character,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                TextVerticalAlignment = CanvasVerticalAlignment.Center,
                Text = Crumb.Title,
                Height = 25,
                Width = DefaultWidth,
                TextColor = Colors.Black,
                Wrapping = CanvasWordWrapping.WholeWord
            };
            _titleBox.Transform.LocalPosition = new Vector2(0,-_titleBox.Height -5);
            AddChild(_titleBox);


            // update the title when the title changes in the crumb
            Crumb.TitleChanged += Crumb_TitleChanged;

        }

        /// <summary>
        /// Change the title when the title changes in the crumb
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="title"></param>
        private void Crumb_TitleChanged(BreadCrumb sender, string title)
        {
            _titleBox.Text = title;
        }

        /// <summary>
        /// remove any events we added
        /// </summary>
        public override void Dispose()
        {
            Crumb.TitleChanged -= Crumb_TitleChanged;
            base.Dispose();
        }

        /// <summary>
        /// Static method to set the shape of the bread crumb based on the type of the passed in controller
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        private static BaseInteractiveUIElement GetShapeFromController(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BreadCrumb crumb)
        {
            // collections are square and everything else is a circle, more granularity can be added later
            if (crumb.IsCollection)
            {
                return new RectangleUIElement(parent, resourceCreator);
            }

            return new EllipseUIElement(parent, resourceCreator);
        }
    }
}
