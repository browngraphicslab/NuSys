using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class InkRenderItem : BaseRenderItem
    {
        private ElementViewModel _vm;
        private ConcurrentBag<InkStroke> _inkStrokes = new ConcurrentBag<InkStroke>(); 


        public InkRenderItem(CollectionRenderItem parent, CanvasAnimatedControl resourceCreator):base(parent, resourceCreator)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            _vm = null;
            _inkStrokes = null;
        }

        public void AddStroke(InkStroke stroke)
        {
            _inkStrokes.Add(stroke);
        }

        public override void Update()
        {
            if (!IsDirty)
                return;

            var aa = GetDrawingAttributes();
            foreach (var s in _inkStrokes)
            {
                var attr = aa;
                attr.Color = Colors.Black;
                s.DrawingAttributes = attr;
            }
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            ds.DrawInk(_inkStrokes);
        }

        protected InkDrawingAttributes GetDrawingAttributes()
        {
            var _drawingAttributes = new InkDrawingAttributes
            {
                PenTip = PenTipShape.Circle,
                PenTipTransform = Matrix3x2.CreateRotation((float)Math.PI / 4),
                IgnorePressure = false,
                Size = new Size(4, 4),
                Color = Colors.Black
            };

            return _drawingAttributes;
        }
    }
}
