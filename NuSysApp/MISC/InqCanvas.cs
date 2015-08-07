﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp.MISC
{
    
    public class InqCanvas : Canvas
    {
        private InkManager _inkManager = new InkManager();
        private bool _isDrawing = false;
        private bool _isInkingEnabled = false;
        private Polyline _currentStroke;
        private bool _isErasing = false;
        private bool _isHighlighting = false;
        private Dictionary<InkStroke, Polyline> _strokes = new Dictionary<InkStroke, Polyline>();

        public InqCanvas():base()
        {
            NuSysApp.MISC.Clip.SetToBounds(this, true);           
        }
        
        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isInkingEnabled && !_isErasing)
            {
                Debug.WriteLine("Pressed " + Parent);

                CapturePointer(e.Pointer);

                _currentStroke = new Polyline();
                _currentStroke.StrokeThickness = Math.Max(4.0 * e.GetCurrentPoint(this).Properties.Pressure, 2);
                if (_isHighlighting)
                {
                    _currentStroke.Stroke = new SolidColorBrush(Colors.Yellow);
                }
                else
                {
                    _currentStroke.Stroke = new SolidColorBrush(Colors.Black);
                }
                
                _currentStroke.PointerPressed += delegate (object o, PointerRoutedEventArgs e2)
                {
                    if (_isErasing)
                    {
                        Children.Remove(o as Polyline);
                        _inkManager.SelectWithLine(e2.GetCurrentPoint(this).Position, e2.GetCurrentPoint(this).Position);
                    }
                };

                PointerPoint p = e.GetCurrentPoint(this);

                Children.Add(_currentStroke);
                _inkManager.ProcessPointerDown(e.GetCurrentPoint(this));
                _isDrawing = true;
            }

            e.Handled = true;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isErasing || !_isDrawing || !_isInkingEnabled)
                return;

            _inkManager.ProcessPointerUpdate(e.GetCurrentPoint(this));
            var currentPoint = e.GetCurrentPoint(this);
            _currentStroke.Points.Add(new Point(currentPoint.Position.X, currentPoint.Position.Y));
            _isDrawing = true;
            e.Handled = true;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_isInkingEnabled && !_isErasing)
            {
                Debug.WriteLine("Released " + Parent);
                ReleasePointerCapture(e.Pointer);
                _inkManager.ProcessPointerUp(e.GetCurrentPoint(this));
                var inkStrokes = _inkManager.GetStrokes();
                _strokes.Add(inkStrokes[inkStrokes.Count-1], _currentStroke);
                _isDrawing = false;
            }
            e.Handled = true;
        }
        
        /// <summary>
        /// Turns erasing on or off
        /// </summary>
        /// <param name="erase"></param>
        public void SetErasing(bool erase)
        {
            _isErasing = erase;
            if (erase)
            { 
                _inkManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Erasing;
            }
            else
            {
                _inkManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Inking;
            }
            _isHighlighting = false;
        }

        /// <summary>
        /// Turns highlighting on or off
        /// </summary>
        /// <param name="highlight"></param>
        public void SetHighlighting(bool highlight)
        {
            _isHighlighting = highlight;
            _isErasing = false;
        }

        public void RemoveByInkStroke(InkStroke stroke)
        {
            var line = _strokes[stroke];
            if (line != null)
                Children.Remove(line);
        }

        public Rect PasteManagedStrokes()
        {
            var rect = Manager.PasteFromClipboard(new Point(0, 0));
            var strokes = _inkManager.GetStrokes();

            foreach (var stroke in strokes)
            {
                var pl = new Polyline();

                var points = stroke.GetInkPoints();
                var minX = points.Min(em => em.Position.X);
                var minY = points.Min(em => em.Position.Y);

                foreach (var point in stroke.GetInkPoints())
                {
                    pl.StrokeThickness = Math.Max(4.0 * point.Pressure, 2);
                    pl.Stroke = new SolidColorBrush(Colors.Black);
                    pl.Points.Add(new Point(point.Position.X - minX, point.Position.Y - minY));
                }


                pl.PointerPressed += delegate (object o, PointerRoutedEventArgs e2)
                {
                    if (_isErasing)
                    {
                        Children.Remove(o as Polyline);
                        _inkManager.SelectWithLine(e2.GetCurrentPoint(this).Position, e2.GetCurrentPoint(this).Position);
                    }
                };


                Children.Add(pl);
            }

            return rect;
        }

        public bool IsEnabled {
            get
            {
                return _isInkingEnabled;
            }
            set
            {
                if (Parent == null)
                    return;

                if (value ==true)
                {
                    PointerPressed += OnPointerPressed;
                    PointerMoved += OnPointerMoved;
                    PointerReleased += OnPointerReleased;
                } else
                {
                    PointerPressed -= OnPointerPressed;
                    PointerMoved -= OnPointerMoved;
                    PointerReleased -= OnPointerReleased;
                }
                _isInkingEnabled = value;
            }
        }

        public InkManager Manager
        {
            get
            {
                return _inkManager;
            }
        }
    }
}
