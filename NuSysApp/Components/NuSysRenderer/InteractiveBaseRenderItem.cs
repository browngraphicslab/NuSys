using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Core;
using System.Numerics;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class InteractiveBaseRenderItem : BaseRenderItem
    {
        public delegate void PointerHandler(InteractiveBaseRenderItem item, CanvasPointer pointer);

        public event PointerHandler Pressed;
        public event PointerHandler Released;
        public event PointerHandler DoubleTapped;
        public event PointerHandler Tapped;
        public event PointerHandler Dragged;
        public event PointerHandler DragStarted;
        public event PointerHandler DragCompleted;
        public event PointerHandler PenPointerPressed;
        public event PointerHandler PenPointerReleased;
        public event PointerHandler PenPointerDragged;
        public event PointerHandler PenPointerCompleted;
        public event PointerHandler PenPointerDragStarted;
        

        private bool _isDragging;
        private bool _isPenDragging;


        public delegate void PointerWheelHandler(InteractiveBaseRenderItem item, CanvasPointer pointer, float delta);
        public event PointerWheelHandler PointerWheelChanged;


        // Delegate for the KeyPressed event
        public delegate void KeyPressedDelegate(KeyArgs args);
        // Event that fires when a key is pressed on this render item
        public event KeyPressedDelegate KeyPressed;


        // Delegate for the KeyReleased event
        public delegate void KeyReleasedDelegate(KeyArgs args);
        // Event that fires when a key is released on this render item
        public event KeyPressedDelegate KeyReleased;

        //Delegate for Holding event
        public delegate void HoldingHandler(InteractiveBaseRenderItem item, Vector2 point);
        //Event that fires when holding a render item with your fingers
        public HoldingHandler Holding;
        private GestureRecognizer _gestureRecognizer;

        public InteractiveBaseRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _gestureRecognizer = new GestureRecognizer();
        }

        public virtual void OnDragged(GestureRecognizer sender, DraggingEventArgs args)
        {

        }

        public virtual void OnManipulationCompleted(CanvasPointer pointer)
        {

        }

        public virtual void OnManipulationUpdated(CanvasPointer pointer)
        {

        }

        public virtual void OnManipulationStarted(CanvasPointer pointer)
        {

        }

        public virtual void OnHolding(CanvasPointer pointer)
        {

        }

        public virtual void OnRightTapped(CanvasPointer pointer)
        {

        }

        public virtual void OnTapped(CanvasPointer pointer)
        {

        }


        public virtual void OnDoubleTapped(CanvasPointer pointer)
        {

        }

        public virtual void OnPressed(CanvasPointer pointer)
        {
            UITask.Run(() =>
            {
                _gestureRecognizer.ProcessDownEvent(pointer.PointerRoutedEventArgs.GetCurrentPoint(pointer.SourceElement));
            });
        }

        public virtual void OnMoved(CanvasPointer pointer)
        {
            UITask.Run(() =>
            {
                _gestureRecognizer.ProcessMoveEvents(
                    pointer.PointerRoutedEventArgs.GetIntermediatePoints(pointer.SourceElement));
            });
        }

        public virtual void OnReleased(CanvasPointer pointer)
        {
            UITask.Run(() =>
            {
                _gestureRecognizer.ProcessUpEvent(pointer.PointerRoutedEventArgs.GetCurrentPoint(pointer.SourceElement));
            });
        }

        public virtual void OnPointerWheelChanged(CanvasPointer pointer)
        {
            UITask.Run(() =>
            {
                _gestureRecognizer.ProcessMouseWheelEvent(
                    pointer.PointerRoutedEventArgs.GetCurrentPoint(pointer.SourceElement));
            });
        }

        // Function fired when key is pressed on this render item
        // Invokes the KeyPressed event if possible
        public virtual void OnKeyPressed(KeyArgs args)
        {
            KeyPressed?.Invoke(args);
        }

        // Function fired when key is released on this render item
        // Invokes the KeyReleased event if possible
        public virtual void OnKeyReleased(KeyArgs args)
        {
            KeyReleased?.Invoke(args);
        }

        // Partial override of BaseRenderItem GotFocus in order to add KeyPressed event
        public override void GotFocus()
        {
            SessionController.Instance.FocusManager.OnKeyPressed += OnKeyPressed;
            SessionController.Instance.FocusManager.OnKeyReleased += OnKeyReleased;
            base.GotFocus();
        }

        // Partial override of BaseRenderItem LostFocus in order to remove KeyPressed event
        public override void LostFocus()
        {
            SessionController.Instance.FocusManager.OnKeyPressed -= OnKeyPressed;
            SessionController.Instance.FocusManager.OnKeyReleased -= OnKeyReleased;
            base.LostFocus();
        }
        /// <summary>
        /// you probably do not want to call this directly since it is automatically called at the lowest level when a drag starts.
        /// calling this directly will result in drag started being fired twice
        /// </summary>
        /// <param name="pointer"></param>
        public virtual void OnDragStarted(CanvasPointer pointer)
        {
            DragStarted?.Invoke(this, pointer);
        }

        /// <summary>
        /// you probably do not want to call this directly since it is automatically called at the lowest level when a drag starts.
        /// calling this directly will result in drag completed being fired twice
        /// </summary>
        /// <param name="pointer"></param>
        public virtual void OnDragCompleted(CanvasPointer pointer)
        {
            DragCompleted?.Invoke(this, pointer);
        }
        
    }
}
