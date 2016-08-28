using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class InkRenderItem : BaseRenderItem
    {
        private ElementViewModel _vm;
        private ConcurrentQueue<InkStroke> _inkStrokes = new ConcurrentQueue<InkStroke>();
        private bool _isEraser;
        private Color _drawingColor = Colors.Black;
        private ConcurrentQueue<InkPoint> _currentStroke = new ConcurrentQueue<InkPoint>();
        private InkStroke _currentInkStroke;
        private InkManager _inkManager = new InkManager();
        private Matrix3x2 _transform = Matrix3x2.Identity;
        private InkStrokeBuilder _builder;
        public InkStroke LatestStroke { get; set; }


        public InkRenderItem(CollectionRenderItem parent, CanvasAnimatedControl resourceCreator):base(parent, resourceCreator)
        {
        }

        public override void CreateResources()
        {
            _builder = new InkStrokeBuilder();
            base.CreateResources();
        }

        public override void Dispose()
        {
            base.Dispose();
            _vm = null;
            _inkStrokes = null;
        }

        public void StartInkByEvent(CanvasPointer e)
        {
            _transform = Win2dUtil.Invert(NuSysRenderer.Instance.GetTransformUntil(this));

                _currentStroke = new ConcurrentQueue<InkPoint>();

                _isEraser = e.Pointer.Properties.IsEraser || e.Pointer.Properties.IsRightButtonPressed;
                if (_isEraser)
                    _drawingColor = Colors.DarkRed;
                else
                    _drawingColor = Colors.Black;

                _currentStroke = new ConcurrentQueue<InkPoint>();

                foreach (var p in PointerPoint.GetIntermediatePoints(e.Pointer.PointerId).Reverse())
                {
                    var np = Vector2.Transform(new Vector2((float) p.RawPosition.X, (float) p.RawPosition.Y), _transform);
                    _currentStroke.Enqueue(new InkPoint(new Point(np.X, np.Y), p.Properties.Pressure));
                }

        }

        public void UpdateInkByEvent(CanvasPointer e)
        {

            foreach (var p in PointerPoint.GetIntermediatePoints(e.Pointer.PointerId).Reverse())
            {
                var np = Vector2.Transform(new Vector2((float) p.RawPosition.X, (float) p.RawPosition.Y), _transform);
                _currentStroke.Enqueue(new InkPoint(new Point(np.X, np.Y), p.Properties.Pressure));
            }
            
        }

        public void StopInkByEvent(CanvasPointer e)
        {
            _drawingColor = Colors.Black;

            foreach (var p in PointerPoint.GetIntermediatePoints(e.Pointer.PointerId).Reverse())
            {
                var np = Vector2.Transform(new Vector2((float) p.RawPosition.X, (float) p.RawPosition.Y), _transform);
                _currentStroke.Enqueue(new InkPoint(new Point(np.X, np.Y), p.Properties.Pressure));
            }

            var builder = new InkStrokeBuilder();
            LatestStroke = builder.CreateStrokeFromInkPoints(_currentStroke.ToArray(), Matrix3x2.Identity);
            LatestStroke.DrawingAttributes = GetDrawingAttributes();


            if (_isEraser)
            {
                var allStrokes = _inkManager.GetStrokes().ToArray();
                var thisStroke = _currentStroke.Select(p => p.Position);
                var thisLineString = thisStroke.GetLineString();

                foreach (var otherStroke in allStrokes)
                {
                    var pts = otherStroke.GetInkPoints().Select(p => p.Position);
                    if (pts.Count() < 2)
                        continue;
                    if (thisLineString.Intersects(pts.GetLineString()))
                    {
                        otherStroke.Selected = true;
                    }
                }

                var selected = GetSelectedStrokes();
                _inkManager.DeleteSelected();

                foreach (var s in selected)
                {
                    //     InkStrokeRemoved?.Invoke(this, s);
                }

                _inkManager.SelectWithPolyLine(thisStroke);
                selected = GetSelectedStrokes();
                _inkManager.DeleteSelected();

                foreach (var s in selected)
                {
                    //         InkStrokeRemoved?.Invoke(this, s);
                }

            }
            else
            {
                _inkManager.AddStroke(LatestStroke);
                _inkStrokes.Enqueue(LatestStroke);
                //  InkStrokeAdded?.Invoke(this, stroke);
            }
            _inkStrokes =  new ConcurrentQueue<InkStroke>(_inkManager.GetStrokes());
          
            _currentStroke = new ConcurrentQueue<InkPoint>();
        }

        public void AddStroke(InkStroke stroke)
        {
            _inkStrokes.Enqueue(stroke);
        }

        public override void Update()
        {
            
            if (_builder == null)
            {
                _builder = new InkStrokeBuilder();
            }

            if (_currentStroke.Count > 2)
            {
                _currentInkStroke = _builder.CreateStrokeFromInkPoints(_currentStroke.ToArray(), Matrix3x2.Identity);
                _currentInkStroke.DrawingAttributes = GetDrawingAttributes();
                if (_isEraser)
                    _currentInkStroke.DrawingAttributes = new InkDrawingAttributes { Color = Colors.DarkRed };
            }
            
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var strokes = _inkStrokes.ToList();
            if (_currentStroke.Count > 2)
            {
                strokes.Add(_currentInkStroke);
            }
            ds.DrawInk(strokes);
        }

        private InkDrawingAttributes GetDrawingAttributes()
        {
            var _drawingAttributes = new InkDrawingAttributes
            {
                PenTip = PenTipShape.Circle,
                PenTipTransform = Matrix3x2.CreateRotation((float)Math.PI / 4),
                IgnorePressure = false,
                Size = new Size(4, 4),
                Color = Colors.Black
            };

            return _drawingAttributes;
        }

        private IEnumerable<InkStroke> GetSelectedStrokes()
        {
            var selectedStrokes = new List<InkStroke>();
            return _inkManager.GetStrokes().ToArray().Where(stroke => stroke.Selected == true);
        }
    }
}
