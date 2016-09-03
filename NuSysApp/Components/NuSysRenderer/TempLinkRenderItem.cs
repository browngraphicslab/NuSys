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
using Microsoft.Graphics.Canvas.UI.Xaml;
using SharpDX.Direct2D1;

namespace NuSysApp
{
    public class TempLinkRenderItem : BaseRenderItem
    {
        
        private CanvasGeometry _path;
        public ElementRenderItem Element1;
        public ElementRenderItem Element2;
        public Color Color;

        public TempLinkRenderItem(ElementRenderItem element1, ElementRenderItem element2, Color color, CollectionRenderItem parent, CanvasAnimatedControl resourceCreator):base(parent, resourceCreator)
        {
            Element1 = element1;
            Element2 = element2;
            Color = color;
        }

        private void OnAnchorChanged(object sender, Point2d point2D)
        {
            IsDirty = true;
        }

        public override void Dispose()
        {
            base.Dispose();
            _path.Dispose();
            _path = null;
        }

        public override void Update()
        {
            if (!IsDirty)
                return;
            var anchor1 = new Vector2((float)Element1.ViewModel.Anchor.X, (float)Element1.ViewModel.Anchor.Y);
            var anchor2 = new Vector2((float)Element2.ViewModel.Anchor.X, (float)Element2.ViewModel.Anchor.Y);

            var distanceX = (float)anchor1.X - anchor2.X;
            var distanceY = (float)anchor1.Y - anchor2.Y;

            var Point2 = new Vector2(anchor1.X - distanceX / 2, anchor2.Y);
            var Point1 = new Vector2(anchor2.X + distanceX / 2, anchor1.Y);
            var StartPoint = anchor1;
            var Point3 = anchor2;

            var cb = new CanvasPathBuilder(ResourceCreator);
            cb.BeginFigure(StartPoint);
            cb.AddCubicBezier(Point1, Point2, Point3);
            cb.EndFigure(CanvasFigureLoop.Open);
            _path = CanvasGeometry.CreatePath(cb);

            IsDirty = false;
        }

        public override void Draw(CanvasDrawingSession ds) {
            if (_path != null)
                ds.DrawGeometry(_path, Color, 30);
        }

        public override BaseRenderItem HitTest(Vector2 point)
        {
            var anchor1 = new Vector2((float)Element1.ViewModel.Anchor.X, (float)Element1.ViewModel.Anchor.Y);
            var anchor2 = new Vector2((float)Element2.ViewModel.Anchor.X, (float)Element2.ViewModel.Anchor.Y);

            var distanceX = (float)anchor1.X - anchor2.X;

            var p2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            var p1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);
            var p0 = anchor1;
            var p3 = anchor2;

            var pointsOnCurve = new List<Point>();
            var numPoints = 10;
            for (var i = 10; i >= 0; i--)
                pointsOnCurve.Add(MathUtil.GetPointOnBezierCurve(p0.ToPoint(), p1, p2, p3.ToPoint(), 1.0 / numPoints * i));

            var minDist = pointsOnCurve.Select(p => MathUtil.Dist(p, new Point(point.X, point.Y))).Concat(new[] { double.PositiveInfinity }).Min();

            return minDist < 50 ? this : null;
        }
    }
}
