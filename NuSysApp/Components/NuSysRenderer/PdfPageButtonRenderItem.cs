using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class PdfPageButtonRenderItem : InteractiveBaseRenderItem
    {
        private CanvasGeometry _triangle;
        private Rect _triangleBounds;
        public PdfPageButtonRenderItem(int direction, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            var x = (float)(direction*15);
            _triangle = CanvasGeometry.CreatePolygon(ResourceCreator, new System.Numerics.Vector2[4]{
                new Vector2(x, -20),
                new Vector2(x + direction * 25, 0),
                new Vector2(x, 20),
                new Vector2(x, -20)});
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
            
            _triangle.Dispose();
            _triangle = null;
            base.Dispose();
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed || !IsVisible)
                return;
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            base.Draw(ds);
            ds.FillGeometry(_triangle, Colors.Black);
            ds.Transform = Matrix3x2.Identity;
            _triangleBounds = _triangle.ComputeBounds(ds.Transform);
            ds.Transform = orgTransform;
        }

        public override Rect GetLocalBounds()
        {
            return _triangleBounds;
        }
    }
}
