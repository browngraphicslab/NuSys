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

        public override void Dispose()
        {
            _triangle.Dispose();
            _triangle = null;
            base.Dispose();
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
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            if (_triangle != null)
                ds.FillGeometry(_triangle, new Vector2(0,0), Colors.Black);

            ds.Transform = orgTransform;

            base.Draw(ds);
        }

        public override BaseRenderItem HitTest(Vector2 point)
        {
            var rect = new Rect(Transform.Position.X, Transform.Position.Y, 30, 30);
            if (rect.Contains(point.ToPoint()))
            {
                return this;
            }
            return null;
        }
    }

    
}
