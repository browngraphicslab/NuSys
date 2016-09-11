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

        private bool _needsRedraw;

        public PresentationLinkViewModel ViewModel => _vm;

        public TrailRenderItem(PresentationLinkViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator):base(parent, resourceCreator)
        {
            _vm = vm;
            _elementController1 = SessionController.Instance.IdToControllers[_vm.Model.OutElementId];
            _elementController2 = SessionController.Instance.IdToControllers[_vm.Model.InElementId];
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
            _elementController1.PositionChanged -= ElementController1OnPositionChanged;
            _elementController1.SizeChanged -= ElementController1OnSizeChanged;
            _elementController2.PositionChanged -= ElementController1OnPositionChanged;
            _elementController2.SizeChanged -= ElementController1OnSizeChanged;
            _elementController1 = null;
            _elementController2 = null;
            _vm = null;
            _path.Dispose();
            _path = null;
            base.Dispose();
        }

        public override void Update()
        {
            base.Update();
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
            

          

        }

        public override void Draw(CanvasDrawingSession ds) {
            if (_path != null)
                ds.DrawGeometry(_path, Colors.PaleVioletRed, 30);
        }
    }
}
