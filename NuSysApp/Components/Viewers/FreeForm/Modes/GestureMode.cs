
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class GestureMode : AbstractWorkspaceViewMode
    {
        private InqCanvasModel _inqCanvasModel;
        private long _tLineFinalized;
        private DateTime _tFirstPress;
        private static List<InqLineModel> _lines = new List<InqLineModel>();
        private bool _wasGesture;
        
        public GestureMode(FreeFormViewer view) : base(view) { }

        public override async Task Activate()
        {
            _view.InqCanvas.IsEnabled = true;
            var wvm = (FreeFormViewerViewModel)_view.DataContext;
            _inqCanvasModel = wvm.Model.InqCanvas;
            _inqCanvasModel.LineFinalizedLocally += OnLineFinalized;
        }

        private void OnLineFinalized(InqLineModel inqLine)
        {
            if (_wasGesture)
            {
                inqLine.Delete();
                _wasGesture = false;
            } else { 
                _lines.Add(inqLine);
                OnPointerPressed();
            }
        }

        private async void OnPointerPressed()
        {
            var s = DateTime.Now.Subtract(_tFirstPress).TotalSeconds;
            if (s < 0.25)
            {
                if (_lines.Count < 3)
                    return;
                   
                var model = _lines[_lines.Count - 3];
                
                var gestureType = GestureRecognizer.Classify(model);
                switch (gestureType)
                {
                    case GestureRecognizer.GestureType.None:
                        break;
                    case GestureRecognizer.GestureType.SELECTION:   
                        //TODO: make sure checkFor TaagCreation ignore gesture lines
                        var isTag = await CheckForTagCreation(model);
                        _wasGesture = isTag;
                        
                        if (!isTag)
                        {
                            CreateAreaNode(model);
                        }

                        _inqCanvasModel.RemoveLine(_lines[_lines.Count - 1]);
                        _inqCanvasModel.RemoveLine(_lines[_lines.Count - 2]);
                        _inqCanvasModel.RemoveLine(_lines[_lines.Count - 3]);
                        _lines.Remove(_lines[_lines.Count - 1]);
                        _lines.Remove(_lines[_lines.Count - 1]);
                        _lines.Remove(_lines[_lines.Count - 1]);

                        break;
                    case GestureRecognizer.GestureType.Scribble:
                        
                        _wasGesture = true;
                        var vm = (FreeFormViewerViewModel) _view.DataContext;
                        var deletedSome = vm.CheckForInkNodeIntersection(model);
                        _inqCanvasModel.RemoveLine(_lines[_lines.Count - 1]);
                        _inqCanvasModel.RemoveLine(_lines[_lines.Count - 2]);
                        _inqCanvasModel.RemoveLine(_lines[_lines.Count - 3]);
                        _lines.Remove(_lines[_lines.Count - 1]);
                        _lines.Remove(_lines[_lines.Count - 1]);
                        _lines.Remove(_lines[_lines.Count - 1]);
                        break;
                }
            }
                
            _tFirstPress = DateTime.Now;
        }

        private async void CreateAreaNode(InqLineModel line)
        {
            line.Points.Add(line.Points.First());
            var bb = Geometry.InqToBoudingRect(line);
            var transPoints = line.Points.Select(p => new Point2d(p.X * Constants.MaxCanvasSize - bb.X, p.Y * Constants.MaxCanvasSize - bb.Y ));
          
            var m = new Message();
            m["x"] = bb.X;
            m["y"] = bb.Y;
            m["width"] = 400;
            m["height"] = 400;
            m["nodeType"] = ElementType.Area.ToString();
            m["points"] = transPoints;
            m["autoCreate"] = true;
            m["creators"] = new List<string>() { SessionController.Instance.ActiveFreeFormViewer.Id };

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewNodeRequest(m));

        }

        public override async Task Deactivate()
        {
            _view.InqCanvas.IsEnabled = false;
            _inqCanvasModel.LineFinalizedLocally -= OnLineFinalized;

        }

        private async Task<bool> CheckForTagCreation(InqLineModel line)
        {
            var wsmodel = (_view.DataContext as FreeFormViewerViewModel).Model as WorkspaceModel;
            var Model = wsmodel.InqCanvas;
            var outerRect = Geometry.PointCollecionToBoundingRect(line.Points.ToList());
            outerRect.X *= Constants.MaxCanvasSize;
            outerRect.Y *= Constants.MaxCanvasSize;
            outerRect.Width *= Constants.MaxCanvasSize;
            outerRect.Height *= Constants.MaxCanvasSize;


            if (outerRect.Width * outerRect.Height < 150.0 * 150.0)
                return false;



            var idsToDelete = new List<InqLineModel>();
            var encompassedLines = new List<InqLineModel>();

            var lastLine = _lines.Last();
            var secondLastLine = _lines[_lines.Count - 2];
            foreach (var otherLine in _lines.Where(l => l.Id != line.Id && l.Id != lastLine.Id && l.Id != secondLastLine.Id))
            {
                var innerRect = Geometry.PointCollecionToBoundingRect(otherLine.Points.ToList());
                innerRect.X *= Constants.MaxCanvasSize;
                innerRect.Y *= Constants.MaxCanvasSize;
                innerRect.Width *= Constants.MaxCanvasSize;
                innerRect.Height *= Constants.MaxCanvasSize;
                var innerRect2 = new Rect(innerRect.X, innerRect.Y, innerRect.Width, innerRect.Height);
                innerRect.Intersect(outerRect);
                if (Math.Abs(innerRect2.Width - innerRect.Width) < 20 && Math.Abs(innerRect2.Height - innerRect.Height) < 20)
                {

                    idsToDelete.Add(otherLine);
                    InqLineModel newModel = new InqLineModel(SessionController.Instance.GenerateId())
                    {
                        Stroke = otherLine.Stroke,
                        StrokeThickness = otherLine.StrokeThickness
                    };

                    foreach (var point in otherLine.Points)
                    {
                        newModel.AddPoint(new Point2d(point.X * Constants.MaxCanvasSize - outerRect.X, point.Y * Constants.MaxCanvasSize - outerRect.Y));
                    }
                    encompassedLines.Add(newModel);
                }
            }

            var first = line.Points.First();
            var last = line.Points.Last();
            if (encompassedLines.Count == 0 || (Math.Abs(first.X - last.X) > 40 || Math.Abs(first.Y - last.Y) > 40))
            {
                return false;
            }


            foreach (var idToDelete in idsToDelete)
            {
                _inqCanvasModel.RemoveLine(idToDelete);
            }

            var titles = await InkToText(encompassedLines);
            var tagNodePos = new Point(outerRect.X + outerRect.Width / 6, outerRect.Y + outerRect.Height / 6);

            var m = new Message();
            m["x"] = tagNodePos.X;
            m["y"] = tagNodePos.Y;
            m["width"] = 400;
            m["title"] = titles.First();
            m["height"] = 400;
            m["nodeType"] = ElementType.Tag.ToString();
            m["titleSuggestions"] = titles;
            m["autoCreate"] = true;
            m["creators"] = new List<string>() { SessionController.Instance.ActiveFreeFormViewer.Id };

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewNodeRequest(m));

            return true;
        }

       

        public async Task<List<string>> InkToText(List<InqLineModel> inqLineModels)
        {
            if (inqLineModels.Count == 0)
                return new List<string>();

            var im = new InkManager();
            var b = new InkStrokeBuilder();

            foreach (var inqLineModel in inqLineModels)
            {
                var pc = new PointCollection();
                foreach (var point2D in inqLineModel.Points)
                {
                    pc.Add(new Point(point2D.X, point2D.Y));
                }

                var stroke = b.CreateStroke(pc);
                im.AddStroke(stroke);
            }
            
            var result = await im.RecognizeAsync(InkRecognitionTarget.All);
            return result[0].GetTextCandidates().ToList();
            
        }
    }
}
