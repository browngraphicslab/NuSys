using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    /// <summary>
    /// buttons with an ellipse shape - all UI defaults should be set here.
    /// </summary>
    public class EllipseButtonUIElement : ButtonUIElement
    {
        /// <summary>
        /// getter to make sure that the base shape is of type ellipse UI element - it really should always be
        /// returns the ellipse UI element
        /// </summary>
        private EllipseUIElement Ellipse
        {
            get
            {
                Debug.Assert(base.Shape.GetType() == typeof(EllipseUIElement));
                return (EllipseUIElement)Shape;
            }
        }

        private int _style;

        /// <summary>
        /// constructor for ellipse button UI element - takes in a parent, resource creator, an optional style enum (primary, secondary, etc.),
        /// and an optional string for the button's label.
        /// 
        /// ellipse buttons should basically always have labels and icons.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="style"></param>
        public EllipseButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            int style = 0, string text = "") : base(parent, resourceCreator, new EllipseUIElement(parent, resourceCreator))
        {
            ButtonText = text;
            Height = 50;
            Width = 50;
            ImageBounds = new Rect(10, 10, 30, 30);
            
            //style dependent properties
            switch (style) 
            {
                ///primary style for ellipse
                case 0:
                    Ellipse.Background = Constants.DARK_BLUE;
                    ButtonTextColor = Constants.DARK_BLUE;
                    break;
                ///secondary style for ellipse
                case 1:
                    Ellipse.Background = Constants.MED_BLUE;
                    ButtonTextColor = Constants.DARK_BLUE;
                    break;
                case 2:
                    Ellipse.Background = Constants.RED;
                    ButtonTextColor = Constants.RED;
                    break;
                case 3:
                {
                    Height = 30;
                    Width = 30;
                    ButtonTextColor = Colors.Black;
                    break;
                }
                default:
                    break;
            }

            _style = style;
            SetOriginalValues();

        }

        /// <summary>
        /// returns a bounding box for the label that is the width of the button and is aligned at the bottom of the button
        /// </summary>
        /// <returns></returns>
        protected override Rect GetTextBoundingBox()
        {
            if (_style == 3)
            {
                // if the ellipse has the style of a bubble then we want the text to be within the circle.
                // this math returns a rect that is centered and around half the size of the circle.
                return new Rect(Width/4, 5, Width/2, Height/2);
            }
            else
            {
                return new Rect(0, Height, Width - 2*BorderWidth, Height - 2*BorderWidth);
            }
        }

        /// <summary>
        /// returns a canvas text format that is centered with no trimming
        /// </summary>
        /// <returns></returns>
        protected override CanvasTextFormat GetCanvasTextFormat()
        {
            var textFormat = base.GetCanvasTextFormat();
            textFormat.WordWrapping = CanvasWordWrapping.WholeWord;
            textFormat.HorizontalAlignment = CanvasHorizontalAlignment.Center;
            textFormat.TrimmingGranularity = CanvasTextTrimmingGranularity.None;
            return textFormat;
        }

        public override void Resize(double e)
        {
            base.Resize(e);
            ImageBounds = new Rect(_originalImageBounds.X, _originalImageBounds.Y, _originalImageBounds.Width * e, _originalImageBounds.Height * e);
        }

    }
}
