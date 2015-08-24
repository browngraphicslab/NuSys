﻿using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Components;

namespace NuSysApp
{

    public class InqCanvas : Canvas
    {
        private bool _isEnabled;
        //private InkManager _inkManager = new InkManager();
        private uint _pointerId = uint.MaxValue;
        private IInqMode _mode = new DrawInqMode();
        private HashSet<InqLine> _strokes = new HashSet<InqLine>();
        public bool IsPressed = false;

        public InqCanvas()
        {
            MISC.Clip.SetToBounds(this, true);
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_pointerId != uint.MaxValue)
            {
                e.Handled = true;
                return;
            }

            _pointerId = e.Pointer.PointerId;
            if (_mode is DrawInqMode)
            {
                CapturePointer(e.Pointer);
            }
            PointerMoved += OnPointerMoved;
            PointerReleased += OnPointerReleased;
            IsPressed = true;

            _mode.OnPointerPressed(this, e);

            e.Handled = true;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId != _pointerId)
            {
                e.Handled = true;
                return;
            }

            _mode.OnPointerMoved(this, e);
            
            e.Handled = true;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId != _pointerId)
            {
                e.Handled = true;
                return;
            }

            PointerMoved -= OnPointerMoved;
            PointerReleased -= OnPointerReleased;
            _pointerId = uint.MaxValue;
            if (this.PointerCaptures.Count != 0)
            {
                ReleasePointerCapture(e.Pointer);
            }
            IsPressed = false;

            _mode.OnPointerReleased(this, e);

            e.Handled = true;
        }
        
        /// <summary>
        /// Turns erasing on or off
        /// </summary>
        /// <param name="erase"></param>
        public void SetErasing(bool erase)
        {
            if (erase)
            {
                _mode = new EraseInqMode();
            }
            else
            {
                _mode = new DrawInqMode();
            }
        }

        /// <summary>
        /// Turns highlighting on or off
        /// </summary>
        /// <param name="highlight"></param>
        public void SetHighlighting(bool highlight)
        {

            if (highlight)
            {
                _mode = new HighlightInqMode();
            }
            else
            {
                _mode = new DrawInqMode();
            }
        }

        //public void RemoveByInkStroke(InkStroke stroke)
        //{
        //    var line = _strokes[stroke];
        //    if (line != null)
        //        Children.Remove(line);
        //}

        public Rect PasteStrokes(InqLine[] lines)
        {

            double width = 0;
            double height = 0;
            foreach (var stroke in lines)
            {
                var pl = new InqLine();

                var points = stroke.Points;
                var minX = points.Min(em => em.X);
                var minY = points.Min(em => em.Y);
                var maxX = points.Max(em => em.X);
                var maxY = points.Max(em => em.Y);

                width = maxX - minX;
                height = maxY - minY;

                foreach (var point in stroke.Points)
                {
                    pl.StrokeThickness = stroke.StrokeThickness;
                    //pl.Stroke = stroke.Stroke != null ? stroke.Stroke : new SolidColorBrush(Color.FromArgb(0,0,0,1));
                    pl.Points.Add(new Point(point.X - minX, point.Y - minY));
                }


                //         pl.PointerPressed += delegate (object o, PointerRoutedEventArgs e2)
                //         {
                //             /*
                //             if (_isErasing)
                //             {
                //                 Children.Remove(o as InqLine);
                //                 _inkManager.SelectWithLine(e2.GetCurrentPoint(this).Position, e2.GetCurrentPoint(this).Position);
                //             }
                //             */
                //         };


                Children.Add(pl);
            }
            Rect rect = new Rect();
            rect.Width = width;
            rect.Height = height;
            return rect;
        }

        public bool IsEnabled {
            get
            {
                return _isEnabled;
            }
            set
            {
                if (Parent == null)
                    return;

                if (value ==true)
                {
                    PointerPressed += OnPointerPressed;

                } else
                {
                    PointerPressed -= OnPointerPressed;
                    PointerMoved -= OnPointerMoved;
                    PointerReleased -= OnPointerReleased;
                }
                _isEnabled = value;
            }
        }

        //public InkManager Manager
        //{
        //    get
        //    {
        //        return _inkManager;
        //    }
        //    set
        //    {
        //        _inkManager = value;
        //    }
        //}

        public IInqMode Mode
        {
            get { return _mode; }
        }

        internal HashSet<InqLine> Strokes
        {
            get
            {
                return _strokes;
            }
            set
            {
                _strokes = value;
            }
        }
    }
}
