using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class TapGestureRecognizer : GestureRecognizer
    {
        private TapEventArgs _tapArgs;
        private bool _isDoubleTap;
        private Timer _timer;

        public delegate void TapEventHandler(TapGestureRecognizer sender, TapEventArgs args);
        public event TapEventHandler OnTapped;
        public event TapEventHandler OnDoubleTapped;


        public void ProcessDownEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            if (_tapArgs == null)
            {
                _tapArgs = new TapEventArgs(args.GetCurrentPoint(sender).Position.ToSystemVector2());
                _isDoubleTap = false;
                _timer = new Timer(FireTapped, null, 200, 200);
            }
            else
            {
                _isDoubleTap = true;
            }
            
        }

        private void FireTapped(object state)
        {
            _timer.Dispose();
            if (_isDoubleTap == false)
            {
                OnTapped?.Invoke(this, _tapArgs);
                _tapArgs = null;
            }
        }

        public void ProcessMoveEvents(FrameworkElement sender, PointerRoutedEventArgs args)
        {

        }

        public async void ProcessUpEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            if (_isDoubleTap)
            {
                OnDoubleTapped?.Invoke(this, _tapArgs);
                _tapArgs = null;
            }
        }

        public void ProcessMouseWheelEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
        }

        public void ProcessExitedEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
        }
    }
}
