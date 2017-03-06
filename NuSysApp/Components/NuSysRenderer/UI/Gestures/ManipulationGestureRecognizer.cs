using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using MyToolkit.Converters;

namespace NuSysApp
{
    class ManipulationGestureRecognizer : GestureRecognizer
    {
        private Dictionary<uint, Vector2> _pointerIdToPosition;

        private ManipulationEventArgs _manipulationArgs;

        public void ProcessDownEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            if (!_pointerIdToPosition.ContainsKey(args.Pointer.PointerId))
            {
                _pointerIdToPosition[args.Pointer.PointerId] = args.GetCurrentPoint(sender).Position.ToSystemVector2();
            }
        }

        public void ProcessMoveEvents(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            _pointerIdToPosition[args.Pointer.PointerId] = args.GetCurrentPoint(sender).Position.ToSystemVector2();

            if (_pointerIdToPosition.Count < 2)
            {
                return;
            }

            var sum = _pointerIdToPosition.Values.Aggregate(Vector2.Zero, (current, point) => current + point);
            var focus = sum / _pointerIdToPosition.Count;
            var devSum = _pointerIdToPosition.Values.Aggregate(Vector2.Zero, (current, point) => current + Vector2.Abs(point - focus));
            var dev = devSum / _pointerIdToPosition.Count;
        }

        public void ProcessUpEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            _pointerIdToPosition.Remove(args.Pointer.PointerId);
            if (_pointerIdToPosition.Count < 2)
            {
                
            }
        }

        public void ProcessMouseWheelEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void ProcessExitedEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            _pointerIdToPosition.Remove(args.Pointer.PointerId);
        }


    }
}
