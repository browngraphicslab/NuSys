﻿using System;
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
      //  private ConcurrentQueue<InkStroke> _inkStrokes = new ConcurrentQueue<InkStroke>();
        private bool _isEraser;
        private Color _drawingColor = Colors.Black;
        private Queue<InkPoint> _currentStroke = new Queue<InkPoint>();
        private InkStroke _currentInkStroke;
        private InkManager _inkManager = new InkManager();
        private Matrix3x2 _transform = Matrix3x2.Identity;
        private InkStrokeBuilder _builder;
        private bool _isDrawing;
        public InkStroke LatestStroke { get; set; }
        public DateTime LatestStrokeAdded { get; set; }
        private List<InkStroke> _strokesToDraw = new List<InkStroke>();
        private CanvasRenderTarget _dryStrokesTarget;
        private bool _needsDryStrokesUpdate;
        private bool _needsWetStrokeUpdate;


        public InkRenderItem(CollectionRenderItem parent, CanvasAnimatedControl resourceCreator):base(parent, resourceCreator)
        {
        }

        public async override Task Load()
        {
           
            _dryStrokesTarget = new CanvasRenderTarget(ResourceCreator, new Size(ResourceCreator.Width, ResourceCreator.Height));
            base.CreateResources();
        }

        public override void Dispose()
        {
            base.Dispose();
            _vm = null;
        }

        public void UpdateDryInkTransform()
        {
            _transform = Win2dUtil.Invert(NuSysRenderer.Instance.GetTransformUntil(this));
            _needsDryStrokesUpdate = true;
        }

        public void StartInkByEvent(CanvasPointer e)
        {
            _isDrawing = true;
            _transform = Win2dUtil.Invert(NuSysRenderer.Instance.GetTransformUntil(this));


            _isEraser = e.Pointer.Properties.IsEraser || e.Pointer.Properties.IsRightButtonPressed;
            if (_isEraser)
                _drawingColor = Colors.DarkRed;
            else
                _drawingColor = Colors.Black;

            _currentStroke = new Queue<InkPoint>();

            foreach (var p in PointerPoint.GetIntermediatePoints(e.Pointer.PointerId).Reverse())
            {
                var np = Vector2.Transform(new Vector2((float) p.RawPosition.X, (float) p.RawPosition.Y), _transform);
                _currentStroke.Enqueue(new InkPoint(new Point(np.X, np.Y), p.Properties.Pressure));
            }

            _needsWetStrokeUpdate = true;

        }

        public void UpdateInkByEvent(CanvasPointer e)
        {
            foreach (var p in PointerPoint.GetIntermediatePoints(e.Pointer.PointerId).Reverse())
            {
                var np = Vector2.Transform(new Vector2((float) p.RawPosition.X, (float) p.RawPosition.Y), _transform);
                _currentStroke.Enqueue(new InkPoint(new Point(np.X, np.Y), p.Properties.Pressure));
            }

            _needsWetStrokeUpdate = true;
        }

        public void StopInkByEvent(CanvasPointer e)
        {
            _drawingColor = Colors.Black;
            var cout = PointerPoint.GetIntermediatePoints(e.Pointer.PointerId).Count;
            Debug.WriteLine(_currentStroke.Count);
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

                LatestStrokeAdded = DateTime.Now;
                //  InkStrokeAdded?.Invoke(this, stroke);
            } 
            _currentStroke = new Queue<InkPoint>();
            _strokesToDraw = _inkManager.GetStrokes().ToList();

            _needsDryStrokesUpdate = true;
            _needsWetStrokeUpdate = true;
        }

        public void AddStroke(InkStroke stroke)
        {
           // _inkStrokes.Enqueue(stroke);
        }

        public void RemoveLatestStroke()
        {
            if (LatestStroke != null) { 
                LatestStroke.Selected = true;
                _inkManager.DeleteSelected();
                LatestStroke = null;
            }
            _strokesToDraw = _inkManager.GetStrokes().ToList();
        }

        public override void Update()
        {
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (_needsDryStrokesUpdate)
            {
                if (_dryStrokesTarget != null) { 
                    using (var dss = _dryStrokesTarget.CreateDrawingSession())
                    {
                        dss.Clear(Colors.Transparent);
                        var dryStrokes = _strokesToDraw;
                        var aa = GetDrawingAttributes();
                        foreach (var s in dryStrokes)
                        {
                            var attr = aa;
                            attr.Color = Colors.Black;
                            s.DrawingAttributes = attr;
                        }
                        dss.Transform = ds.Transform;
                        dss.DrawInk(dryStrokes);

                        _needsDryStrokesUpdate = false;
                    }
                }
            }

            var orgTransform = ds.Transform;
            ds.Transform = Matrix3x2.Identity;

            if (_dryStrokesTarget != null)
                ds.DrawImage(_dryStrokesTarget);

            ds.Transform = orgTransform;
            if (_needsWetStrokeUpdate && _currentStroke.Count > 2)
            {
                if (_builder == null)
                {
                    _builder = new InkStrokeBuilder();
                    _builder.SetDefaultDrawingAttributes(GetDrawingAttributes());
                }

                var s = _builder.CreateStrokeFromInkPoints(_currentStroke.ToArray(), Matrix3x2.Identity);
                _builder.SetDefaultDrawingAttributes(GetDrawingAttributes());
                if (_isEraser)
                    s.DrawingAttributes = new InkDrawingAttributes { Color = Colors.DarkRed };
                ds.DrawInk(new InkStroke[] { s });
            }
            
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
