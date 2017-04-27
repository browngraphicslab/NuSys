using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NusysIntermediate;
using NuSysApp.Network.Requests;

// TODO remove this;; probably gets too slow so that it doesn't respond on time (so the InkPoint does not get added fast enough, creating sharper edges instead of the round 

namespace NuSysApp
{
    public class InkRenderItem : BaseRenderItem
    {
        public static bool StopCanvasInk;
        private ElementController _parentCollectionController;
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

        // ImproveInk Changes 
        private List<InkStroke> _wetStrokesToDraw = new List<InkStroke>(); 

        public Color InkColor { get; set; } = Colors.Black;
        public float InkSize = 4;
        public BiDictionary<string, InkStroke> StrokesMap = new BiDictionary<string, InkStroke>();

        public InkRenderItem(CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator):base(parent, resourceCreator)
        {
            _canvas = (CanvasAnimatedControl) resourceCreator;
            _parentCollectionController = parent.ViewModel.Controller;
            parent.ViewModel.Controller.LibraryElementController.ContentDataController.InkAdded += ContentDataControllerOnInkAdded;
            parent.ViewModel.Controller.LibraryElementController.ContentDataController.InkRemoved += ContentDataControllerOnInkRemoved;
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
            await _canvas.RunOnGameLoopThreadAsync(delegate
            {
                _inkManager = new InkManager();
                _dryStrokesTarget = new CanvasRenderTarget(ResourceCreator, _canvas.Size);
                base.CreateResources();
            });
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            var parent = (CollectionRenderItem) Parent;
            parent.ViewModel.Controller.LibraryElementController.ContentDataController.InkAdded -= ContentDataControllerOnInkAdded;
            parent.ViewModel.Controller.LibraryElementController.ContentDataController.InkRemoved -= ContentDataControllerOnInkRemoved;

            _parentCollectionController = null;
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
            _transform = Transform.ScreenToLocalMatrix;
            _needsDryStrokesUpdate = true;
        }

        public void StartInkByEvent(CanvasPointer e)
        {
            if (StopCanvasInk)
            {
                return;
            }
            _isEraser = e.IsEraser || e.IsRightButtonPressed;

            _isDrawing = true;
            _transform = Transform.ScreenToLocalMatrix;

            _currentInkPoints = new List<InkPoint>();
            var np = Vector2.Transform(e.CurrentPoint, _transform);
            _currentInkPoints.Add(new InkPoint(new Point(np.X, np.Y), e.Pressure));
            _needsWetStrokeUpdate = true;
        }

        public void UpdateInkByEvent(CanvasPointer e)
        {
            if (StopCanvasInk)
            {
                return;
            }
            var np = Vector2.Transform(e.CurrentPoint, _transform);
            _currentInkPoints.Add(new InkPoint(new Point(np.X, np.Y), e.Pressure));
            _needsWetStrokeUpdate = true;
        }

        public InkStroke CurrentInkStrokeWithEndpoint(CanvasPointer e)
        {
            var np = Vector2.Transform(e.CurrentPoint, _transform);
            _currentInkPoints.Add(new InkPoint(new Point(np.X, np.Y), e.Pressure));
            var builder = new InkStrokeBuilder();
            builder.SetDefaultDrawingAttributes(GetDrawingAttributes(InkColor, InkSize));
            return builder.CreateStrokeFromInkPoints(_currentInkPoints.ToArray(), Matrix3x2.Identity);
        }

        public InkStroke CurrentInkStroke()
        {
            var builder = new InkStrokeBuilder();
            builder.SetDefaultDrawingAttributes(GetDrawingAttributes(InkColor, InkSize));
            return builder.CreateStrokeFromInkPoints(_currentInkPoints.ToArray(), Matrix3x2.Identity);
        }

        public void StopInkByEvent(CanvasPointer e)
        {
            if (StopCanvasInk)
            {
                return;
            }
            LatestStroke = CurrentInkStrokeWithEndpoint(e);

            if (_isEraser)                                                                      // KBTODO eraser may erase the strokes in "chunks" since i'm dividing up the lines lol ... 
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
                    _parentCollectionController.LibraryElementController.ContentDataController.RemoveInk(strokeId);
                }
                _inkManager.DeleteSelected();

            }
            else
            {     
                LatestStrokeAdded = DateTime.Now;
                var contentDataModelId = _parentCollectionController.LibraryElementController.LibraryElementModel.ContentDataModelId;

                /* 
                var model = LatestStroke.ToInkModel(contentDataModelId, InkColor, InkSize);

                StrokesMap[model.InkStrokeId] = LatestStroke;
                _parentCollectionController.LibraryElementController.ContentDataController.AddInk(model);
                */

                // IMPROVEINK CHANGES                                                   /////////////////////////////////////////////////////////////////////////////////////////////
                _wetStrokesToDraw.Add(LatestStroke);                 
                foreach (var wetStroke in _wetStrokesToDraw)
                {
                    var model = wetStroke.ToInkModel(contentDataModelId, InkColor, InkSize);

                    StrokesMap[model.InkStrokeId] = wetStroke;
                    _parentCollectionController.LibraryElementController.ContentDataController.AddInk(model);
                }

            }

