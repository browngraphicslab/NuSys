using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using GeoAPI.Geometries;

namespace App2
{
    public class WetDryInkCanvas
    {
        public delegate void InkStrokeEventHandler(WetDryInkCanvas canvas, InkStroke stroke);
        public event InkStrokeEventHandler InkStrokeAdded;
        public event InkStrokeEventHandler InkStrokeRemoved;

        private CanvasControl _wetCanvas;
        private CanvasControl _dryCanvas;
        private InkStrokeBuilder strokeBuilder = new InkStrokeBuilder();

        private InkManager _inkManager = new InkManager();

        private List<InkPoint> _currentStroke;
        private InkStrokeBuilder _strokeBuilder = new InkStrokeBuilder();
        private bool _isEraser;

        private Color _drawingColor = Colors.Black;
        public CompositeTransform Transform { get; set; }
        
        
        public WetDryInkCanvas(CanvasControl wetCanvas, CanvasControl dryCanvas)
        {
            Transform = new CompositeTransform();

            _wetCanvas = wetCanvas;
            _dryCanvas = dryCanvas;

            _wetCanvas.PointerPressed += OnPointerPressed;
            _wetCanvas.PointerReleased += OnPointerReleased;
            strokeBuilder.SetDefaultDrawingAttributes( GetDrawingAttributes());
           
            _wetCanvas.Draw += OnWetCanvasDraw;
            _dryCanvas.Draw += OnDryCanvasDraw;           
        }

        private InkDrawingAttributes GetDrawingAttributes()
        {
            var _drawingAttributes = new InkDrawingAttributes {
                PenTip = PenTipShape.Circle,
                PenTipTransform = Matrix3x2.CreateRotation((float)Math.PI / 4),
                IgnorePressure = false,
                Size = new Size(4, 4),
                Color = _drawingColor
            };
    
            return _drawingAttributes;
        }

        public void Redraw()
        {
            _dryCanvas.Invalidate();
        }

        private void OnDryCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;
                ds.Clear(Colors.Transparent);

            var dryStrokes = _inkManager.GetStrokes().ToArray();
                foreach (var s in dryStrokes)
                {
                    var attr = GetDrawingAttributes();  
                    s.DrawingAttributes = attr;                    
                }

                var inv = (MatrixTransform)Transform.Inverse.Inverse;
                var m = new Matrix3x2((float)inv.Matrix.M11, (float)inv.Matrix.M12, (float)inv.Matrix.M21,
                    (float)inv.Matrix.M22, (float)inv.Matrix.OffsetX, (float)inv.Matrix.OffsetY);

                ds.Transform = m;  
                ds.DrawInk(dryStrokes);
        }

        private void OnWetCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_currentStroke == null)
                return;
            var ds = args.DrawingSession;
                ds.Clear(Colors.Transparent);

                if (_currentStroke != null) {  
                    try {     
                    var stroke =  strokeBuilder.CreateStrokeFromInkPoints(_currentStroke, Matrix3x2.Identity);
                    var attr = GetDrawingAttributes();
                    attr.Size = new Size(4 * Transform.ScaleX, 4 * Transform.ScaleY);
                    stroke.DrawingAttributes = attr;
                    ds.DrawInk(new List<InkStroke> { stroke });
                    } catch
                    {
                        Debug.WriteLine("couldn't draw wet stroke");
                    }
                }
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Pen)
                return;

            _isEraser = e.GetCurrentPoint(null).Properties.IsEraser;
            if (_isEraser)
                _drawingColor = Colors.DarkRed;
            else
                _drawingColor = Colors.Black;
            
            _currentStroke = new List<InkPoint>();      

            foreach (var p in e.GetIntermediatePoints(_wetCanvas).Reverse())
            {
                _currentStroke.Add(new InkPoint(p.RawPosition, p.Properties.Pressure));
            }

            _wetCanvas.PointerMoved += OnPointerMoved;
            _wetCanvas.Invalidate();
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Pen)
                return;
            _wetCanvas.PointerMoved -= OnPointerMoved;

            _drawingColor = Colors.Black;

            foreach (var p in e.GetIntermediatePoints(_wetCanvas).Reverse())
            {
                _currentStroke.Add(new InkPoint(p.RawPosition, p.Properties.Pressure));
            }
            var stroke = strokeBuilder.CreateStrokeFromInkPoints(_currentStroke, Matrix3x2.Identity);
            _currentStroke = null;
            
            var inv = (MatrixTransform)Transform.Inverse;
            var m = new Matrix3x2((float)inv.Matrix.M11, (float)inv.Matrix.M12, (float)inv.Matrix.M21,
                (float)inv.Matrix.M22, (float)inv.Matrix.OffsetX, (float)inv.Matrix.OffsetY);
            stroke.PointTransform = m;

            if (_isEraser)
            {

                var allStrokes = _inkManager.GetStrokes().ToArray();
                var thisStroke = stroke.GetInkPoints().Select(p => inv.TransformPoint(p.Position));
                var thisLineString = thisStroke.GetLineString(); ;

                foreach (var otherStroke in allStrokes)
                {
                    var otherMatrix = otherStroke.PointTransform;
                    var mt = new Matrix { M11 = otherMatrix.M11, M12 = otherMatrix.M12, M21 = otherMatrix.M21, M22 = otherMatrix.M22, OffsetX = otherMatrix.M31, OffsetY = otherMatrix.M32 };
                    var mt2 = new MatrixTransform { Matrix = mt };
                    var pts = otherStroke.GetInkPoints().Select(p => mt2.TransformPoint(p.Position) );
                    if (thisLineString.Intersects(pts.GetLineString())) {
                        otherStroke.Selected = true;
                    }
                }

                var selected = GetSelectedStrokes();
                _inkManager.DeleteSelected();
                
                foreach (var s in selected)
                {
                    InkStrokeRemoved?.Invoke(this, s);
                }

                _inkManager.SelectWithPolyLine(thisStroke);
                selected = GetSelectedStrokes();
                _inkManager.DeleteSelected();

                foreach (var s in selected)
                {
                    InkStrokeRemoved?.Invoke(this, s);
                }

            } else { 
                _inkManager.AddStroke(stroke);
            }

            _wetCanvas.Invalidate();
            _dryCanvas.Invalidate();
            
        }

        private IEnumerable<InkStroke> GetSelectedStrokes()
        {
            var selectedStrokes = new List<InkStroke>();
            return _inkManager.GetStrokes().ToArray().Where(stroke => stroke.Selected == true);
        }


        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Pen)
                return;

            foreach (var p in e.GetIntermediatePoints(_wetCanvas).Reverse())
            {
                _currentStroke.Add(new InkPoint(p.RawPosition, p.Properties.Pressure));
            }            
          
            _wetCanvas.Invalidate();
        }
    }
}

static class Extensions {
    public static GeoAPI.Geometries.ILineString GetLineString(this IEnumerable<Point> s)
    {
        GeoAPI.Geometries.Coordinate[] coords;
        coords = new GeoAPI.Geometries.Coordinate[s.Count()];
        int i = 0;
        foreach (Point pt in s)
        {
            coords[i] = new GeoAPI.Geometries.Coordinate(pt.X, pt.Y);
            i++;
        }

        return new NetTopologySuite.Geometries.LineString(coords);
    }
}