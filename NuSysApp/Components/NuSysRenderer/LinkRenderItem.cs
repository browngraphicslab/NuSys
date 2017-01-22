using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NusysIntermediate;
using SharpDX.Direct2D1;

namespace NuSysApp
{
    public class LinkRenderItem : BaseRenderItem
    {
        private LinkViewModel _vm;
        private CanvasGeometry _path;
        private CanvasGeometry _arrow;
        private Point _midPoint;
        private float _angle;
        public LinkViewModel ViewModel => _vm;

        public LinkRenderItem(LinkViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator):base(parent, resourceCreator)
        {
            _vm = vm;
            _vm.Controller.InElement.AnchorChanged += OnAnchorChanged;
            _vm.Controller.OutElement.AnchorChanged += OnAnchorChanged;

            _vm.Controller.LibraryElementController.Deleted += LibraryElementControllerOnDeleted;
        }

        /// <summary>
        /// Event handler called when the library element model is deleted
        /// </summary>
        /// <param name="sender"></param>
        private void LibraryElementControllerOnDeleted(object sender)
        {
            Parent?.RemoveChild(this);
            Dispose();
        }

        private void OnAnchorChanged(object sender, Point2d point2D)
        {
            IsDirty = true;
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
    
            _vm.Controller.InElement.AnchorChanged -= OnAnchorChanged;
            _vm.Controller.OutElement.AnchorChanged -= OnAnchorChanged;

            _vm.Controller.LibraryElementController.Deleted -= LibraryElementControllerOnDeleted;

            _path?.Dispose();
            _path = null;
            _arrow?.Dispose();
            _arrow = null;
            base.Dispose();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (IsDisposed)
                return;

            base.Update(parentLocalToScreenTransform);

            if (!IsDirty)
                return;

            
            var controller = (LinkController)_vm.Controller;
            if (controller.InElement == null || controller.OutElement == null)
                return;
            var anchor1 = new Vector2((float)controller.InElement.Anchor.X, (float)controller.InElement.Anchor.Y);
            var anchor2 = new Vector2((float)controller.OutElement.Anchor.X, (float)controller.OutElement.Anchor.Y);

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

            if ((_vm.Controller.LibraryElementController.LibraryElementModel as LinkLibraryElementModel)?.Direction !=
                NusysConstants.LinkDirection.None)
            {
                var lowerAnchor = anchor1.Y <= anchor2.Y ? anchor1.Y : anchor2.Y;
                var higherAnchor = anchor1.Y > anchor2.Y ? anchor1.Y : anchor2.Y;
                var leftAnchor = anchor1.X <= anchor2.X ? anchor1.X : anchor2.X;
                var rightAnchor = anchor1.X > anchor2.X ? anchor1.X : anchor2.X;
                var midPointX = leftAnchor + (rightAnchor - leftAnchor)/2;
                var midPointY = lowerAnchor + (higherAnchor - lowerAnchor)/2;
                _midPoint = new Point(midPointX, midPointY);
                var apex = new Point(midPointX, midPointY);
                var leftLeg = new Point(midPointX - 10, midPointY + 20);
                var rightLeg = new Point(midPointX + 10, midPointY + 20);
                _angle = (float) Math.Atan2(anchor1.Y - anchor2.Y, anchor1.X - anchor2.X);

                _arrow = CanvasGeometry.CreatePolygon(ResourceCreator,
                    new[] {apex.ToSystemVector2(), leftLeg.ToSystemVector2(), rightLeg.ToSystemVector2()});
            }

            IsDirty = false;
        }

        public override void Draw(CanvasDrawingSession ds) {
            if (IsDisposed || SessionController.Instance.SessionSettings.LinksVisible == LinkVisibilityOption.NoLinks || 
                (SessionController.Instance.SessionSettings.LinksVisible == LinkVisibilityOption.VisibleWhenSelected && 
                !SessionController.Instance.SessionView.FreeFormViewer.Selections.Any(i => i.ViewModel.Controller.Id == _vm.Controller.InElement.Id || i.ViewModel.Controller.Id == _vm.Controller.OutElement.Id)))
                return;

            if (_path != null)
            {
                ds.DrawGeometry(_path, Constants.LinkColor, 30);
            }
            if ((_vm.Controller.LibraryElementController.LibraryElementModel as LinkLibraryElementModel)?.Direction == NusysConstants.LinkDirection.Forward)
            {
                Matrix3x2 originalTranform = ds.Transform;
                ds.Transform = Matrix3x2.CreateRotation(_angle + (float)Math.PI / 2, new Vector2((float)_midPoint.X, (float)_midPoint.Y)) * ds.Transform;
                ds.DrawGeometry(_arrow, Colors.Black, 12);
                ds.Transform = originalTranform;
            }
            else if (
              (_vm.Controller.LibraryElementController.LibraryElementModel as LinkLibraryElementModel)?.Direction == NusysConstants.LinkDirection.Backward)
            {
                Matrix3x2 originalTranform = ds.Transform;
                ds.Transform = Matrix3x2.CreateRotation(_angle - (float)Math.PI / 2, new Vector2((float)_midPoint.X, (float)_midPoint.Y)) * ds.Transform;
                ds.DrawGeometry(_arrow, Colors.Black, 12);
                ds.Transform = originalTranform;
            }
        }

        public override BaseRenderItem HitTest(Vector2 screenPoint)
        {
            var worldPoint = Vector2.Transform(screenPoint, Transform.ScreenToLocalMatrix);
            var controller = _vm.Controller;
            var anchor1 = new Point((float)controller.InElement.Anchor.X, (float)controller.InElement.Anchor.Y);
            var anchor2 = new Point((float)controller.OutElement.Anchor.X, (float)controller.OutElement.Anchor.Y);

            var distanceX = (float)anchor1.X - anchor2.X;
            
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
