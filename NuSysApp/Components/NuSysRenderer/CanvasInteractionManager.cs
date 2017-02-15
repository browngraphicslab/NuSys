using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{

   public class CanvasInteractionManager : IDisposable
    {
        /// <summary>
        /// The interaction types that pointers can support, basically determines if the pointer comes from a finger, mouse, or pen
        /// </summary>
        public enum InteractionType
        {
            Mouse,
            Pen,
            Touch
        }
        public delegate void TappedHandler(BaseRenderItem element, PointerRoutedEventArgs args);
        public delegate void TwoPointerPressedHandler(CanvasPointer pointer1, CanvasPointer pointer2);
        public delegate void PointerPressedHandler(CanvasPointer pointer);
        public delegate void MarkingMenuPointerReleasedHandler();
        public delegate void MarkingMenuPointerMoveHandler(Vector2 p);
        public delegate void PointerWheelHandler(CanvasPointer pointer, float delta);
        public delegate void HoldingHandler(Vector2 point);
        public delegate void TranslateHandler(CanvasPointer pointer, Vector2 point, Vector2 delta );
        public delegate void PanZoomHandler(Vector2 center, Vector2 deltaTranslation, float deltaZoom);
        public event TranslateHandler Translated;
        public event PanZoomHandler PanZoomed;
        public event MarkingMenuPointerReleasedHandler AllPointersReleased;
        public event PointerPressedHandler PointerMoved;
        public event PointerPressedHandler PointerPressed;
        public event PointerPressedHandler PointerReleased;
        public event PointerPressedHandler ItemTapped;
        public event PointerPressedHandler ItemLongTapped;
        public event PointerPressedHandler ItemDoubleTapped;
        public event PointerWheelHandler PointerWheelChanged;
        public event HoldingHandler Holding;
        public event TwoPointerPressedHandler TwoPointerPressed;
        public event MarkingMenuPointerMoveHandler MarkingMenuPointerMove;

        /// <summary>
        /// Event fired whenever the last interaction changed the stored type of interaction.
        /// The passed InteractionType is the latest interaction type 
        /// </summary>
        public event EventHandler<InteractionType> InteractionTypeChanged;

        private CanvasPointer _lastTappedPointer = new CanvasPointer();
        private Vector2 _centerPoint;
        private double _twoFingerDist;
        private double _twoFingerDistTarget;
        private List<CanvasPointer> _pointers = new List<CanvasPointer>();


        /// <summary>
        /// The xaml element which maps pointer events to our win2d pointer handling system.
        /// This is like the gateway to our app!
        /// </summary>
        private FrameworkElement _canvas;
        private bool _cancelLongTapped;

        /// <summary>
        /// True if interactions are currently enabled for the app, false otherwise. When this is
        /// false no interactions will be sent to any level of the win2d framework
        /// </summary>
        private bool _isEnabled = true;
        private InteractionType _lastInteractionType = InteractionType.Touch;

        /// <summary>
        /// Gesture recognizer for the interactions
        /// </summary>
        private Windows.UI.Input.GestureRecognizer _gestureRecognizer;

        /// <summary>
        /// The interaction type of the last pointer down event.
        /// </summary>
        public InteractionType LastInteractionType
        {
            get { return _lastInteractionType;}
        }
        public List<CanvasPointer> ActiveCanvasPointers { get { return _pointers; } } 
        
        public CanvasInteractionManager(FrameworkElement pointerEventSource)
        {
            _canvas = pointerEventSource;
            _canvas.PointerPressed += OnPointerPressed;
            _canvas.PointerReleased += OnPointerReleased;
            _canvas.PointerWheelChanged += ResourceCreatorOnPointerWheelChanged;
            _canvas.PointerCaptureLost += CanvasOnPointerExited;
            _canvas.PointerCanceled += CanvasOnPointerExited;
            _canvas.PointerExited += CanvasOnPointerExited;
            _canvas.Holding += OnHolding;
            AllPointersReleased += OnAllPointersReleased;
            SetEnabled(true);

            // gesture recognizer based on this code http://bsubramanyamraju.blogspot.com/2015/01/windowsphone-81-gesture-support-with.html
            // another example is here https://github.com/Microsoft/Windows-universal-samples/blob/master/Samples/BasicInput/cs/5-GestureRecognizer.xaml.cs
            // create the gesture recognizer
            _gestureRecognizer = new Windows.UI.Input.GestureRecognizer();
            _gestureRecognizer.CrossSliding += gr_CrossSliding;
            _gestureRecognizer.Dragging += gr_Dragging;
            _gestureRecognizer.Holding += gr_Holding;
            _gestureRecognizer.ManipulationCompleted += gr_ManipulationmCompleted;
            _gestureRecognizer.ManipulationInertiaStarting += gr_ManipulationInertiaStarting;
            _gestureRecognizer.ManipulationStarted += gr_ManipulationStarted;
            _gestureRecognizer.ManipulationUpdated += gr_ManipulationUpdated;
            _gestureRecognizer.RightTapped += gr_RightTapped;
            _gestureRecognizer.Tapped += gr_Tapped;
            _gestureRecognizer.GestureSettings = GestureSettings.ManipulationRotate | GestureSettings.ManipulationTranslateX | GestureSettings.ManipulationTranslateY |
            GestureSettings.ManipulationScale | GestureSettings.ManipulationRotateInertia | GestureSettings.ManipulationScaleInertia |
            GestureSettings.ManipulationTranslateInertia | GestureSettings.Tap | GestureSettings.DoubleTap;

            // route events to the gesture recognizer through the canvas
            _canvas.PointerCanceled += _canvasGRPointCanceled;
            _canvas.PointerPressed += _canvasGRPointPressed;
            _canvas.PointerReleased += _canvasGRPointReleased;
            _canvas.PointerMoved += _canvasGRPointMoved;
        }

        private void _canvasGRPointMoved(object sender, PointerRoutedEventArgs e)
        {
            _gestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(_canvas));
        }

        private void _canvasGRPointReleased(object sender, PointerRoutedEventArgs e)
        {
            _gestureRecognizer.ProcessUpEvent(e.GetCurrentPoint(_canvas));
            e.Handled = true;
        }

        private void _canvasGRPointPressed(object sender, PointerRoutedEventArgs e)
        {
            _gestureRecognizer.ProcessDownEvent(e.GetCurrentPoint(_canvas));
            // Set the pointer capture to the element being interacted with  
            _canvas.CapturePointer(e.Pointer);
            // Mark the event handled to prevent execution of default handlers  
            e.Handled = true;
        }

        private void _canvasGRPointCanceled(object sender, PointerRoutedEventArgs e)
        {
            _gestureRecognizer.CompleteGesture();
            e.Handled = true;
        }

        private void gr_Tapped(Windows.UI.Input.GestureRecognizer sender, TappedEventArgs args)
        {
        }

        private void gr_RightTapped(Windows.UI.Input.GestureRecognizer sender, RightTappedEventArgs args)
        {
        }

        private void gr_ManipulationUpdated(Windows.UI.Input.GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
        }

        private void gr_ManipulationStarted(Windows.UI.Input.GestureRecognizer sender, ManipulationStartedEventArgs args)
        {
        }

        private void gr_ManipulationInertiaStarting(Windows.UI.Input.GestureRecognizer sender, ManipulationInertiaStartingEventArgs args)
        {
        }

        private void gr_ManipulationmCompleted(Windows.UI.Input.GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
        }

        private void gr_Holding(Windows.UI.Input.GestureRecognizer sender, HoldingEventArgs args)
        {
        }

        private void gr_Dragging(Windows.UI.Input.GestureRecognizer sender, DraggingEventArgs args)
        {
        }

        private void gr_CrossSliding(Windows.UI.Input.GestureRecognizer sender, CrossSlidingEventArgs args)
        {
        }


        public virtual void Dispose()
       {
           SetEnabled(false);
            _canvas.PointerPressed -= OnPointerPressed;
            _canvas.PointerReleased -= OnPointerReleased;
            _canvas.PointerWheelChanged -= ResourceCreatorOnPointerWheelChanged;
            _canvas.Holding -= OnHolding;
            _canvas.PointerCaptureLost -= CanvasOnPointerExited;
            _canvas.PointerCanceled -= CanvasOnPointerExited;
            _canvas.PointerExited -= CanvasOnPointerExited;
            AllPointersReleased -= OnAllPointersReleased;
        }

       public void SetEnabled(bool enabled)
       {
            if (!enabled)
                _pointers.Clear();
            _isEnabled = enabled;
       }

       private void CanvasOnPointerExited(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
       {
          if (pointerRoutedEventArgs.OriginalSource != _canvas)
                OnPointerReleased(sender, pointerRoutedEventArgs);
       }

       private void OnAllPointersReleased()
       {
           _twoFingerDist = 0;
           _twoFingerDistTarget = 0;
           _cancelLongTapped = false;
       }

       private void ResourceCreatorOnPointerWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            var p = args.GetCurrentPoint(_canvas).Position;
            var newCenter = new Vector2((float)p.X, (float)p.Y);
            _centerPoint = newCenter;

            var point = args.GetCurrentPoint(_canvas);
            var delta = Math.Sign((double)args.GetCurrentPoint(_canvas).Properties.MouseWheelDelta);
            PointerWheelChanged?.Invoke(new CanvasPointer(point), delta);
        }


        private void OnHolding(object sender, HoldingRoutedEventArgs args)
        {
            if (!_isEnabled)
            {
                return;
            }
            var point = args.GetPosition(_canvas).ToSystemVector2();
            Holding?.Invoke(point);

        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            if (!_isEnabled)
                return;

            OnPointerTouchReleased(sender, args);
        }

        /// <summary>
        /// Called whenever a pointer is pressed on the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            // make sure all elements are responding to the correct interaction type
            //todo this is probably deprecated in the new interaction scheme
            if (_lastInteractionType != PointerToInteractionType(args.Pointer))
            {
                _lastInteractionType = PointerToInteractionType(args.Pointer);
                InteractionTypeChanged?.Invoke(this, _lastInteractionType);
            }

            // if interactions are currently disabled simply return
            if (!_isEnabled)
                return;

            // call the correct pointer pressed events based on the interactions type
            switch (_lastInteractionType)
            {
                case InteractionType.Pen:
                    OnPointerPenPressed(sender, args);
                    break;
                case InteractionType.Mouse:
                case InteractionType.Touch:
                    OnPointerTouchOrMousePressed(sender, args);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }


        private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            if (!_isEnabled)
                return;
            OnPointerTouchMoved(sender, args);
        }

        private void UpdateCenterPoint()
        {
            if (_pointers.Count == 1)
            {
                _centerPoint = new Vector2(_pointers[0].CurrentPoint.X, _pointers[0].CurrentPoint.Y);
            } else {
                var p0 = _pointers[0].GetBufferMean();
                var p1 = _pointers[1].GetBufferMean();
                _centerPoint = new Vector2((p0.X + p1.X)/2f, (p0.Y + p1.Y)/2f);
            }
        }

        private void UpdateDist()
        {
            var points = _pointers.Select(p => p.CurrentPoint).ToArray();
            var p0 = _pointers[0].GetBufferMean();
            var p1 = _pointers[1].GetBufferMean();
            _twoFingerDist = MathUtil.Dist(p0, p1);
        }


        private void OnPointerPenPressed(object sender, PointerRoutedEventArgs args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called whenever a touch or mouse pointer is pressed on the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPointerTouchOrMousePressed(object sender, PointerRoutedEventArgs e)
        {
            // capturing the pointer makes sure that all events are sent to the canvas even if the point moves off the canvas
            // or is moving to rapidly, basically takes care of any edge cases
            _canvas.CapturePointer(e.Pointer);

            // add a new CanvasPointer to the list of pointers currently in contact with the app, where CanvasPointer is a wrapper around 
            // the PointerPoint class. We get the pointer in the same coordinate system as the canvas, which essentially fills the full window
            // of our app 
            _pointers.Add(new CanvasPointer(e.GetCurrentPoint(_canvas)));

            UpdateCenterPoint();

            PointerPressed?.Invoke(_pointers.Last());

            if (_pointers.Count >= 2) {
                _pointers[0].StartBuffering(5);
                _pointers[1].StartBuffering(5);
                UpdateCenterPoint();
                UpdateDist();
               
                if (_pointers[0].MillisecondsActive > 300)
                {
                    _cancelLongTapped = true;
                    TwoPointerPressed?.Invoke(_pointers[0], _pointers[1]);
                } 
            }

            //these lines actually do something, trust me
            _canvas.PointerMoved -= OnPointerMoved; 
            _canvas.PointerMoved += OnPointerMoved;

        }

        private async void OnPointerTouchReleased(object sender, PointerRoutedEventArgs e)
        {
            _canvas.ReleasePointerCapture(e.Pointer);
            var exisitingPointer = _pointers.Where(p => p.PointerId == e.Pointer.PointerId);
            if (!exisitingPointer.Any())
                return;

            var releasedPointer = exisitingPointer.First();
            releasedPointer.Update(e.GetCurrentPoint(_canvas));
            _pointers.Remove(releasedPointer);
            PointerReleased?.Invoke(releasedPointer);
            if (_pointers.Count == 1)
            {
                if (releasedPointer.MillisecondsActive < 200)
                {
                    ItemTapped?.Invoke(releasedPointer);
                }
            }

            if (_pointers.Count == 0)
            {
                _canvas.PointerMoved -= OnPointerMoved;

                if (releasedPointer.DistanceTraveled < 20 && releasedPointer.StartTimeDelta(_lastTappedPointer) > 300 )
                {
                    if (releasedPointer.MillisecondsActive < 150)
                    {
                        ItemTapped?.Invoke(releasedPointer);
                        _lastTappedPointer = releasedPointer;
                    }
                    else if (releasedPointer.MillisecondsActive > 250)
                    {
                        if (!_cancelLongTapped) { 
                            ItemLongTapped?.Invoke(releasedPointer);
                        }
                    }
                }
                else if ((releasedPointer.DistanceTraveled < 20 && releasedPointer.StartTimeDelta(_lastTappedPointer) < 300))
                {
                    ItemDoubleTapped?.Invoke(releasedPointer);
                }

                AllPointersReleased?.Invoke();
               
            }
        }

        private void OnPointerTouchMoved(object sender, PointerRoutedEventArgs args)
        {
            var exisitingPointer = _pointers.Where(p => p.PointerId == args.Pointer.PointerId);
            if (!exisitingPointer.Any())
                return;

            var pointer = exisitingPointer.First();
            pointer.Update(args.GetCurrentPoint(_canvas));

            if (_pointers.Count == 1)
            {
                if (Math.Abs(pointer.DeltaSinceLastUpdate.X) > 0 || Math.Abs(pointer.DeltaSinceLastUpdate.Y) > 0)
                {
                    Translated?.Invoke(pointer, pointer.CurrentPoint, pointer.DeltaSinceLastUpdate);
                }
            }
            if (_pointers.Count >= 2)
            {
                var prevCenterPoint = _centerPoint;
                var prevDist = _twoFingerDist;
                UpdateCenterPoint();
                UpdateDist();
                var dx = _centerPoint.X - prevCenterPoint.X;
                var dy = _centerPoint.Y - prevCenterPoint.Y;
                var ds = (float)(_twoFingerDist / prevDist);
                if (Math.Abs(ds) > 0.9)
                {
                    PanZoomed?.Invoke(_centerPoint, new Vector2(dx, dy), ds);
                }
            }

            PointerMoved?.Invoke(pointer);
        }

        /// <summary>
        /// method to call to forcibly forget all current canvas pointers
        /// </summary>
        public void ClearAllPointers()
        {
            Debug.Assert(_canvas != null && _pointers != null);
            _canvas?.ReleasePointerCaptures();
            _pointers?.Clear();
        }

        private InteractionType PointerToInteractionType(Pointer pointer)
        {
            if (pointer.PointerDeviceType == PointerDeviceType.Pen)
            {
                return InteractionType.Pen;
            }
            if (pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                return InteractionType.Mouse;
            }
            return InteractionType.Touch;
        }
    }
}