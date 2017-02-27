using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class DragGestureRecognizer : GestureRecognizer
    {
        private DragEventArgs _dragEventArgs;

        private bool _isDragging;

        public delegate void DragEventHandler(DragGestureRecognizer sender, DragEventArgs args);

        public event DragEventHandler OnDragged;

        public void ProcessDownEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            Debug.Assert(_isDragging == false);
            _dragEventArgs = new DragEventArgs(args.GetCurrentPoint(sender).Position.ToSystemVector2());
        }

        public void ProcessMoveEvents(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            if (_dragEventArgs == null)
            {
                return;
            }

            _isDragging = true;
            _dragEventArgs.Update(args.GetCurrentPoint(sender).Position.ToSystemVector2());
            OnDragged?.Invoke(this, _dragEventArgs);
        }

        public void ProcessUpEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            CompleteCurrentDrag(sender, args);
        }

        public void ProcessMouseWheelEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {

        }

        public void ProcessExitedEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            CompleteCurrentDrag(sender, args);
        }

        private void CompleteCurrentDrag(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            if (_isDragging == false)
            {
                _dragEventArgs = null;
                return;
            }

            _dragEventArgs.Complete(args.GetCurrentPoint(sender).Position.ToSystemVector2());
            OnDragged?.Invoke(this, _dragEventArgs);
            _isDragging = false;
            _dragEventArgs = null;
        }
    }
}
