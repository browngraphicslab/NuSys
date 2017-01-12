using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class ReadOnlyModeWindow : ResizeableWindowUIElement
    {
        private RoundedRectButtonUIElement _closeButton;
        public ReadOnlyModeWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _closeButton = new RoundedRectButtonUIElement(this, resourceCreator, 1, "X");
            
            _closeButton.Height = 15;
            _closeButton.Width = 15;
            _closeButton.Background = Colors.SlateGray;
            AddChild(_closeButton);

            MinHeight = 300;
            MinWidth = 250;

            KeepAspectRatio = false;

            _closeButton.Transform.LocalPosition = new Vector2(-18, 2);
            _closeButton.Tapped += _closeButton_Tapped;
        }

        private void _closeButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            this.IsVisible = false;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _closeButton.Transform.LocalPosition = new Vector2(-18, 2);
            base.Update(parentLocalToScreenTransform);
        }

        public override void Dispose()
        {
            _closeButton.Tapped -= _closeButton_Tapped;
            base.Dispose();
        }
    }
}
