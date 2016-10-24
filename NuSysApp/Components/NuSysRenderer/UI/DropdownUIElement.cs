using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using static NuSysApp.ButtonUIElement;
using Microsoft.Graphics.Canvas.Text;
using Windows.UI;

namespace NuSysApp.Components.NuSysRenderer.UI
{
    class DropdownUIElement : InteractiveBaseRenderItem
    {
        private static float BUTTON_HEIGHT = 30.0f;
        public StackLayoutManager _layoutManager;
        private float _width;
        private int _items = 0;

        public DropdownUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, float width) : base(parent, resourceCreator)
        {
            _width = width;

            _layoutManager = new StackLayoutManager(StackAlignment.Vertical);
            _layoutManager.Spacing = 0.0f;
            _layoutManager.TopMargin = 0.0f;
            _layoutManager.BottomMargin = 0.0f;
            _layoutManager.LeftMargin = 0.0f;
            _layoutManager.RightMargin = 0.0f;
            _layoutManager.Width = width;
            _layoutManager.Height = 0.0f;
            _layoutManager.ItemWidth = width;
            _layoutManager.ItemHeight = BUTTON_HEIGHT;
            _layoutManager.HorizontalAlignment = HorizontalAlignment.Left;
            _layoutManager.VerticalAlignment = VerticalAlignment.Top;
        }

        public void AddOption(string item, ButtonTappedHandler handler)
        {
            _layoutManager.Height += BUTTON_HEIGHT;
            var button = new ButtonUIElement(this, base.ResourceCreator, new RectangleUIElement(this, base.ResourceCreator));
            button.ButtonText = item;
            button.ButtonTextColor = Colors.Black;

            if (_items % 2 == 0)
            {
                button.Background = Colors.LightGray;
            } else
            {
                button.Background = Colors.LightSkyBlue;
            }

            button.Tapped += handler;
            button.BorderWidth = 0.0f;
            button.Width = _width;
            button.Height = BUTTON_HEIGHT;
            button.ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Left;
            button.ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;
            AddChild(button);
            _layoutManager.AddElement(button);
            _items++;
        }

        public void Layout()
        {
            _layoutManager.ArrangeItems();
        }
    }
}
