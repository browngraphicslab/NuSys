﻿using System;
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
        private Rect _measure = new Rect();
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

        public override void OnTapped(CanvasPointer pointer)
        {
            base.OnTapped(pointer);
        }

        public override void Dispose()
        {
            base.Dispose();
            _triangle.Dispose();
            _triangle = null;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);
            ds.FillGeometry(_triangle, Colors.Black);
            _triangleBounds = _triangle.ComputeBounds(ds.Transform);
        }

        public override Rect GetMeasure()
        {
            return _triangleBounds;
        }

        public override BaseRenderItem HitTest(Vector2 point)
        {
            return _triangleBounds.Contains(new Point(point.X, point.Y)) ? this : null;
        }
    }
}