            _currentInkPoints = new List<InkPoint>();
            _strokesToDraw = _inkManager.GetStrokes().ToList();


            // IMPROVEINK CHANGES                                                        /////////////////////////////////////////////////////////////////////////////////////////////
            /* 
            _inkManager.AddStroke(LatestStroke);  
            foreach (var wetStroke in _wetStrokesToDraw)
            {
                _inkManager.AddStroke(wetStroke);
            }

            _strokesToDraw = _inkManager.GetStrokes().ToList();
            */
            
            _wetStrokesToDraw.Clear();

            _needsDryStrokesUpdate = true;
            _needsWetStrokeUpdate = true;
        }

        public void RemoveCurrentStroke()
        {
            _currentInkPoints = new List<InkPoint>();
            _needsWetStrokeUpdate = true;
        }

        public void AddInkModel(InkModel inkModel)
        {
            _canvas.RunOnGameLoopThreadAsync(async () =>
            {
                var builder = new InkStrokeBuilder();
                var inkStroke =
                    builder.CreateStrokeFromInkPoints(
                        inkModel.InkPoints.Select(p => new InkPoint(new Point(p.X, p.Y), p.Pressure)),
                        Matrix3x2.Identity);
                inkStroke.DrawingAttributes = GetDrawingAttributes(Color.FromArgb((byte)inkModel.Color.A, (byte)inkModel.Color.R, (byte)inkModel.Color.G, (byte)inkModel.Color.B), (float)inkModel.Thickness);
                _inkManager.AddStroke(inkStroke);
                /*
                try
                {
                    var recog = await _inkManager.RecognizeAsync(InkRecognitionTarget.Recent);
                    foreach (InkRecognitionResult ink in recog)
                    {
                        var s = ink.GetTextCandidates();
                        Debug.WriteLine(s.First());
                    }
                }
                catch (Exception e)
                {
                    
                }*/
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
                    StrokesMap.Remove(strokeId);
                    inkStroke.Selected = true;
                    _inkManager.DeleteSelected();
                    _strokesToDraw = _inkManager.GetStrokes().ToList();
                    _needsDryStrokesUpdate = true;
                }
            });
        }

        public void RemoveLatestStroke()
        {
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                if (LatestStroke != null && StrokesMap.ContainsValue(LatestStroke))
                {
                    var strokeId = StrokesMap.GetKeyByValue(LatestStroke);
                    StrokesMap[strokeId].Selected = true;
                    StrokesMap.Remove(strokeId);
                    _parentCollectionController.LibraryElementController.ContentDataController.RemoveInk(strokeId);
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
                if (_dryStrokesTarget != null)
                { 
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

                
                _builder.SetDefaultDrawingAttributes(GetDrawingAttributes(InkColor, InkSize));
                var s = _builder.CreateStrokeFromInkPoints(_currentInkPoints.ToArray(), Matrix3x2.Identity);

                if (_isEraser)
                    s.DrawingAttributes = GetDrawingAttributes(Colors.DarkRed, InkSize);

                // IMPROVEINK CHANGE ////////////////                                  ///////////////////////////////////////////////////////////////////////////////////////////// 
                const int threshold = 800; //                                
                //Debug.WriteLine(_currentInkPoints.Count); 
                if (_currentInkPoints.Count >= threshold)
                {
                    _wetStrokesToDraw.Add(s);
                    InkPoint lastPoint = _currentInkPoints.Last(); 
                    _currentInkPoints.Clear();                                                                      // KBTODO check if there is anything else to reset 
                    _currentInkPoints.Add(lastPoint);
                    //ds.DrawInk(_wetStrokesToDraw);
                    // return; 
                    Debug.WriteLine("new");                                                                             // KBTODO erase later 
                }
                var toDraw = new List<InkStroke>( _wetStrokesToDraw );
                toDraw.Add(s);
                ds.DrawInk(toDraw);

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
