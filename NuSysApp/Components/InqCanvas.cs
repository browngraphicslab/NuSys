using System;
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
using NuSysApp.Components;

namespace NuSysApp
{

    public class InqCanvas : Canvas
    {
        private bool _isEnabled;
        private InkManager _inkManager = new InkManager();
        private uint _pointerId = uint.MaxValue;
        private IInqMode _mode = new DrawInqMode();
        private Dictionary<Polyline, InkStroke> _strokes = new Dictionary<Polyline, InkStroke>();

        public InqCanvas() : base()
        {
            NuSysApp.MISC.Clip.SetToBounds(this, true);
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_pointerId != uint.MaxValue)
            {
                e.Handled = true;
                return;
            }

            _pointerId = e.Pointer.PointerId;
            CapturePointer(e.Pointer);
            PointerMoved += OnPointerMoved;
            PointerReleased += OnPointerReleased;

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
            ReleasePointerCapture(e.Pointer);

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
                    /*
                    if (_isErasing)
                    {
                        Children.Remove(o as Polyline);
                        _inkManager.SelectWithLine(e2.GetCurrentPoint(this).Position, e2.GetCurrentPoint(this).Position);
                    }
                    */
                };


                Children.Add(pl);
            }

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

        public InkManager Manager
        {
            get
            {
                return _inkManager;
            }
            set
            {
                _inkManager = value;
            }
        }

        public IInqMode Mode
        {
            get { return _mode; }
        }

        internal Dictionary<Polyline, InkStroke> Strokes
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
