﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas.UI.Xaml;
using SharpDX.Direct2D1;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{

   public class CanvasInteractionManager : IDisposable
    {
        public enum InteractionType
        {
            Pen,
            /// <summary>
            /// Right now, touch includes pointer/mouse
            /// </summary>
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
        /// The passed InteractionType is the latest interation type 
        /// </summary>
        public event EventHandler<InteractionType> InteractionTypeChanged;

        private CanvasPointer _lastTappedPointer = new CanvasPointer();
        private Vector2 _centerPoint;
        private double _twoFingerDist;
        private double _twoFingerDistTarget;
        private List<CanvasPointer> _pointers = new List<CanvasPointer>();
        private FrameworkElement _canvas;
        private bool _cancelLongTapped;
        private bool _isEnabled = true;
        private InteractionType _lastInteractionType = InteractionType.Touch;

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

        private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            if (_lastInteractionType != PointerToInteractionType(args.Pointer))
            {
                _lastInteractionType = PointerToInteractionType(args.Pointer);
                InteractionTypeChanged?.Invoke(this, _lastInteractionType);
            }
            if (!_isEnabled)
                return;
            OnPointerTouchPressed(sender, args);
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
                _centerPoint = new Vector2((float)_pointers[0].CurrentPoint.X, (float)_pointers[0].CurrentPoint.Y);
            } else {
                var p0 = _pointers[0].GetBufferMean();
                var p1 = _pointers[1].GetBufferMean();
                _centerPoint = new Vector2((float) (p0.X + p1.X)/2f, (float) (p0.Y + p1.Y)/2f);
            }
        }

        private void UpdateDist()
        {
            var points = _pointers.Select(p => p.CurrentPoint).ToArray();
            var p0 = _pointers[0].GetBufferMean();
            var p1 = _pointers[1].GetBufferMean();
            _twoFingerDist = MathUtil.Dist(p0, p1);
        }
               
        private async void OnPointerTouchPressed(object sender, PointerRoutedEventArgs e)
        {
            _canvas.CapturePointer(e.Pointer);

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
            unsafe
            {
                var exisitingPointer = _pointers.Where(p => p.PointerId == args.Pointer.PointerId);
                if (!exisitingPointer.Any())
                    return;

                var pointer = exisitingPointer.First();
                pointer.Update(args.GetCurrentPoint(_canvas));

                if (_pointers.Count == 1)
                {
                    if (Math.Abs(pointer.DeltaSinceLastUpdate.X) > 0 || Math.Abs(pointer.DeltaSinceLastUpdate.Y) > 0)
                        Translated?.Invoke(pointer, pointer.CurrentPoint, pointer.DeltaSinceLastUpdate);
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
                        PanZoomed?.Invoke(_centerPoint, new Vector2(dx, dy), ds);
                }

                PointerMoved?.Invoke(pointer);
            }
           
        }

        private InteractionType PointerToInteractionType(Pointer pointer)
        {
            if (pointer.PointerDeviceType == PointerDeviceType.Pen)
            {
                return InteractionType.Pen;
            }
            return InteractionType.Touch;
        }
    }
}