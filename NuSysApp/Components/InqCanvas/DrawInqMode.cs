using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class DrawInqMode : IInqMode
    {
        private InqLineModel _currentStroke;
        private InqLineView _currentInqLineView;
        private InkManager _inkManager = new InkManager();

        public DrawInqMode(InqCanvasView view)
        {
            // This adds the final line to the canvas, after the host send it to this client
            //(((InqCanvasViewModel)view.DataContext).Model).OnFinalizedLine += delegate(InqLineModel lineModel)
            //{
            //    var lineView = new InqLineView(new InqLineViewModel(lineModel));
            //    var points = lineModel.Points;
            //    view.Children.Add(lineView);
            //};
        }

        public void OnPointerPressed(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            _inkManager.ProcessPointerDown(e.GetCurrentPoint(inqCanvas));
            _currentStroke = new InqLineModel(DateTime.UtcNow.Ticks.ToString());
            _currentStroke.InqCanvasId = inqCanvas.ViewModel.Model.Id;
            _currentStroke.Stroke = new SolidColorBrush(Colors.Black);
            _currentInqLineView = new InqLineView(new InqLineViewModel(_currentStroke));

            //TODO: add data binding for thickness and color
            _currentStroke.StrokeThickness = Math.Max(4.0 * e.GetCurrentPoint(inqCanvas).Properties.Pressure, 2);
            _currentInqLineView.StrokeThickness = _currentStroke.StrokeThickness;
            inqCanvas.ViewModel.Lines.Add(_currentInqLineView);
            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _currentStroke.AddPoint(new Point2d(currentPoint.Position.X, currentPoint.Position.Y));
        }

        public void OnPointerMoved(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            _inkManager.ProcessPointerUpdate(e.GetCurrentPoint(inqCanvas));
            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _currentStroke.AddPoint(new Point2d(currentPoint.Position.X, currentPoint.Position.Y));
                if (_currentStroke.Points.Count > 1)
                {
                    NetworkConnector.Instance.RequestSendPartialLine(_currentStroke.Id, ((InqCanvasViewModel)inqCanvas.DataContext).Model.Id,
                        _currentStroke.Points[_currentStroke.Points.Count - 2].X.ToString(),
                        _currentStroke.Points[_currentStroke.Points.Count - 2].Y.ToString(),
                        _currentStroke.Points[_currentStroke.Points.Count - 1].X.ToString(),
                        _currentStroke.Points[_currentStroke.Points.Count - 1].Y.ToString());
                }
        }

        public void OnPointerReleased(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            _inkManager.ProcessPointerUp(e.GetCurrentPoint(inqCanvas));
            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _currentStroke.AddPoint(new Point2d(currentPoint.Position.X, currentPoint.Position.Y));
            NetworkConnector.Instance.RequestFinalizeGlobalInk(_currentStroke.Id, ((InqCanvasViewModel)inqCanvas.DataContext).Model.Id, _currentStroke.GetString());
            (((InqCanvasViewModel) inqCanvas.DataContext).Model).LineFinalized += delegate
            {
                inqCanvas.ViewModel.Lines.Remove(_currentInqLineView);
            };

        }

        
    }
}
