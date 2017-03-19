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
    public class ManipulationGestureRecognizer : GestureRecognizer
    {
        private Dictionary<uint, Vector2> _pointerIdToPosition = new Dictionary<uint, Vector2>();

        public delegate void ManipulationEventHandler(ManipulationGestureRecognizer sender, ManipulationEventArgs args);

        public event ManipulationEventHandler OnManipulation;

        private ManipulationEventArgs _manipulationArgs = new ManipulationEventArgs();

        private float previousSpan = 0.0f;

        private bool _isMoveEventProcessed;

        public void ProcessDownEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            if (!_pointerIdToPosition.ContainsKey(args.Pointer.PointerId))
            {
                _pointerIdToPosition[args.Pointer.PointerId] = args.GetCurrentPoint(sender).Position.ToSystemVector2();
            }

            var sum = _pointerIdToPosition.Values.Aggregate(Vector2.Zero, (current, point) => current + point);
            var focus = sum / _pointerIdToPosition.Count;

            _manipulationArgs.PreviousFocus = focus;
            _manipulationArgs.CurrentFocus = focus;

            var devSum = _pointerIdToPosition.Values.Aggregate(Vector2.Zero, (current, point) => current + Vector2.Abs(point - focus));
            var dev = devSum / _pointerIdToPosition.Count;
            var span = (2.0f * dev).Length();

            previousSpan = span;

            _manipulationArgs.ScaleDelta = (previousSpan == 0.0f ? 1.0f : span / previousSpan);
        }

        public void ProcessMoveEvents(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            _pointerIdToPosition[args.Pointer.PointerId] = args.GetCurrentPoint(sender).Position.ToSystemVector2();

            var sum = _pointerIdToPosition.Values.Aggregate(Vector2.Zero, (current, point) => current + point);
            var focus = sum / _pointerIdToPosition.Count;
            var devSum = _pointerIdToPosition.Values.Aggregate(Vector2.Zero, (current, point) => current + Vector2.Abs(point - focus));
            var dev = devSum / _pointerIdToPosition.Count;
            var span = (2.0f * dev).Length();

            _manipulationArgs.Focus = focus;

            _manipulationArgs.PreviousFocus = _manipulationArgs.CurrentFocus;
            _manipulationArgs.CurrentFocus = focus;

            _manipulationArgs.ScaleDelta = (previousSpan == 0.0f ? 1.0f : span / previousSpan);

            previousSpan = span;

            OnManipulation?.Invoke(this, _manipulationArgs);
        }

        public void ProcessUpEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
            _pointerIdToPosition.Remove(args.Pointer.PointerId);

            if (_pointerIdToPosition.Any())
            {
                var sum = _pointerIdToPosition.Values.Aggregate(Vector2.Zero, (current, point) => current + point);
                var focus = sum / _pointerIdToPosition.Count;

                _manipulationArgs.PreviousFocus = focus;
                _manipulationArgs.CurrentFocus = focus;

                var devSum = _pointerIdToPosition.Values.Aggregate(Vector2.Zero, (current, point) => current + Vector2.Abs(point - focus));
                var dev = devSum / _pointerIdToPosition.Count;
                var span = (2.0f * dev).Length();

                previousSpan = span;

                _manipulationArgs.ScaleDelta = (previousSpan == 0.0f ? 1.0f : span / previousSpan);
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
