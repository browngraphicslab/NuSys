using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class ReadOnlyModeWindow : ResizeableWindowUIElement
    {
        private RectangleButtonUIElement _closeButton;
        public ReadOnlyModeWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _closeButton = new RectangleButtonUIElement(this, resourceCreator);
            _closeButton.ButtonText = "Close";
            AddChild(_closeButton);

            _closeButton.Transform.LocalPosition = new Vector2(10, 25);
            _closeButton.Tapped += _closeButton_Tapped;
        }

        private void _closeButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            this.IsVisible = false;
        }

        public void Dispose()
        {
            _closeButton.Tapped -= _closeButton_Tapped;
            base.Dispose();
        }
    }
}
