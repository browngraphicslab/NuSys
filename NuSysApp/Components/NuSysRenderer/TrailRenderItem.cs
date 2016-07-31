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
using SharpDX.Direct2D1;

namespace NuSysApp
{
    public class TrailRenderItem : BaseRenderItem
    {
        private PresentationLinkViewModel _vm;

        public TrailRenderItem(PresentationLinkViewModel vm, ICanvasResourceCreator resourceCreator):base(resourceCreator)
        {
            _vm = vm;
        }

        public override void Dispose()
        {
            base.Dispose();
            _vm = null;
        }

        public override void Draw(CanvasDrawingSession ds) {

            var anchor1 = new Vector2((float)_vm.InAnchor.X, (float)_vm.InAnchor.Y);
            var anchor2 = new Vector2((float)_vm.OutAnchor.X, (float)_vm.OutAnchor.Y);

            var distanceX = (float)anchor1.X - anchor2.X;
            var distanceY = (float)anchor1.Y - anchor2.Y;

            var Point2 = new Vector2(anchor1.X - distanceX / 2, anchor2.Y);
            var Point1 = new Vector2(anchor2.X + distanceX / 2, anchor1.Y);
            var StartPoint = anchor1;
            var Point3 = anchor2;

            var cb = new CanvasPathBuilder(ds);
            cb.BeginFigure(StartPoint);
            cb.AddCubicBezier(Point1,Point2,Point3);
            cb.EndFigure(CanvasFigureLoop.Open);
            var path = CanvasGeometry.CreatePath(cb);
            ds.DrawGeometry(path, Colors.PaleVioletRed, 20);
        }
    }
}
