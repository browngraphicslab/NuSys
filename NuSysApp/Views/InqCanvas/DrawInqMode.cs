using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class DrawInqMode : IInqMode
    {
        private InqLineModel _currentStroke;
        private InqLineView _currentInqLineView;

        public DrawInqMode(InqCanvasView view)
        {
            // This adds the final line to the canvas, after the host send it to this client
            (((InqCanvasViewModel)view.DataContext).Model).OnFinalizedLine += delegate(InqLineModel lineModel)
            {
                var lineView = new InqLineView(new InqLineViewModel(lineModel));
                view.Children.Add(lineView);
            };
        }

        public void OnPointerPressed(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            //inqCanvas.Manager.ProcessPointerDown(e.GetCurrentPoint(inqCanvas));
            _currentStroke = new InqLineModel(DateTime.UtcNow.Ticks.ToString());
            _currentStroke.ParentID = inqCanvas.ViewModel.Model.ID;
            _currentStroke.Stroke = new SolidColorBrush(Colors.Black);
            _currentInqLineView = new InqLineView(new InqLineViewModel(_currentStroke));

            //TODO: add data binding for thickness and color
            _currentStroke.StrokeThickness = Math.Max(4.0 * e.GetCurrentPoint(inqCanvas).Properties.Pressure, 2);
            _currentInqLineView.StrokeThickness = _currentStroke.StrokeThickness;
            inqCanvas.Children.Add(_currentInqLineView);
            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _currentStroke.AddPoint(new Point(currentPoint.Position.X, currentPoint.Position.Y));
        }

        public void OnPointerMoved(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _currentStroke.AddPoint(new Point(currentPoint.Position.X, currentPoint.Position.Y));
                if (_currentStroke.Points.Count > 1)
                {
                    NetworkConnector.Instance.RequestSendPartialLine(_currentStroke.ID, ((InqCanvasViewModel)inqCanvas.DataContext).Model.ID,
                        _currentStroke.Points[_currentStroke.Points.Count - 2].X.ToString(),
                        _currentStroke.Points[_currentStroke.Points.Count - 2].Y.ToString(),
                        _currentStroke.Points[_currentStroke.Points.Count - 1].X.ToString(),
                        _currentStroke.Points[_currentStroke.Points.Count - 1].Y.ToString());
                }
        }

        public void OnPointerReleased(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _currentStroke.AddPoint(new Point(currentPoint.Position.X, currentPoint.Position.Y));
            NetworkConnector.Instance.RequestFinalizeGlobalInk(_currentStroke.ID, ((InqCanvasViewModel)inqCanvas.DataContext).Model.ID, _currentStroke.GetString());
            (((InqCanvasViewModel) inqCanvas.DataContext).Model).OnFinalizedLine += delegate
            {
                inqCanvas.Children.Remove(_currentInqLineView);
            };
        }
    }
}
