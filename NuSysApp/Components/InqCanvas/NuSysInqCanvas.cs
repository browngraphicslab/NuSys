using System;
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


        private List<Color> _colors = new List<Color>();
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
                AdornmentRemoved?.Invoke(this, inkStroke);

            Redraw();
        }

        public InkStroke AddAdorment(IEnumerable<InkPoint> points, Color color, bool fireEvent = true)
        {
            var stroke = strokeBuilder.CreateStrokeFromInkPoints(points, Matrix3x2.Identity);
            AddAdorment(stroke, color, fireEvent);
            return stroke;
        }

        public void AddAdorment(InkStroke stroke, Color color, bool fireEvent = true)
        {
            _colors.Add(color);
            var multipoint = new MultiPoint(stroke.GetInkPoints().Select(p => new NetTopologySuite.Geometries.Point(p.Position.X, p.Position.Y)).ToArray());
            var ch = multipoint.ConvexHull().Coordinates.Select(p => new Vector2((float)p.X, (float)p.Y)).ToArray();
            var geom = CanvasGeometry.CreatePolygon(_dryCanvas, ch);
            _adornments.Add(geom);
            _inkStrokes.Add(stroke, geom);

            if (fireEvent)
                AdornmentAdded?.Invoke(this, stroke);

            Redraw();
        }

        protected override void OnDryCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            base.OnDryCanvasDraw(sender, args);

            var ads = _adornments.ToArray();
            var ds = args.DrawingSession;
            for(var i = 0; i < _adornments.Count; i++) { 
                ds.FillGeometry(ads[i], _colors[i]);
            }
        }

        protected override void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Pen)
                return;
   //         bool isEraser = e.GetCurrentPoint(null).Properties.IsEraser;
            base.OnPointerReleased(sender, e);
            
            if ( _isEraser && _currentStroke.Count > 0)
            {
                var currentStroke = _currentStroke.Select(p => p.Position).GetLineString();
                foreach(var stroke in _inkStrokes.Keys.ToArray())
                {
                    var pts = stroke.GetInkPoints().Select(p => p.Position).GetLineString();
                    if (currentStroke.Intersects(pts))
                    {
                        RemoveAdorment(stroke);
                    }
                }


            }

            _currentStroke.Clear();

            Redraw();
        }

    }
}
