using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;
using MyToolkit.UI;

namespace NuSysApp
{
    public class GestureMode : AbstractWorkspaceViewMode
    {
        private InqCanvasModel _inqCanvasModel;
        private long _tLineFinalized;
        private DateTime _tFirstPress;
        private InqLineModel _inqLine;
        private bool _wasGesture;
        private bool _released;
       

        public GestureMode(FreeFormViewer view) : base(view)
        {
            var wvm = (FreeFormViewerViewModel)_view.DataContext;
            _inqCanvasModel = wvm.Model.InqCanvas;
            
            _view.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnPointerPressed), true);
            _view.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnPointerReleased), true);
            _tFirstPress = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));
        }

        public override async Task Activate()
        {
            _inqCanvasModel.LineFinalizedLocally += OnLineFinalized;
        }

        private void OnPointerReleased(object source, PointerRoutedEventArgs args)
        {
            _released = true;
        }

        private async void  OnPointerPressed(object source, PointerRoutedEventArgs args)
        {
            _released = false;
            if (SessionController.Instance.SessionView.IsPenMode)
                return;

            var s = DateTime.Now.Subtract(_tFirstPress).TotalSeconds;
            if (s > 1)
            {
                var f = (FrameworkElement)args.OriginalSource;
                var pc = f.FindParentDataContext();
                await Task.Delay(200);
                if (_released && SessionController.Instance.ActiveFreeFormViewer.Selections.Count < 2 || (pc is FreeFormViewerViewModel))
                    _view.MultiMenu.Visibility = Visibility.Collapsed;
                return;
            }
            
            SelectionByStroke();
            args.Handled = true;
        }

        private void SelectionByStroke()
        {
            var screenPoints = new Polyline();
            var t = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            foreach (var point in _inqLine.Points)
            {
                var np = t.TransformPoint(new Point(point.X * Constants.MaxCanvasSize, point.Y * Constants.MaxCanvasSize));
                screenPoints.Points.Add(np);
            }

            var hull = new SelectionHull();
            var numSelections = hull.Compute(screenPoints, SessionController.Instance.SessionView.MainCanvas);
            if (numSelections > 0) { 
                _inqLine.Delete();
                _view.MultiMenu.Visibility = Visibility.Visible;
                Canvas.SetLeft(_view.MultiMenu, screenPoints.Points[0].X);
                Canvas.SetTop(_view.MultiMenu, screenPoints.Points[0].Y);
            }
        }

        private void OnLineFinalized(InqLineModel inqLine)
        {
            _inqLine = inqLine;
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
            m["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id ;
            m["creatorContentID"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));

        }


        public override async Task Deactivate()
        {
            _view.InqCanvas.IsEnabled = false;
            _inqCanvasModel.LineFinalizedLocally -= OnLineFinalized;
        }


        /*
        private async Task<bool> CheckForTagCreation(InqLineModel line)
        {
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

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));

            return true;
        }
        */
       

        
    }
}
