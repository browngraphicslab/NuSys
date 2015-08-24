﻿using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Components;

namespace NuSysApp
{
    public class DrawInqMode : IInqMode
    {
        private InqLine _currentStroke;

        public void OnPointerPressed(InqCanvas inqCanvas, PointerRoutedEventArgs e)
        {
            //inqCanvas.Manager.ProcessPointerDown(e.GetCurrentPoint(inqCanvas));

            _currentStroke = new InqLine();
            _currentStroke.StrokeThickness = Math.Max(4.0 * e.GetCurrentPoint(inqCanvas).Properties.Pressure, 2);
            _currentStroke.PointerEntered += delegate (object o, PointerRoutedEventArgs e2)
            {
                
                if (inqCanvas.Mode is EraseInqMode && inqCanvas.IsPressed)
                {
                    InqLine me = o as InqLine;
                    inqCanvas.Children.Remove(me);
                    inqCanvas.Strokes.Remove(me);
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
