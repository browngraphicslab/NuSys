using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using System.Numerics;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    class LayoutWindowUIElement : DraggableWindowUIElement
    {
        private static float PANEL_WIDTH = 200.0f;
        private static float PANEL_HEIGHT = 322.0f;
        private static float PANEL_BORDER = 15.0f;
        private static float ARRANGE_BUTTON_WIDTH = PANEL_WIDTH - 2 * PANEL_BORDER;
        private static float ARRANGE_BUTTON_HEIGHT = ARRANGE_BUTTON_WIDTH / 1.61f / 2.0f;
        private static Vector2 ARRANGE_BUTTON_POSITION = new Vector2(PANEL_BORDER, PANEL_HEIGHT - PANEL_BORDER - ARRANGE_BUTTON_HEIGHT);
        private static String ARRANGE_TEXT = "Arrange";
        private ButtonUIElement _arrangeButton;
        public LayoutWindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Width = PANEL_WIDTH;
            Height = PANEL_HEIGHT;

            _arrangeButton = new ButtonUIElement(parent, resourceCreator, new RectangleUIElement(parent, resourceCreator));
            _arrangeButton.ButtonText = ARRANGE_TEXT;
            _arrangeButton.Width = ARRANGE_BUTTON_WIDTH;
            _arrangeButton.Height = ARRANGE_BUTTON_HEIGHT;
            _arrangeButton.ButtonTextColor = Colors.White;
            _arrangeButton.Background = Colors.Red;
            _arrangeButton.ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            _arrangeButton.ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;
            _arrangeButton.Transform.LocalPosition = ARRANGE_BUTTON_POSITION;
            AddChild(_arrangeButton);
        }
    }
}
