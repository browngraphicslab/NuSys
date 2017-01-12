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
            _closeButton.ButtonText = "X";
            _closeButton.Height = 15;
            _closeButton.Width = 15;
            AddChild(_closeButton);

            MinHeight = 300;
            MinWidth = 250;

            _closeButton.Transform.LocalPosition = new Vector2(5, 5);
            _closeButton.Tapped += _closeButton_Tapped;
        }

        private void _closeButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            this.IsVisible = false;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _closeButton.Transform.LocalPosition = new Vector2(5, 5);
            base.Update(parentLocalToScreenTransform);
        }

        public override void Dispose()
        {
            _closeButton.Tapped -= _closeButton_Tapped;
            base.Dispose();
        }
    }
}
