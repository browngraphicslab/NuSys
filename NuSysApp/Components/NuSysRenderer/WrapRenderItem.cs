using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class WrapRenderItem : BaseRenderItem
    {
  
        private Rect _measurement;
        public float MarginX = 10;
        public float MarginY = 10;
        public float MaxWidth;

        public WrapRenderItem(float maxWidth, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            MaxWidth = maxWidth;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (IsDisposed)
                return;

            var lineWidth = 0.0;
            var lineY = 0.0;
            var lineX = 0.0;
            var linNum = 0;
            Rect measure;
            for (int index = 0; index < _children.Count; index++)
            {
                var baseRenderItem = _children[index];
                measure = baseRenderItem.GetMeasure();
                // lineHeight = lineHeight + measure.Height > lineHeight ? lineHeight + measure.Height : lineHeight;


                baseRenderItem.Transform.LocalPosition = new Vector2((float)lineX, (float)lineY);

                if (index >= _children.Count - 1)
                {
                    continue;
                }

                var nextMeasure = _children[index + 1].GetMeasure();

                if (lineX + measure.Width + MarginX + nextMeasure.Width < MaxWidth)
                {
                    lineX += measure.Width + MarginX;
                }
                else if (lineX + measure.Width + MarginX + nextMeasure.Width > MaxWidth)
                {
                    linNum += 1;
                    lineY = linNum * (measure.Height + MarginY);
                    lineX = 0.0;
                }
            }
            _measurement.X = 0;
            _measurement.Y = 0;
            _measurement.Width = MaxWidth;
            _measurement.Height = lineY + measure.Height;

            base.Update(parentLocalToScreenTransform);
          
        }

        public override Rect GetMeasure()
        {
            return _measurement;
        }

    }
}
