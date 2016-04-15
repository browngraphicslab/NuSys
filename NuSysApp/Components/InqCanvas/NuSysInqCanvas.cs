﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml.Media;
using System.Numerics;
using Windows.UI;
using Windows.UI.Input.Inking;
using Microsoft.Graphics.Canvas.Geometry;
using NetTopologySuite.Geometries;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class NuSysInqCanvas : WetDryInkCanvas
    {   
        public delegate void AdornmentEventHandler(WetDryInkCanvas canvas, InkStroke adornment);
        public event AdornmentEventHandler AdornmentAdded;
        public event AdornmentEventHandler AdornmentRemoved;

        private HashSet<CanvasGeometry> _adornments = new HashSet<CanvasGeometry>();
        private Dictionary<InkStroke, CanvasGeometry> _inkStrokes = new Dictionary<InkStroke, CanvasGeometry>();

        public NuSysInqCanvas(CanvasControl wetCanvas, CanvasControl dryCanvas) : base(wetCanvas, dryCanvas)
        {
        }

        public void RemoveAdorment(InkStroke inkStroke, bool fireEvent = true)
        {
            if (!_inkStrokes.ContainsKey(inkStroke))
                return;

            var geom = _inkStrokes[inkStroke];

            _adornments.Remove(geom);
            if (fireEvent)
                AdornmentAdded?.Invoke(this, inkStroke);
        }

        public void AddAdorment(IEnumerable<InkPoint> points, bool fireEvent = true)
        {
            AddAdorment(strokeBuilder.CreateStrokeFromInkPoints(points, Matrix3x2.Identity), fireEvent);
        }

        public void AddAdorment(InkStroke stroke, bool fireEvent = true)
        {
            var multipoint = new MultiPoint(stroke.GetInkPoints().Select(p => new NetTopologySuite.Geometries.Point(p.Position.X, p.Position.Y)).ToArray());
            var ch = multipoint.ConvexHull().Coordinates.Select(p => new Vector2((float)p.X, (float)p.Y)).ToArray();
            var geom = CanvasGeometry.CreatePolygon(_dryCanvas, ch);
            _adornments.Add(geom);

            if (fireEvent)
                AdornmentAdded?.Invoke(this, stroke); 
        }

        protected override void OnDryCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            base.OnDryCanvasDraw(sender, args);

            var ds = args.DrawingSession;
            var inv = (MatrixTransform)Transform.Inverse.Inverse;
            var m = new Matrix3x2((float)inv.Matrix.M11, (float)inv.Matrix.M12, (float)inv.Matrix.M21,
                (float)inv.Matrix.M22, (float)inv.Matrix.OffsetX, (float)inv.Matrix.OffsetY);

            ds.Transform = m;
            foreach(var adornment in _adornments) {
                ds.FillGeometry(adornment, Colors.Red);
            }
        }

        protected override void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(sender, e);
        }

    }
}
