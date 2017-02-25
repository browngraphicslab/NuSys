using System;
using System.Collections.Generic;
using Windows.UI.Input;

namespace NuSysApp
{
    public class GestureRecognizer
    {
        private Windows.UI.Input.GestureRecognizer _gestureRecognizer;
        private CanvasPointer _pointer;

        public delegate void PointerDragEvent(CanvasPointer pointer, DraggingEventArgs args);

        public event PointerDragEvent OnDragged;

        public GestureRecognizer()
        {
            _gestureRecognizer = new Windows.UI.Input.GestureRecognizer();
            _gestureRecognizer.GestureSettings = GestureSettings.DoubleTap | GestureSettings.Drag | GestureSettings.Hold | GestureSettings.Tap | GestureSettings.RightTap;
            _gestureRecognizer.Dragging += OnDragging;
            _gestureRecognizer.Tapped += OnTapped;
            _gestureRecognizer.RightTapped += OnRightTapped;
            _gestureRecognizer.Holding += OnHolding;
            _gestureRecognizer.ManipulationStarted += OnManipulationStarted;
            _gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            _gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;
        }

        public void Dispose()
        {
            _gestureRecognizer.Dragging -= OnDragging;
            _gestureRecognizer.Tapped -= OnTapped;
            _gestureRecognizer.RightTapped -= OnRightTapped;
            _gestureRecognizer.Holding -= OnHolding;
            _gestureRecognizer.ManipulationStarted -= OnManipulationStarted;
            _gestureRecognizer.ManipulationUpdated -= OnManipulationUpdated;
            _gestureRecognizer.ManipulationCompleted -= OnManipulationCompleted;
        }

        private void OnManipulationCompleted(Windows.UI.Input.GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
        }

        private void OnManipulationUpdated(Windows.UI.Input.GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
        }

        private void OnManipulationStarted(Windows.UI.Input.GestureRecognizer sender, ManipulationStartedEventArgs args)
        {
        }

        private void OnHolding(Windows.UI.Input.GestureRecognizer sender, HoldingEventArgs args)
        {
        }

        private void OnRightTapped(Windows.UI.Input.GestureRecognizer sender, RightTappedEventArgs args)
        {
        }

        private void OnTapped(Windows.UI.Input.GestureRecognizer sender, TappedEventArgs args)
        {
        }

        private void OnDragging(Windows.UI.Input.GestureRecognizer sender, DraggingEventArgs args)
        {
            OnDragged?.Invoke(_pointer, args);
        }

        public void ProcessDownEvent(PointerPoint currentPoint)
        {
        }

        public void ProcessMoveEvents(CanvasPointer pointer)
        {
            _pointer = pointer;
            _gestureRecognizer.ProcessMoveEvents(pointer.PointerRoutedEventArgs.GetIntermediatePoints(pointer.SourceElement));
        }

        public void ProcessUpEvent(CanvasPointer pointer)
        {
            _pointer = pointer;
            _gestureRecognizer.ProcessUpEvent(pointer.PointerRoutedEventArgs.GetCurrentPoint(pointer.SourceElement));
        }

        public void ProcessMouseWheelEvent(CanvasPointer pointer)
        {
            _pointer = pointer;
            _gestureRecognizer.ProcessMouseWheelEvent(pointer.PointerRoutedEventArgs.GetCurrentPoint(pointer.SourceElement), false, false);
        }
    }
}