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
using NusysIntermediate;
using NuSysApp.Network.Requests;

namespace NuSysApp
{
    public class InkRenderItem : BaseRenderItem
    {
        
        private ElementViewModel _vm;
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

        public InkRenderItem(CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator):base(parent, resourceCreator)
        {
            _canvas = (CanvasAnimatedControl) resourceCreator;
            parent.ViewModel.Controller.LibraryElementController.ContentDataController.InkAdded += ContentDataControllerOnInkAdded;
            parent.ViewModel.Controller.LibraryElementController.ContentDataController.InkRemoved += ContentDataControllerOnInkRemoved;
        }

        private void ContentDataControllerOnInkRemoved(string strokeId)
        {
            RemoveInkModel(strokeId);
        }

        private void ContentDataControllerOnInkAdded(InkModel inkModel)
        {
            AddInkModel(inkModel);
        }

        public async override Task Load()
        {           
            _inkManager = new InkManager();
            _dryStrokesTarget = new CanvasRenderTarget(ResourceCreator, _canvas.Size);
            base.CreateResources();
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            var parent = (CollectionRenderItem) Parent;
            parent.ViewModel.Controller.LibraryElementController.ContentDataController.InkAdded -= ContentDataControllerOnInkAdded;
            parent.ViewModel.Controller.LibraryElementController.ContentDataController.InkRemoved -= ContentDataControllerOnInkRemoved;

            _vm = null;
            _builder = null;
            _currentInkStroke = null;
            _currentInkPoints?.Clear();
            _currentInkPoints = null;
            _dryStrokesTarget?.Dispose();;
            _dryStrokesTarget = null;
            _inkManager = null;
            LatestStroke = null;
            _strokesToDraw?.Clear();
            _strokesToDraw = null;
            _lock = null;
            base.Dispose();
        }

        public void UpdateDryInkTransform()
        {
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                _transform = Transform.ScreenToLocalMatrix;
                _needsDryStrokesUpdate = true;
            });
        }

        public void StartInkByEvent(CanvasPointer e)
        {
            _isEraser = e.Properties.IsEraser || e.Properties.IsRightButtonPressed;

            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                _isDrawing = true;
                _transform = Transform.ScreenToLocalMatrix;

                _currentInkPoints = new List<InkPoint>();
                var np = Vector2.Transform(e.CurrentPoint, _transform);
                _currentInkPoints.Add(new InkPoint(new Point(np.X, np.Y), e.Pressure));
                _needsWetStrokeUpdate = true;
            });
        }

        public void UpdateInkByEvent(CanvasPointer e)
        {
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                var np = Vector2.Transform(e.CurrentPoint, _transform);
                _currentInkPoints.Add(new InkPoint(new Point(np.X, np.Y), e.Pressure));
                _needsWetStrokeUpdate = true;
            });
        }

        public void StopInkByEvent(CanvasPointer e)
        {
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                var np = Vector2.Transform(e.CurrentPoint, _transform);
                _currentInkPoints.Add(new InkPoint(new Point(np.X, np.Y), e.Pressure));

                var builder = new InkStrokeBuilder();
                builder.SetDefaultDrawingAttributes(GetDrawingAttributes(InkColor, InkSize));
                LatestStroke = builder.CreateStrokeFromInkPoints(_currentInkPoints.ToArray(), Matrix3x2.Identity);

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
                        SendInkStrokeRemovedRequest(strokeId);
                    }
                    _inkManager.DeleteSelected();
                    _inkManager.SelectWithPolyLine(thisStroke);
                    selected = GetSelectedStrokes();
                    _inkManager.DeleteSelected();

                }
                else
                {
                    _inkManager.AddStroke(LatestStroke);
                    LatestStrokeAdded = DateTime.Now;

                    SendInkStrokeAddedRequest();
                }

                _currentInkPoints = new List<InkPoint>();
                _strokesToDraw = _inkManager.GetStrokes().ToList();

                _needsDryStrokesUpdate = true;
                _needsWetStrokeUpdate = true;
            });
        }

        private async Task SendInkStrokeRemovedRequest(string strokeId)
        {
            StrokesMap.Remove(strokeId);
            var parentCollection = (CollectionRenderItem)Parent;

            var contentId = parentCollection.ViewModel.Controller.LibraryElementController.LibraryElementModel.ContentDataModelId;
            var request = new DeleteInkStrokeRequest(strokeId, contentId);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            parentCollection.ViewModel.Controller.LibraryElementController.ContentDataController.RemoveInk(strokeId);
        }

        private async Task SendInkStrokeAddedRequest()
        {
            var strokeId = SessionController.Instance.GenerateId();
            StrokesMap[strokeId] = LatestStroke;
            var parentCollection = (CollectionRenderItem)Parent;
            var args = new CreateInkStrokeRequestArgs();
            args.ContentId = parentCollection.ViewModel.Controller.LibraryElementController.LibraryElementModel.ContentDataModelId;
            args.InkPoints = LatestStroke.GetInkPoints().Select(p => new PointModel(p.Position.X, p.Position.Y, p.Pressure)).ToList();
            args.InkStrokeId = strokeId;
            args.Color = new ColorModel
            {
                A = InkColor.A,
                B = InkColor.B,
                G = InkColor.G,
                R = InkColor.R,
            };
            args.Thickness = InkSize;

            var request = new CreateInkStrokeRequest(args);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
        }

        public void AddInkModel(InkModel inkModel)
        {
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                var builder = new InkStrokeBuilder();
                var inkStroke =
                    builder.CreateStrokeFromInkPoints(
                        inkModel.InkPoints.Select(p => new InkPoint(new Point(p.X, p.Y), p.Pressure)),
                        Matrix3x2.Identity);
                inkStroke.DrawingAttributes = GetDrawingAttributes(Color.FromArgb((byte)inkModel.Color.A, (byte)inkModel.Color.R, (byte)inkModel.Color.G, (byte)inkModel.Color.B), (float)inkModel.Thickness);
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
                if (StrokesMap.ContainsKey(strokeId)) // TODO not have to check, this should only be getting called from the controller if it exists
                {
                    var inkStroke = StrokesMap[strokeId];
                    inkStroke.Selected = true;
                    _inkManager.DeleteSelected();
                    _strokesToDraw = _inkManager.GetStrokes().ToList();
                }
            });
        }

        public void RemoveLatestStroke()
        {
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                if (LatestStroke != null)
                {
                    LatestStroke.Selected = true;
                    _inkManager.DeleteSelected();
                    LatestStroke = null;
                }
                _strokesToDraw = _inkManager.GetStrokes().ToList();
                _needsDryStrokesUpdate = true;
            });
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;
            if (_needsDryStrokesUpdate)
            {
                if (_dryStrokesTarget != null) { 
                    using (var dss = _dryStrokesTarget.CreateDrawingSession())
                    {
                        dss.Clear(Colors.Transparent);
                        var dryStrokes = _strokesToDraw;
                        dss.Transform = ds.Transform;
                        dss.DrawInk(dryStrokes);

                        _needsDryStrokesUpdate = false;
                    }
                }
            }

            ds.Transform = Matrix3x2.Identity;
            if (_dryStrokesTarget != null)
                ds.DrawImage(_dryStrokesTarget);

            ds.Transform = orgTransform;
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
                    var s = _builder.CreateStrokeFromInkPoints(_currentInkPoints.ToArray(), Matrix3x2.Identity);
                    if (_isEraser)
                        s.DrawingAttributes = GetDrawingAttributes(Colors.DarkRed, InkSize);
                    ds.DrawInk(new InkStroke[] {s});
                }
            }
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
    }
}
