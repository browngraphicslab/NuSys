using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Components;
using NuSysApp.Views.Workspace;

namespace NuSysApp
{
    public class DrawInqMode : IInqMode
    {
        private Polyline _currentStroke;

        public void OnPointerPressed(InqCanvas inqCanvas, PointerRoutedEventArgs e)
        {
            //inqCanvas.Manager.ProcessPointerDown(e.GetCurrentPoint(inqCanvas));

            _currentStroke = new Polyline();
            _currentStroke.StrokeThickness = Math.Max(4.0 * e.GetCurrentPoint(inqCanvas).Properties.Pressure, 2);
            _currentStroke.Stroke = new SolidColorBrush(Colors.Black);
            _currentStroke.PointerPressed += delegate (object o, PointerRoutedEventArgs e2)
            {
                
                if (inqCanvas.Mode is EraseInqMode)
                {
                    inqCanvas.Children.Remove(o as Polyline);
                    inqCanvas.Strokes.Remove(o as Polyline);
                    //InkStroke inkStroke = inqCanvas.Strokes[o as Polyline];
                    //inqCanvas.Manager.SelectWithLine(e2.GetCurrentPoint(inqCanvas).Position, e2.GetCurrentPoint(inqCanvas).Position);
                    //if (inkStroke.Selected)
                    //{
                    //    inqCanvas.Manager.DeleteSelected();
                    //}

                } 
            };
            inqCanvas.Children.Add(_currentStroke);
        }

        public void OnPointerMoved(InqCanvas inqCanvas, PointerRoutedEventArgs e)
        {
            //inqCanvas.Manager.ProcessPointerUpdate(e.GetCurrentPoint(inqCanvas));
            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _currentStroke.Points.Add(new Point(currentPoint.Position.X, currentPoint.Position.Y));
        }

        public void OnPointerReleased(InqCanvas inqCanvas, PointerRoutedEventArgs e)
        {
            //inqCanvas.Manager.ProcessPointerUp(e.GetCurrentPoint(inqCanvas));
            //var inkStrokes = inqCanvas.Manager.GetStrokes();
            //inqCanvas.Strokes.Add(_currentStroke, inkStrokes[inkStrokes.Count - 1]);
            inqCanvas.Strokes.Add(_currentStroke);
        }
    }
}
