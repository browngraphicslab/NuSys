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
        private InqLineModel _currentStroke;
        private InqLineView _currentInqLineView;

        public void OnPointerPressed(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            //inqCanvas.Manager.ProcessPointerDown(e.GetCurrentPoint(inqCanvas));
            _currentStroke = new InqLineModel(DateTime.UtcNow.Ticks.ToString());
            _currentStroke.ParentID = inqCanvas.ViewModel.Model.ID;
            _currentInqLineView = new InqLineView(new InqLineViewModel(_currentStroke));
            //TODO: add data binding for thickness and color
            _currentStroke.StrokeThickness = Math.Max(4.0 * e.GetCurrentPoint(inqCanvas).Properties.Pressure, 2);
            _currentStroke.Stroke = new SolidColorBrush(Colors.Yellow);
            _currentInqLineView.StrokeThickness = _currentStroke.StrokeThickness;
            _currentInqLineView.Stroke = _currentStroke.Stroke;
            inqCanvas.Children.Add(_currentInqLineView);
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
            (((InqCanvasViewModel)inqCanvas.DataContext).Model).OnFinalizedLine += delegate
            {
                inqCanvas.Children.Remove(_currentInqLineView);
            };
        }
    }
}
