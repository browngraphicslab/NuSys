using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp.Components.NuSysRenderer.UI.BaseUIElements
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
            : base(parent, resourceCreator, new RoundedRectButtonUIElement(parent, resourceCreator))
        {
            RoundedRect.Radius = 5;
            switch (style) 
            {
                case 0:
                    RoundedRect.Background = Constants.DARK_BLUE;
                    break;
                case 1:
                    RoundedRect.Background = Constants.MED_BLUE;
                    break;
            }

            ButtonText = text;
            ButtonTextColor = Colors.White;
            Width = 200;
            Height = 50;
        }
    }
}
