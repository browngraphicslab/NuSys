using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace NuSysApp
{

    public class RectangularMarqueeUIElement : RectangleUIElement
    {

        private const float MAX_OFFSET = 15f;

        private float _dashOffset = 0f;


        public RectangularMarqueeUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var strokeStyle = new CanvasStrokeStyle
            {
                DashCap = CanvasCapStyle.Flat,
                DashStyle = CanvasDashStyle.Dash,
                DashOffset = _dashOffset,
            };
            ds.DrawRectangle(GetLocalBounds(), Colors.SlateGray, 3f, strokeStyle);
            base.Draw(ds);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _dashOffset += 0.15f;

            if (_dashOffset > MAX_OFFSET)
            {
                _dashOffset = 0;
            }

            base.Update(parentLocalToScreenTransform);
        }
    }
}
