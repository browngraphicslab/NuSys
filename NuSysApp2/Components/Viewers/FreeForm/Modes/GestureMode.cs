﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;
using MyToolkit.UI;
using Windows.UI.Input.Inking;

namespace NuSysApp
{
    public class GestureMode : AbstractWorkspaceViewMode
    {
 
        private long _tLineFinalized;
        private DateTime _tFirstPress;
        private InkStroke _inqLine;
        private bool _wasGesture;
        private bool _released;
       
        private FreeFormViewer _cview;

        public GestureMode(FreeFormViewer view) : base(view)
        {
            var wvm = (FreeFormViewerViewModel)_view.DataContext;
            _cview = (FreeFormViewer) view;           
 
            _tFirstPress = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));
        }

        public override async Task Activate()
        {
            _view.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnPointerPressed), true);
            _view.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnPointerReleased), true);
            _cview.InqCanvas.InkStrokeAdded += OnLineFinalized;
        }

        public override async Task Deactivate()
        {
            _cview.InqCanvas.InkStrokeAdded -= OnLineFinalized;
            _view.RemoveHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnPointerPressed));
            _view.RemoveHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnPointerReleased));
        }

        private void OnPointerReleased(object source, PointerRoutedEventArgs args)
        {
             _released = true;
        }

        private async void  OnPointerPressed(object source, PointerRoutedEventArgs args)
        {
            if (args.Pointer.PointerDeviceType == PointerDeviceType.Pen)
                return;

            _released = false;

            var s = DateTime.Now.Subtract(_tFirstPress).TotalSeconds;
            if (s > 1.5)
            {
                var f = (FrameworkElement)args.OriginalSource;
                var pc = f.DataContext;
                if (_released &&  (pc is FreeFormViewerViewModel))
                    _cview.MultiMenu.Visibility = Visibility.Collapsed;

                return;
            }

            SelectionByStroke();



            _cview.MultiMenu.Stroke = _inqLine;

            var refPoint = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(args.GetCurrentPoint(null).Position);
            if (_cview.MultiMenu.Visibility == Visibility.Collapsed && IsPointCloseToInk(refPoint)) { 
                var p = args.GetCurrentPoint(null).Position;
                _cview.MultiMenu.Show();
                Canvas.SetLeft(_cview.MultiMenu,  p.X + 10);
                Canvas.SetTop(_cview.MultiMenu, p.Y - 60);
            } 
            args.Handled = true;
        }

        private void SelectionByStroke()
        {
            var screenPoints = new Polyline();
            var t = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            foreach (var point in _inqLine.GetInkPoints())
            {
                screenPoints.Points.Add(new Point(point.Position.X, point.Position.Y));
            }

            var hull = new SelectionHull();
            var numSelections = hull.Compute(screenPoints, SessionController.Instance.SessionView.MainCanvas);          
        }

        private void OnLineFinalized(WetDryInkCanvas canvas, InkStroke stroke)
        {
            _inqLine = stroke;
            _tFirstPress = DateTime.Now;
        }

        private bool IsPointCloseToInk(Point p)
        {
            var minDist = double.PositiveInfinity;
            foreach(var inqPoint in _inqLine.GetInkPoints())
            {
                var dist = Math.Sqrt((p.X - inqPoint.Position.X) * (p.X - inqPoint.Position.X) + (p.Y - inqPoint.Position.Y) * (p.Y - inqPoint.Position.Y));
                if (dist < minDist)
                    minDist = dist;
            }
            return minDist < 100;
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
            foreach (var otherLine in _lines.Where(l => l.LibraryId != line.LibraryId && l.LibraryId != lastLine.LibraryId && l.LibraryId != secondLastLine.LibraryId))
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
            m["creators"] = new List<string>() { SessionController.Instance.ActiveFreeFormViewer.LibraryId };

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));

            return true;
        }
        */
       

        
    }
}
