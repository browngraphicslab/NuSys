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
    /// rounded rectangle button class. automatically sets radius of rounded rect to 5 in accordance with styles.
    /// </summary>
    public class RoundedRectButtonUIElement : ButtonUIElement
    {
        private RoundedRectangleUIElement RoundedRect
        {
            get
            {
                Debug.Assert(base.Shape.GetType() == typeof(RoundedRectangleUIElement));
                return (RoundedRectangleUIElement)Shape;
            }
        }

        public RoundedRectButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            int style = 0, string text = "")
            : base(parent, resourceCreator, new RoundedRectangleUIElement(parent, resourceCreator))
        {
            ButtonText = text;
            ButtonTextColor = Colors.White;
            Width = 200;
            Height = 50;
            ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;
            RoundedRect.Radius = 5;

            switch (style) 
            {
                case 0:
                    RoundedRect.Background = Constants.DARK_BLUE;
                    break;
                case 1:
                    RoundedRect.Background = Constants.MED_BLUE;
                    break;
                default:
                    break;
            }


        }
    }
}
