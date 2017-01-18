using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    /// <summary>
    /// Another widely used button class to factor out styles, extending from ButtonUIElement.
    /// </summary>
    public class RectangleButtonUIElement : ButtonUIElement
    {
        /// <summary>
        /// get base shape as a rectangle.
        /// should ALWAYS be a rectangle.
        /// </summary>
        private RectangleUIElement Rect
        {
            get
            {
                Debug.Assert(base.Shape.GetType() == typeof(RectangleUIElement));
                return (RectangleUIElement)Shape;
            }
        }

        /// <summary>
        /// constructor for a rectangle button ui element 
        /// like the other button UI elements, takes in an optional style (primary, secondary, etc) and text for a label.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="style"></param>
        /// <param name="text"></param>
        public RectangleButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, int style = 0, string text = "")
            : base(parent, resourceCreator, new RectangleUIElement(parent, resourceCreator))
        {
            ButtonText = text;
            ButtonTextColor = Colors.White;
            Width = 200;
            Height = 50;
            ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;

            switch (style)
            {
                case 0:
                    Rect.Background = Constants.DARK_BLUE;
                    SelectedBackground = Constants.MED_BLUE;
                    break;
                case 1:
                    Rect.Background = Constants.MED_BLUE;
                    SelectedBackground = Constants.DARK_BLUE;
                    break;
                case 2:
                    Rect.Background = Constants.RED;
                    ButtonTextColor = Colors.White;
                    break;
                default:
                    Rect.Background = Colors.White;
                    SelectedBackground = Constants.LIGHT_BLUE;
                    ButtonTextColor = Constants.DARK_BLUE;

                    break;
            }
        }
    }
}
