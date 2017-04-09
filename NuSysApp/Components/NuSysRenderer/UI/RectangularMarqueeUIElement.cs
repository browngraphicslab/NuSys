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
        //The offset at which we set _dashoffset back to 0.
        private const float MAX_OFFSET = 15f;
        //Offset we set the strokestyle's offset too
        private float _dashOffset = 0f;
        //Stroke style with dash style
        private CanvasStrokeStyle _strokeStyle;

        public RectangularMarqueeUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _strokeStyle = new CanvasStrokeStyle
            {
                DashCap = CanvasCapStyle.Flat,
                DashStyle = CanvasDashStyle.Dash,
                DashOffset = _dashOffset,
            };
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            ds.DrawRectangle(GetLocalBounds(), Colors.SlateGray, 3f, strokeStyle);
            base.Draw(ds);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _dashOffset += 0.25f;
            
            if (_dashOffset > MAX_OFFSET)
            {
                _dashOffset -= MAX_OFFSET;
            }

            _strokeStyle.DashOffset = _dashOffset;
            
            base.Update(parentLocalToScreenTransform);
        }
    }
}
