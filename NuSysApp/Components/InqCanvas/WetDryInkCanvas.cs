using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using GeoAPI.Geometries;
using Windows.UI.Core;

namespace NuSysApp
{
    public class WetDryInkCanvas
    {
        public delegate void InkStrokeEventHandler(WetDryInkCanvas canvas, InkStroke stroke);
        public event InkStrokeEventHandler InkStrokeAdded;
        public event InkStrokeEventHandler InkStrokeRemoved;

        protected CanvasControl _wetCanvas;
        protected CanvasControl _dryCanvas;
        protected InkStrokeBuilder strokeBuilder = new InkStrokeBuilder();
        protected InkManager _inkManager = new InkManager();
        protected List<InkPoint> _currentStroke = new List<InkPoint>();
        protected List<InkStroke> _dryStrokes = new List<InkStroke>();
        protected InkStrokeBuilder _strokeBuilder = new InkStrokeBuilder();
        protected bool _isEraser;
        protected Pointer _capturedPointer;
        protected Color _drawingColor = Colors.Black;
        protected GeneralTransform _inverseTransform;
        protected CompositeTransform _transform;
        private bool _shiftIsDown;
        private bool _mouseDown;

        public CompositeTransform Transform { get
            {
                return _transform;
            }
            set
            {
                _transform = value;
                _inverseTransform = _transform.Inverse;
            }
        }
        
        
        public WetDryInkCanvas(CanvasControl wetCanvas, CanvasControl dryCanvas)
        {
            Transform = new CompositeTransform();

            _wetCanvas = wetCanvas;
            _dryCanvas = dryCanvas;

            _wetCanvas.PointerPressed += OnPointerPressed;
            _wetCanvas.PointerReleased += OnPointerReleased;
            _wetCanvas.PointerMoved -= OnPointerMoved;
            strokeBuilder.SetDefaultDrawingAttributes( GetDrawingAttributes());
           
            _wetCanvas.Draw += OnWetCanvasDraw;
            _dryCanvas.Draw += OnDryCanvasDraw;
            CoreWindow.GetForCurrentThread().KeyDown += OnKeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += OnKeyUp;
            _mouseDown = false;
        }

        /// <summary>
        /// Resets the virtual key once any key is released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnKeyUp(CoreWindow sender, KeyEventArgs args)
        {
            _shiftIsDown = false;
        }

        /// <summary>
        /// Keeps track of the currently pressed key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Shift)
            {
                _shiftIsDown = true;
            }
        }

        public void Dispose()
        {
            _wetCanvas.PointerPressed -= OnPointerPressed;
            _wetCanvas.PointerReleased -= OnPointerReleased;
            _wetCanvas.Draw -= OnWetCanvasDraw;
            _dryCanvas.Draw -= OnDryCanvasDraw;
        }

        public InkStroke AddStroke(IEnumerable<InkPoint> points)
        {
            var stroke = strokeBuilder.CreateStrokeFromInkPoints(points, Matrix3x2.Identity);
            AddStroke(stroke);
            return stroke;
        }

        public void AddStroke(InkStroke stroke)
        {
            try
            {
                _inkManager.AddStroke(stroke);

                _dryStrokes = _inkManager.GetStrokes().ToList();
                Redraw();
            }
            catch (Exception e)
            {
                Redraw();
            }
        }

        public void RemoveStroke(InkStroke stroke)
        {
            stroke.Selected = true;
            _inkManager.DeleteSelected();
            _dryStrokes = _inkManager.GetStrokes().ToList();
            Redraw();
        }

        protected InkDrawingAttributes GetDrawingAttributes()
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

        protected virtual void OnDryCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;
                ds.Clear(Colors.Transparent);

            var dryStrokes = _dryStrokes;
            var aa = GetDrawingAttributes();
            foreach (var s in dryStrokes)
            {
                var attr = aa;
                attr.Color = Colors.Black;
                s.DrawingAttributes = attr;                    
            }

            var inv = (MatrixTransform)Transform.Inverse.Inverse;
            var m = new Matrix3x2((float)inv.Matrix.M11, (float)inv.Matrix.M12, (float)inv.Matrix.M21,
                (float)inv.Matrix.M22, (float)inv.Matrix.OffsetX, (float)inv.Matrix.OffsetY);

