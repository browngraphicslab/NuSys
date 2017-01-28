using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// This class creates an inkable element with a given content data model to support saving and updating of ink across inkable elements.
    /// InkableUIElement listens for changes to the input type, and ignores hit tests when the input type is not pen.
    /// </summary>
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
        public bool ClampInkToBounds = true;
        private Rect _imageRect;

        public InkableUIElement(IInkController inkController, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _canvas = (CanvasAnimatedControl)resourceCreator;
            _inkController = inkController;
            PenPointerPressed += OnPenPointerPressed_Callback;
            PenPointerDragged += OnPenPointerDragged_Callback;
            PenPointerReleased += OnPenPointerReleased_Callback;
            _inkController.InkAdded += ContentDataControllerOnInkAdded;
            _inkController.InkRemoved += ContentDataControllerOnInkRemoved;
            SessionController.Instance.SessionView.FreeFormViewer.CanvasInteractionManager.InteractionTypeChanged += CanvasInteractionManagerOnInteractionTypeChanged;
            IsHitTestVisible = false;
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                _inkManager = new InkManager();
                foreach (InkModel model in _inkController.Strokes)
                {
                    AddInkModel(model);
                }
                _dryStrokesTarget = new CanvasRenderTarget(ResourceCreator, new Size(1000.0, 1000.0));
            });
        }

        public InkableUIElement(ImageLibraryElementController controller, BaseRenderItem parent,
            ICanvasResourceCreatorWithDpi resourceCreator) : this(controller.ContentDataController, parent, resourceCreator)
        {
            ImageLibraryElementModel model = controller.ImageLibraryElementModel;
             _imageRect = new Rect(model.NormalizedX, model.NormalizedY, model.NormalizedWidth, model.NormalizedHeight);
        }

        private void CanvasInteractionManagerOnInteractionTypeChanged(object sender, CanvasInteractionManager.InteractionType interactionType)
        {
            if (interactionType == CanvasInteractionManager.InteractionType.Pen)
            {
                IsHitTestVisible = true;
            } else if (interactionType == CanvasInteractionManager.InteractionType.Touch)
            {
                IsHitTestVisible = false;
            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            _inkController.InkAdded -= ContentDataControllerOnInkAdded;
            _inkController.InkRemoved -= ContentDataControllerOnInkRemoved;
            SessionController.Instance.SessionView.FreeFormViewer.CanvasInteractionManager.InteractionTypeChanged -= CanvasInteractionManagerOnInteractionTypeChanged;

            _builder = null;
            _currentInkStroke = null;
            _currentInkPoints?.Clear();
            _currentInkPoints = null;
            _dryStrokesTarget?.Dispose(); ;
            _dryStrokesTarget = null;
            _inkManager = null;
            LatestStroke = null;
            _strokesToDraw?.Clear();
            _strokesToDraw = null;
            _lock = null;
            base.Dispose();
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
            _currentInkPoints.Add(PointerToInkPoint(np, e.Pressure));
            _needsWetStrokeUpdate = true;
        }

        public void UpdateInkByEvent(CanvasPointer e)
        {
            var np = Vector2.Transform(e.CurrentPoint, _transform);
            _currentInkPoints.Add(PointerToInkPoint(np, e.Pressure));
            _needsWetStrokeUpdate = true;
        }

        public InkStroke CurrentInkStrokeWithEndpoint(CanvasPointer e)
        {
            InkRenderItem.StopCanvasInk = false;
            var np = Vector2.Transform(e.CurrentPoint, _transform);
            _currentInkPoints.Add(PointerToInkPoint(np, e.Pressure));
            var builder = new InkStrokeBuilder();
            builder.SetDefaultDrawingAttributes(GetDrawingAttributes(InkColor, InkSize));
            return builder.CreateStrokeFromInkPoints(_currentInkPoints.ToArray(), Matrix3x2.Identity);
        }

        private InkPoint PointerToInkPoint(Vector2 np, float pressure)
        {
            if (ClampInkToBounds)
            {
                np = Vector2.Clamp(np, new Vector2(0, 0), new Vector2(Width, Height));
            }
            return (new InkPoint(new Point(1000 * (_imageRect.Width * np.X + _imageRect.X * Width) / Width, 1000 * (_imageRect.Height * np.Y + _imageRect.Y * Height) / Height), pressure));
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
                    _inkManager.SelectWithPolyLine(thisStroke);
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

            await UITask.Run(async delegate
            {
                // display ink options
                double d = e.DistanceTraveled;
                DateTime n  = DateTime.Now;
                DateTime s = e.StartTime;
                double t = (n - s).TotalMilliseconds;
                if (e.DistanceTraveled < 20 && (DateTime.Now - e.StartTime).TotalMilliseconds > 500)
                {
                    var screenBounds = CoreApplication.MainView.CoreWindow.Bounds;
                    var optionsBounds =
                        SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.InkOptions.GetLocalBounds();
                    var targetPoint = e.CurrentPoint;
                    if (targetPoint.X < screenBounds.Width/2)
                    {
                        targetPoint.X += 20;
                    }
                    else
                    {
                        targetPoint.X -= (20 + (float) optionsBounds.Width);
                    }
                    targetPoint.Y -= (float) optionsBounds.Height/2;
                    targetPoint.X =
                        (float) Math.Min(screenBounds.Width - optionsBounds.Width, Math.Max(0, targetPoint.X));
                    targetPoint.Y =
                        (float) Math.Min(screenBounds.Height - optionsBounds.Height, Math.Max(0, targetPoint.Y));
                    SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.InkOptions.Transform
                        .LocalPosition = targetPoint;
                    SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.InkOptions.IsVisible = true;
                }
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
                        Color.FromArgb((byte)inkModel.Color.A, (byte)inkModel.Color.R, (byte)inkModel.Color.G,
                            (byte)inkModel.Color.B), (float)inkModel.Thickness);
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

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            InkColor =
                SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.InkRenderItem.InkColor;
            InkSize =
                SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.InkRenderItem.InkSize;
            base.Update(parentLocalToScreenTransform);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;

            if (_dryStrokesTarget != null)
            {
                try
                {
                    using (var dss = _dryStrokesTarget.CreateDrawingSession())
                    {
                        dss.Clear(Colors.Transparent);
                        var dryStrokes = _strokesToDraw;
                        dss.Transform = Matrix3x2.CreateTranslation((float) (-_imageRect.X*1000),
                            (float) (-_imageRect.Y*1000));
                        dss.DrawInk(dryStrokes);

                        _needsDryStrokesUpdate = false;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message, "Failed in inkable ui element draw call");
                }
            }
            
            ds.Transform = Matrix3x2.CreateScale((float)(Width / (1000 * _imageRect.Width)), (float)(Height / (1000 * _imageRect.Height))) * Transform.LocalToScreenMatrix;
            using (ds.CreateLayer(1.0f, new Rect(0, 0, 1000 * _imageRect.Width, 1000 * _imageRect.Height)))
            {
                if (_dryStrokesTarget != null)
                {
                    ds.DrawImage(_dryStrokesTarget);
                }

                ds.Transform = Matrix3x2.CreateTranslation((float)(-_imageRect.X * 1000), (float)(-_imageRect.Y * 1000)) *
                               Matrix3x2.CreateScale((float)(Width / (1000 * _imageRect.Width)),
                                   (float)(Height / (1000 * _imageRect.Height))) * Transform.LocalToScreenMatrix;
                if (_currentInkPoints != null && _currentInkPoints.Count > 2)
                {
                    if (_builder == null)
                    {
                        _builder = new InkStrokeBuilder();
                        _builder.SetDefaultDrawingAttributes(GetDrawingAttributes(InkColor, InkSize));

                    }
                    lock (_lock)
                    {
                        _builder.SetDefaultDrawingAttributes(GetDrawingAttributes(InkColor, InkSize));
                        var s = _builder.CreateStrokeFromInkPoints(_currentInkPoints.ToArray(), Matrix3x2.Identity);
                        if (_isEraser)
                        {
                            s.DrawingAttributes = GetDrawingAttributes(Colors.DarkRed, InkSize);
                        }
                        ds.DrawInk(new InkStroke[] { s });
                    }
                }
            }

            ds.Transform = orgTransform;
            base.Draw(ds);
        }

        /// <summary>
        /// Creates drawing attributes for the pen.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the currently selected strokes from the InkManager.
        /// </summary>
        /// <returns></returns>
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
