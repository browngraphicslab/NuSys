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
            _gestureRecognizer.GestureSettings = GestureSettings.None;
            _gestureRecognizer.Dragging += OnDragging;
            _gestureRecognizer.Tapped += OnTapped;
            _gestureRecognizer.RightTapped += OnRightTapped;
            _gestureRecognizer.Holding += OnHolding;
            _gestureRecognizer.ManipulationStarted += OnManipulationStarted;
            _gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            _gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;
        }

        public virtual void OnDragging(GestureRecognizer sender, DraggingEventArgs args)
        {

        }

        public virtual void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {

        }

        public virtual void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {

        }

        public virtual void OnManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        {

        }

        public virtual void OnHolding(GestureRecognizer sender, HoldingEventArgs args)
        {

        }

        public virtual void OnRightTapped(GestureRecognizer sender, RightTappedEventArgs args)
        {

        }

        public virtual void OnTapped(GestureRecognizer sender, TappedEventArgs args)
        {

        }

        public virtual void OnPressed(CanvasPointer pointer)
        {
            _gestureRecognizer.ProcessDownEvent(pointer.PointerRoutedEventArgs.GetCurrentPoint(pointer.SourceElement));
        }

        public virtual void OnMoved(CanvasPointer pointer)
        {
            _gestureRecognizer.ProcessMoveEvents(pointer.PointerRoutedEventArgs.GetIntermediatePoints(pointer.SourceElement));
        }

        public virtual void OnReleased(CanvasPointer pointer)
        {
            _gestureRecognizer.ProcessUpEvent(pointer.PointerRoutedEventArgs.GetCurrentPoint(pointer.SourceElement));
        }

        public virtual void OnPointerWheelChanged(CanvasPointer pointer)
        {
            _gestureRecognizer.ProcessMouseWheelEvent(pointer.PointerRoutedEventArgs.GetCurrentPoint(pointer.SourceElement), false, false);
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
