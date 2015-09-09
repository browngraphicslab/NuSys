using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    class HighlightInqMode : IInqMode
    {
        private InqLine _currentStroke;

        public void OnPointerPressed(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {

            _currentStroke = new InqLine(DateTime.UtcNow.Ticks.ToString());
            _currentStroke.StrokeThickness = Math.Max(4.0 * e.GetCurrentPoint(inqCanvas).Properties.Pressure, 2);
            _currentStroke.Stroke = new SolidColorBrush(Colors.Yellow);
            inqCanvas.Children.Add(_currentStroke);
        }

        public void OnPointerMoved(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _currentStroke.AddPoint(new Point(currentPoint.Position.X, currentPoint.Position.Y));
            if (_currentStroke.Points.Count > 1)
            {
                NetworkConnector.Instance.SendPartialLine(_currentStroke.ID, ((InqCanvasViewModel)inqCanvas.DataContext).Model.ID, 
                    _currentStroke.Points[_currentStroke.Points.Count - 2].X.ToString(),
                    _currentStroke.Points[_currentStroke.Points.Count - 2].Y.ToString(),
                    _currentStroke.Points[_currentStroke.Points.Count - 1].X.ToString(),
                    _currentStroke.Points[_currentStroke.Points.Count - 1].Y.ToString());
            }
        }

        public void OnPointerReleased(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            NetworkConnector.Instance.FinalizeGlobalInk(_currentStroke.ID, ((InqCanvasViewModel)inqCanvas.DataContext).Model.ID, _currentStroke.GetString());
            inqCanvas.Children.Remove(_currentStroke);
        }
    }
}
