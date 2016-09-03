using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Foundation;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Numerics;
using Windows.UI;

namespace NuSysApp
{
    public class NodeResizerRenderItem : BaseRenderItem
    {

        private CanvasGeometry _triangle;
        public NodeResizerRenderItem(CollectionRenderItem parent, CanvasAnimatedControl resourceCreator) : base(parent, resourceCreator)
        {
        }

        public override async Task Load()
        {
            _triangle = CanvasGeometry.CreatePolygon(ResourceCreator, new System.Numerics.Vector2[4]{new Vector2(0, 30),
                new Vector2(30, 30),
                new Vector2(30, 0),
                new Vector2(0, 30)
            });
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Win2dUtil.Invert(C) * S * C * T * ds.Transform;

            // ds.FillCircle(new Rect { X = Postion.X, Y = 0, Width = _vm.Width, Height = _vm.Height }, Colors.Red);
            if (_triangle != null)
                ds.FillGeometry(_triangle, new Vector2(0,0), Colors.Black);

            ds.Transform = orgTransform;

            base.Draw(ds);
        }

        public override BaseRenderItem HitTest(Vector2 point)
        {
            var rect = new Rect(T.M31, T.M32, 30, 30);
            if (rect.Contains(point.ToPoint()))
            {
                return this;
            }
            return null;
        }
    }

    
}