            ds.Transform = m;  
            ds.DrawInk(dryStrokes);
        }

        protected virtual void OnWetCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;
                ds.Clear(Colors.Transparent);

                if (_currentStroke.Count > 0) {  
                    try {     
                    var stroke =  strokeBuilder.CreateStrokeFromInkPoints(_currentStroke, Matrix3x2.Identity);
                    stroke.DrawingAttributes = GetDrawingAttributes();

                    var inv = (MatrixTransform)Transform.Inverse.Inverse;
                    var m = new Matrix3x2((float)inv.Matrix.M11, (float)inv.Matrix.M12, (float)inv.Matrix.M21,
                        (float)inv.Matrix.M22, (float)inv.Matrix.OffsetX, (float)inv.Matrix.OffsetY);

                    ds.Transform = m;
                    ds.DrawInk(new List<InkStroke> { stroke });
                    } catch
                    {
                        Debug.WriteLine("couldn't draw wet stroke");
                    }
                }
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // We only register ink if shift is down or the pen is being used. Otherwise, return.
            if (!(_shiftIsDown || e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen))
                return;

            _mouseDown = true;
            _currentStroke.Clear();

            _capturedPointer = e.Pointer;
            _wetCanvas.CapturePointer(_capturedPointer);

            _isEraser = e.GetCurrentPoint(null).Properties.IsEraser;
            if (_isEraser)
                _drawingColor = Colors.DarkRed;
            else
                _drawingColor = Colors.Black;
            
            _currentStroke = new List<InkPoint>();      

            foreach (var p in e.GetIntermediatePoints(_wetCanvas).Reverse())
            {
                _currentStroke.Add(new InkPoint(_inverseTransform.TransformPoint(p.RawPosition), p.Properties.Pressure));
            }

            _wetCanvas.PointerMoved += OnPointerMoved;
            _wetCanvas.Invalidate();
        }

        protected virtual void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            
            // We only register ink if shift is down or the pen is being used. Otherwise, return.
            if (!(_shiftIsDown || e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen))
                return;

            _mouseDown = false;


            _wetCanvas.PointerMoved -= OnPointerMoved;

            _drawingColor = Colors.Black;

            foreach (var p in e.GetIntermediatePoints(_wetCanvas).Reverse())
            {
                _currentStroke.Add(new InkPoint(_inverseTransform.TransformPoint(p.RawPosition), p.Properties.Pressure));
            }
            InkStroke stroke = null;
            try
            {
                stroke = strokeBuilder.CreateStrokeFromInkPoints(_currentStroke, Matrix3x2.Identity);
            }
            catch(Exception ee)
            {
                return;
            }
                             

            if (_isEraser)
            {
                var allStrokes = _inkManager.GetStrokes().ToArray();
                var thisStroke = stroke.GetInkPoints().Select(p => p.Position);
                var thisLineString = thisStroke.GetLineString(); ;

                foreach (var otherStroke in allStrokes)
                {
                    var pts = otherStroke.GetInkPoints().Select(p => p.Position );
                    if (pts.Count() < 2)
                        continue;
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
                InkStrokeAdded?.Invoke(this, stroke);
            }
            _dryStrokes = _inkManager.GetStrokes().ToList();
            if (_capturedPointer != null) { 
                _wetCanvas.ReleasePointerCapture(_capturedPointer);
            }
            _wetCanvas.Invalidate();
            _dryCanvas.Invalidate();            
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            // We only register ink if shift is down or the pen is being used. Otherwise, return.
            if (!(_shiftIsDown || e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen))
                return;

            if (!_mouseDown)
                return;

            foreach (var p in e.GetIntermediatePoints(_wetCanvas).Reverse())
            {
                _currentStroke.Add(new InkPoint(_inverseTransform.TransformPoint(p.RawPosition), p.Properties.Pressure));
            }             
          
            _wetCanvas.Invalidate();
        }

        private IEnumerable<InkStroke> GetSelectedStrokes()
        {
            var selectedStrokes = new List<InkStroke>();
            return _inkManager.GetStrokes().ToArray().Where(stroke => stroke.Selected == true);
        }
    }
}

public static class Extensions {
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
        if(coords.Count() < 2)
        {
            var c = new Coordinate[2];
            c[0] = coords[0];
            c[1] = coords[0];
            coords = c;
        }
        return new NetTopologySuite.Geometries.LineString(coords);
    }
}