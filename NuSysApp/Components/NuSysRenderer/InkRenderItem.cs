﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private ConcurrentBag<InkStroke> _inkStrokes = new ConcurrentBag<InkStroke>();
        private bool _isEraser;
        private Color _drawingColor = Colors.Black;
        private List<InkPoint> _currentStroke = new List<InkPoint>();
        private InkManager _inkManager = new InkManager();
        private Matrix3x2 _transform = Matrix3x2.Identity;
        private InkStroke _activeInkStroke;
        private InkStrokeBuilder _builder;


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

        public void StartInkByEvent(PointerRoutedEventArgs e)
        {
            _transform = Win2dUtil.Invert(NuSysRenderer.Instance.GetTransformUntil(this));
            lock (this)
            {
                _currentStroke.Clear();

                _isEraser = e.GetCurrentPoint(null).Properties.IsEraser;
                if (_isEraser)
                    _drawingColor = Colors.DarkRed;
                else
                    _drawingColor = Colors.Black;

                _currentStroke = new List<InkPoint>();

                foreach (var p in e.GetIntermediatePoints(null).Reverse())
                {
                    var np = Vector2.Transform(new Vector2((float) p.RawPosition.X, (float) p.RawPosition.Y), _transform);
                    _currentStroke.Add(new InkPoint(new Point(np.X, np.Y), p.Properties.Pressure));
                }
            }
        }

        public void UpdateInkByEvent(PointerRoutedEventArgs e)
        {
            lock (this)
            {
                foreach (var p in e.GetIntermediatePoints(null).Reverse())
                {
                    var np = Vector2.Transform(new Vector2((float) p.RawPosition.X, (float) p.RawPosition.Y), _transform);
                    _currentStroke.Add(new InkPoint(new Point(np.X, np.Y), p.Properties.Pressure));
                }
            }
        }

        public void StopInkByEvent(PointerRoutedEventArgs e)
        {
            lock (this)
            {
                _drawingColor = Colors.Black;

                foreach (var p in e.GetIntermediatePoints(null).Reverse())
                {
                    var np = Vector2.Transform(new Vector2((float) p.RawPosition.X, (float) p.RawPosition.Y), _transform);
                    _currentStroke.Add(new InkPoint(new Point(np.X, np.Y), p.Properties.Pressure));
                }

                var builder = new InkStrokeBuilder();
                var stroke = builder.CreateStrokeFromInkPoints(_currentStroke.ToArray(), Matrix3x2.Identity);
                stroke.DrawingAttributes = GetDrawingAttributes();


                if (_isEraser)
                {
                    var allStrokes = _inkManager.GetStrokes().ToArray();
                    var thisStroke = _activeInkStroke.GetInkPoints().Select(p => p.Position);
                    var thisLineString = thisStroke.GetLineString();
                    ;

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
                    _inkManager.AddStroke(stroke);
                    //  InkStrokeAdded?.Invoke(this, stroke);
                }
                //_dryStrokes = _inkManager.GetStrokes().ToList();
                _inkStrokes.Add(stroke);
                _currentStroke.Clear();
            }
        }

        public void AddStroke(InkStroke stroke)
        {
            _inkStrokes.Add(stroke);
        }

        public override void Update()
        {
            if (!IsDirty)
                return;

            var aa = GetDrawingAttributes();
            foreach (var s in _inkStrokes)
            {
                var attr = aa;
                attr.Color = Colors.Black;
                s.DrawingAttributes = attr;
            }
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (_builder == null)
            {
                _builder = new InkStrokeBuilder();
            }

            // 
            lock (this)
            {
                ds.DrawInk(_inkStrokes);
            if (_currentStroke.Count > 2) { 
                
                    var stroke = _builder.CreateStrokeFromInkPoints(_currentStroke.ToArray(), Matrix3x2.Identity);
                    stroke.DrawingAttributes = GetDrawingAttributes();
                    ds.DrawInk(new InkStroke[1]{ stroke });
                }
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
