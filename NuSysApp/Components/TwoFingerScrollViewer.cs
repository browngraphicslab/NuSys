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
                var r = new Windows.UI.Xaml.Media.RectangleGeometry();
                r.Rect = new Rect(0, 0, Width, Height);
                Clip = r;
            };

            PointerWheelChanged += delegate (object sender, PointerRoutedEventArgs e)
            {
                var scrollDir = Math.Sign(e.GetCurrentPoint(this).Properties.MouseWheelDelta);
                Scroll(scrollDir, 10);

                e.Handled = true;
            };

            PointerPressed += delegate
            {
                _numTouchPoints++;
            };

            PointerReleased += delegate
            {
                _numTouchPoints--;
            };

            ManipulationDelta += delegate (object sender, ManipulationDeltaRoutedEventArgs e)
            {
                if (_numTouchPoints >=2) {
                    Scroll(Math.Sign(e.Delta.Translation.Y), Math.Abs(e.Delta.Translation.Y));

                    e.Handled = true;
                }
            };
        }  
        
        private void Scroll(int scrollDir, double speed)
        {
            foreach (var c in Children)
            {
                var child = (FrameworkElement)c;
                var currY = Canvas.GetTop(child);
                double ty = Math.Min(0, Math.Max(currY + speed * scrollDir, -(child.Height - Height)));
                Canvas.SetTop(child, ty);
            }
        }      
    }
}
