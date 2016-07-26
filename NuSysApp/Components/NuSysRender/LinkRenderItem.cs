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
    public class LinkRenderItem : BaseRenderItem
    {
        private LinkViewModel _vm;

        public LinkRenderItem(LinkViewModel vm)
        {
            _vm = vm;
        }

        public override void Draw(CanvasDrawingSession ds) {

            var controller = (LinkController)_vm.Controller;
            var anchor1 = new Vector2((float)controller.InElement.Anchor.X, (float)controller.InElement.Anchor.Y);
            var anchor2 = new Vector2((float)controller.OutElement.Anchor.X, (float)controller.OutElement.Anchor.Y);

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
            ds.DrawGeometry(path, Colors.DodgerBlue, 20);
            //ds.DrawGeometry( new BezierSegment {Point1 = Point1, Point2 = Point2, Point3 = Point3}, Colors.Black);
            //ds.FillRectangle( new Rect {X=_vm., Y= _vm.Y, Width = 100, Height=100}, Colors.Black);
        }
    }
}
