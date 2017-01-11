using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NusysIntermediate;

namespace NuSysApp
{
    class InkableUIElement : RectangleUIElement
    {
        private IInkController _inkController;

        private bool _isEraser;
        private List<InkPoint> _currentInkPoints = new List<InkPoint>();
        private InkStroke _currentInkStroke;
        private InkManager _inkManager;
        private Matrix3x2 _transform = Matrix3x2.Identity;
        private InkStrokeBuilder _builder;
        private bool _isDrawing;
        public InkStroke LatestStroke { get; set; }
        public DateTime LatestStrokeAdded { get; set; }
        private List<InkStroke> _strokesToDraw = new List<InkStroke>();
        private CanvasRenderTarget _dryStrokesTarget;
        private bool _needsDryStrokesUpdate;
        private bool _needsWetStrokeUpdate;
        private object _lock = new object();
        private CanvasAnimatedControl _canvas;
        public Color InkColor { get; set; } = Colors.Black;
        public float InkSize = 4;
        public BiDictionary<string, InkStroke> StrokesMap = new BiDictionary<string, InkStroke>();

        public InkableUIElement(IInkController inkController, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _canvas = (CanvasAnimatedControl)resourceCreator;
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                _inkController = inkController;
                PenPointerPressed += OnPenPointerPressed_Callback;
                PenPointerDragged += OnPenPointerDragged_Callback;
                PenPointerReleased += OnPenPointerReleased_Callback;
                _inkController.InkAdded += ContentDataControllerOnInkAdded;
                _inkController.InkRemoved += ContentDataControllerOnInkRemoved;
                _inkManager = new InkManager();
            });
        }

        private void ContentDataControllerOnInkRemoved(object sender, string strokeId)
        {
            RemoveInkModel(strokeId);
        }

        private void ContentDataControllerOnInkAdded(object sender, InkModel inkModel)
        {
            AddInkModel(inkModel);
        }

        public async override Task Load()
        {
            _dryStrokesTarget = new CanvasRenderTarget(ResourceCreator, _canvas.Size);
            base.CreateResources();
        }

        public void StartInkByEvent(CanvasPointer e)
        {
            InkRenderItem.StopCanvasInk = true;
            _isEraser = e.IsEraser || e.IsRightButtonPressed;

            _isDrawing = true;
            _transform = Transform.ScreenToLocalMatrix;

            _currentInkPoints = new List<InkPoint>();
            var np = Vector2.Transform(e.CurrentPoint, _transform);
            _currentInkPoints.Add(new InkPoint(new Point(np.X / Width, np.Y / Height), e.Pressure));
            _needsWetStrokeUpdate = true;
        }

        public void UpdateInkByEvent(CanvasPointer e)
        {
            var np = Vector2.Transform(e.CurrentPoint, _transform);
            _currentInkPoints.Add(new InkPoint(new Point(np.X / Width, np.Y / Height), e.Pressure));
            _needsWetStrokeUpdate = true;
        }

        public InkStroke CurrentInkStrokeWithEndpoint(CanvasPointer e)
        {
            InkRenderItem.StopCanvasInk = false;
            var np = Vector2.Transform(e.CurrentPoint, _transform);
            _currentInkPoints.Add(new InkPoint(new Point(np.X / Width, np.Y / Height), e.Pressure));
            var builder = new InkStrokeBuilder();
            builder.SetDefaultDrawingAttributes(GetDrawingAttributes(InkColor, InkSize));
            return builder.CreateStrokeFromInkPoints(_currentInkPoints.ToArray(), Matrix3x2.Identity);
        }

        public async Task StopInkByEvent(CanvasPointer e)
        {
            LatestStroke = CurrentInkStrokeWithEndpoint(e);
            await _canvas.RunOnGameLoopThreadAsync(() =>
            {
                if (_isEraser)
                {
                    var allStrokes = _inkManager.GetStrokes().ToArray();
                    var thisStroke = _currentInkPoints.Select(p => p.Position);
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


                    foreach (var s in selected)
                    {
                        var strokeId = StrokesMap.GetKeyByValue(s);
                        StrokesMap.Remove(strokeId);
                        _inkController.RemoveInk(strokeId);
                    }
                    _inkManager.DeleteSelected();
                    _inkManager.SelectWithPolyLine(thisStroke);
                    selected = GetSelectedStrokes();
                    _inkManager.DeleteSelected();

                }
                else
                {
                    LatestStrokeAdded = DateTime.Now;

                    var contentController = (ContentDataController)(_inkController);
                    var contentDataModelId = contentController.ContentDataModel.ContentId;
                    var model = LatestStroke.ToInkModel(contentDataModelId, InkColor, InkSize);

                    StrokesMap[model.InkStrokeId] = LatestStroke;

                    _inkController.AddInk(model);
                }

                _currentInkPoints = new List<InkPoint>();

                _strokesToDraw = _inkManager.GetStrokes().ToList();

                _needsDryStrokesUpdate = true;
                _needsWetStrokeUpdate = true;
            });
        }

        public async Task AddInkModel(InkModel inkModel)
        {
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                if (_inkManager == null)
                {
                    return;
                }

                var builder = new InkStrokeBuilder();
                var inkStroke =
                    builder.CreateStrokeFromInkPoints(
                        inkModel.InkPoints.Select(p => new InkPoint(new Point(p.X, p.Y), p.Pressure)),
                        Matrix3x2.Identity);
                inkStroke.DrawingAttributes =
                    GetDrawingAttributes(
                        Color.FromArgb((byte) inkModel.Color.A, (byte) inkModel.Color.R, (byte) inkModel.Color.G,
                            (byte) inkModel.Color.B), (float) inkModel.Thickness);
                _inkManager.AddStroke(inkStroke);
                _strokesToDraw = _inkManager.GetStrokes().ToList();
                _needsDryStrokesUpdate = true;
                StrokesMap[inkModel.InkStrokeId] = inkStroke;
            });
        }

        public void RemoveInkModel(string strokeId)
        {
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                if (StrokesMap.ContainsKey(strokeId))
                {
                    var inkStroke = StrokesMap[strokeId];
                    StrokesMap.Remove(strokeId);
                    inkStroke.Selected = true;
                    _inkManager.DeleteSelected();
                    _strokesToDraw = _inkManager.GetStrokes().ToList();
                }
            });
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
            {
                return;
            }

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            var strokes = new List<InkStroke>();
            foreach (InkStroke stroke in _strokesToDraw)
            {
                var inkPoints = new List<InkPoint>();
                foreach (InkPoint inkPoint in stroke.GetInkPoints())
                {
                    InkPoint p = new InkPoint(new Point(inkPoint.Position.X * Width, inkPoint.Position.Y * Height), inkPoint.Pressure);
                    inkPoints.Add(p);
                }

                _builder = new InkStrokeBuilder();
                _builder.SetDefaultDrawingAttributes(GetDrawingAttributes(InkColor, InkSize));
                var s = _builder.CreateStrokeFromInkPoints(inkPoints.ToArray(), Matrix3x2.Identity);
                strokes.Add(s);
            }

            ds.DrawInk(strokes);

            if (_needsWetStrokeUpdate && _currentInkPoints.Count > 2)
            {
                if (_builder == null)
                {
                    _builder = new InkStrokeBuilder();
                    _builder.SetDefaultDrawingAttributes(GetDrawingAttributes(InkColor, InkSize));

                }
                lock (_lock)
                {
                    _builder.SetDefaultDrawingAttributes(GetDrawingAttributes(InkColor, InkSize));
                    var inkPoints = new List<InkPoint>();
                    foreach (InkPoint inkPoint in _currentInkPoints)
                    {
                        InkPoint p = new InkPoint(new Point(inkPoint.Position.X * Width, inkPoint.Position.Y * Height), inkPoint.Pressure);
                        inkPoints.Add(p);
                    }
                    var s = _builder.CreateStrokeFromInkPoints(inkPoints.ToArray(), Matrix3x2.Identity);
                    if (_isEraser)
                        s.DrawingAttributes = GetDrawingAttributes(Colors.DarkRed, InkSize);
                    ds.DrawInk(new InkStroke[] { s });
                }
            }

            ds.Transform = orgTransform;

            base.Draw(ds);
        }

        private InkDrawingAttributes GetDrawingAttributes(Color color, float thickness)
        {
            var _drawingAttributes = new InkDrawingAttributes
            {
                PenTip = PenTipShape.Circle,
                PenTipTransform = Matrix3x2.CreateRotation((float)Math.PI / 4),
                IgnorePressure = false,
                Size = new Size(thickness, thickness),
                Color = color
            };

            return _drawingAttributes;
        }

        private IEnumerable<InkStroke> GetSelectedStrokes()
        {
            return _inkManager.GetStrokes().ToArray().Where(stroke => stroke.Selected == true);
        }

        private void OnPenPointerPressed_Callback(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            StartInkByEvent(pointer);
        }

        private void OnPenPointerDragged_Callback(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            UpdateInkByEvent(pointer);
        }

        private async void OnPenPointerReleased_Callback(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            await StopInkByEvent(pointer);
        }
    }
}
