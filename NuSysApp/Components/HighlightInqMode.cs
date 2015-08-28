using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp.Components
{
    class HighlightInqMode : IInqMode
    {
        private InqLine _currentStroke;

        public void OnPointerPressed(InqCanvas inqCanvas, PointerRoutedEventArgs e)
        {

            _currentStroke = new InqLine();
            _currentStroke.StrokeThickness = Math.Max(4.0 * e.GetCurrentPoint(inqCanvas).Properties.Pressure, 2);
            _currentStroke.SetHighlighting(true);
            _currentStroke.PointerPressed += delegate (object o, PointerRoutedEventArgs e2)
            {

                if (inqCanvas.Mode is EraseInqMode)
                {
                    inqCanvas.Children.Remove(o as InqLine);
                    inqCanvas.Strokes.Remove(o as InqLine);
                }

            };
            inqCanvas.Children.Add(_currentStroke);
        }

        public void OnPointerMoved(InqCanvas inqCanvas, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _currentStroke.AddPoint(new Point(currentPoint.Position.X, currentPoint.Position.Y));
        }

        public void OnPointerReleased(InqCanvas inqCanvas, PointerRoutedEventArgs e)
        {
            inqCanvas.Strokes.Add(_currentStroke);
        }
    }
}
