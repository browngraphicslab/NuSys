using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class TwoFingerScrollViewer : Canvas
    {
        private int _numTouchPoints = 0;

        public TwoFingerScrollViewer()
        {

            ManipulationMode = ManipulationModes.All;

            SizeChanged += delegate
            {
                // TODO: Read hardcoded margins from xaml property
                var r = new Windows.UI.Xaml.Media.RectangleGeometry();
                r.Rect = new Rect(30, 30, Math.Max(Width-60,0), Math.Max(Height-60,0));
                Clip = r;
            };

            PointerWheelChanged += delegate (object sender, PointerRoutedEventArgs e)
            {
                var scrollDir = Math.Sign(e.GetCurrentPoint(this).Properties.MouseWheelDelta);
                Scroll(scrollDir, 10);
                e.Handled = true;
            };

            PointerPressed += delegate ( object sender, PointerRoutedEventArgs e)
            {
                CapturePointer(e.Pointer);
                if (++_numTouchPoints==2) {
                    ManipulationDelta -= OnDelta;
                    ManipulationDelta += OnDelta;
                }
            };

            PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
            {
                if (--_numTouchPoints <= 1)
                {
                    ManipulationDelta -= OnDelta;
                }

                if (--_numTouchPoints <= 0)
                {
                    _numTouchPoints = 0;
                    ReleasePointerCaptures();
                }

                e.Handled = true;
            };       
        }  

        private void OnDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {          
            Scroll(Math.Sign(e.Delta.Translation.Y), Math.Abs(e.Delta.Translation.Y));
            e.Handled = true;
        }

        private void Scroll(int scrollDir, double speed)
        {
            foreach (var c in Children)
            {
                var child = (FrameworkElement)c;
                var currY = Canvas.GetTop(child);

                // TODO: Read hardcoded margins from xaml property
                var diff = -(child.Height - (Height - 60));

                double ty = Math.Min(0, Math.Max(currY + speed * scrollDir, diff));
                Canvas.SetTop(child, ty);
            }
        }      
    }
}
