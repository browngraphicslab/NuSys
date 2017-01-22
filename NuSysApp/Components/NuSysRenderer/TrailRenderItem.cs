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
    public class TrailRenderItem : BaseRenderItem
    {
        private PresentationLinkViewModel _vm;
        private CanvasGeometry _path;
        private ElementController _elementController1;
        private ElementController _elementController2;
        private CanvasGeometry _arrow;
        private Point _midPoint;
        private float _angle;

        private bool _needsRedraw;

        public PresentationLinkViewModel ViewModel => _vm;

        public TrailRenderItem(PresentationLinkViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator):base(parent, resourceCreator)
        {
            _vm = vm;
            _elementController1 = SessionController.Instance.ElementModelIdToElementController[_vm.Model.InElementId];
            _elementController2 = SessionController.Instance.ElementModelIdToElementController[_vm.Model.OutElementId];
            _elementController1.PositionChanged += ElementController1OnPositionChanged;
            _elementController1.SizeChanged += ElementController1OnSizeChanged;
            _elementController2.PositionChanged += ElementController1OnPositionChanged;
            _elementController2.SizeChanged += ElementController1OnSizeChanged;
        }

        private void ElementController1OnSizeChanged(object source, double width, double height)
        {
            _needsRedraw = true;
        }

        private void ElementController1OnPositionChanged(object source, double d, double d1, double dx, double dy)
        {
            _needsRedraw = true;
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            _elementController1.PositionChanged -= ElementController1OnPositionChanged;
            _elementController1.SizeChanged -= ElementController1OnSizeChanged;
            _elementController2.PositionChanged -= ElementController1OnPositionChanged;
            _elementController2.SizeChanged -= ElementController1OnSizeChanged;
            _elementController1 = null;
            _elementController2 = null;
            _vm = null;
            _path?.Dispose();
            _path = null;

            base.Dispose();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (IsDisposed)
                return;

            
            if (!IsDirty || _vm == null)
                return;
            var anchor1 = new Vector2((float)_vm.InAnchor.X, (float)_vm.InAnchor.Y);
            var anchor2 = new Vector2((float)_vm.OutAnchor.X, (float)_vm.OutAnchor.Y);

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

            var lowerAnchor = anchor1.Y <= anchor2.Y ? anchor1.Y : anchor2.Y;
            var higherAnchor = anchor1.Y > anchor2.Y ? anchor1.Y : anchor2.Y;
            var leftAnchor = anchor1.X <= anchor2.X ? anchor1.X : anchor2.X;
            var rightAnchor = anchor1.X > anchor2.X ? anchor1.X : anchor2.X;
            var midPointX = leftAnchor + (rightAnchor - leftAnchor) / 2;
            var midPointY = lowerAnchor + (higherAnchor - lowerAnchor) / 2;
            _midPoint = new Point(midPointX, midPointY);
            var apex = new Point(midPointX, midPointY);
            var leftLeg = new Point(midPointX - 10, midPointY + 20);
            var rightLeg = new Point(midPointX + 10, midPointY + 20);
            _angle = (float)Math.Atan2(anchor1.Y - anchor2.Y, anchor1.X - anchor2.X);

            _arrow = CanvasGeometry.CreatePolygon(ResourceCreator,
                new[] { apex.ToSystemVector2(), leftLeg.ToSystemVector2(), rightLeg.ToSystemVector2() });

            base.Update(parentLocalToScreenTransform);
        }

        public override void Draw(CanvasDrawingSession ds) {
            if (IsDisposed || SessionController.Instance.SessionSettings.LinksVisible == LinkVisibilityOption.NoLinks || SessionController.Instance.SessionSettings.LinksVisible == LinkVisibilityOption.NoTrails ||
                (SessionController.Instance.SessionSettings.LinksVisible == LinkVisibilityOption.VisibleWhenSelected &&
                !SessionController.Instance.SessionView.FreeFormViewer.Selections.Any(i => i.ViewModel.Controller.Id == _vm.Model.InElementId|| i.ViewModel.Controller.Id == _vm.Model.OutElementId)))
                return;

            if (_path != null)
            {
                ds.DrawGeometry(_path, Constants.TrailColor, 30);
            }

            Matrix3x2 originalTranform = ds.Transform;
            ds.Transform = Matrix3x2.CreateRotation(_angle + (float)Math.PI / 2, new Vector2((float)_midPoint.X, (float)_midPoint.Y)) * ds.Transform;
            ds.DrawGeometry(_arrow, Colors.Black, 12);
            ds.Transform = originalTranform;
        }

        public override BaseRenderItem HitTest(Vector2 screenPoint)
        {
            var worldPoint = Vector2.Transform(screenPoint, Transform.ScreenToLocalMatrix);
            var anchor1 = new Point((float)_vm.InAnchor.X, (float)_vm.InAnchor.Y);
            var anchor2 = new Point((float)_vm.OutAnchor.X, (float)_vm.OutAnchor.Y);

            var distanceX = (float)anchor1.X - anchor2.X;

            ;

            var p2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            var p1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);
            var p0 = anchor1;
            var p3 = anchor2;

            var pointsOnCurve = new List<Point>();
            var numPoints = Math.Min(30, MathUtil.Dist(anchor1, anchor2) / 90);
            for (var i = numPoints; i >= 0; i--)
                pointsOnCurve.Add(MathUtil.GetPointOnBezierCurve(p0, p1, p2, p3, 1.0 / numPoints * i));

            var minDist = pointsOnCurve.Select(p => MathUtil.Dist(p, new Point(worldPoint.X, worldPoint.Y))).Concat(new[] { double.PositiveInfinity }).Min();

            return minDist < 50 ? this : null;
        }
    }
}
