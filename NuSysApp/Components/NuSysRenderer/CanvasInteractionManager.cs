using System;
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
        public delegate void TappedHandler(BaseRenderItem element, PointerRoutedEventArgs args);
        public delegate void TwoPointerPressedHandler(CanvasPointer pointer1, CanvasPointer pointer2);
        public delegate void PointerPressedHandler(CanvasPointer pointer);
        public delegate void MarkingMenuPointerReleasedHandler();
        public delegate void MarkingMenuPointerMoveHandler(Vector2 p);
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
        public event TwoPointerPressedHandler TwoPointerPressed;
        public event MarkingMenuPointerMoveHandler MarkingMenuPointerMove;

        private CanvasPointer _lastTappedPointer = new CanvasPointer();
        private Vector2 _centerPoint;
        private double _twoFingerDist;
        private List<CanvasPointer> _pointers = new List<CanvasPointer>();
        private FrameworkElement _canvas;
       private bool _cancelLongTapped;

       public List<CanvasPointer> ActiveCanvasPointers { get { return _pointers; } } 
        
        public CanvasInteractionManager(FrameworkElement canvas)
        {
            _canvas = canvas;
            _canvas.PointerPressed += OnPointerPressed;
            _canvas.PointerReleased += OnPointerReleased;
            _canvas.PointerWheelChanged += ResourceCreatorOnPointerWheelChanged;
            AllPointersReleased += OnAllPointersReleased;
        }

       private void OnAllPointersReleased()
       {
           _cancelLongTapped = false;
       }

       private void ResourceCreatorOnPointerWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            var p = args.GetCurrentPoint(null).Position;
            var newCenter = new Vector2((float)p.X, (float)p.Y);
            _centerPoint = newCenter;

        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            OnPointerTouchReleased(sender, args);
     
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            OnPointerTouchPressed(sender, args);
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {

            OnPointerTouchMoved(sender, args);
        }
        
        public void Dispose()
        {
            _canvas.PointerPressed -= OnPointerPressed;
            _canvas.PointerReleased -= OnPointerReleased;
            _canvas.PointerMoved -= OnPointerMoved;
        }

        private void UpdateCenterPoint()
        {
            if (_pointers.Count == 1)
            {
                _centerPoint = new Vector2((float)_pointers[0].CurrentPoint.X, (float)_pointers[0].CurrentPoint.Y);
            } else { 
                var p0 = _pointers[0].CurrentPoint;
                var p1 = _pointers[1].CurrentPoint;
                _centerPoint = new Vector2((float) (p0.X + p1.X)/2f, (float) (p0.Y + p1.Y)/2f);
            }
        }

        private void UpdateDist()
        {
            var points = _pointers.Select(p => p.CurrentPoint).ToArray();
            var p0 = new Vector2((float)points[0].X, (float)points[0].Y);
            var p1 = new Vector2((float)points[1].X, (float)points[1].Y);
            _twoFingerDist = MathUtil.Dist(p0, p1);
        }
               
        private async void OnPointerTouchPressed(object sender, PointerRoutedEventArgs e)
        {
            _pointers.Add(new CanvasPointer(e.GetCurrentPoint(null)));

            UpdateCenterPoint();

            PointerPressed?.Invoke(_pointers.Last());

            if (_pointers.Count == 2) {
                UpdateCenterPoint();
                UpdateDist();
               
                if (_pointers[0].MillisecondsActive > 300)
                {
                    _cancelLongTapped = true;
                    TwoPointerPressed?.Invoke(_pointers[0], _pointers[1]);
                } 
            }            

            _canvas.PointerMoved += OnPointerMoved;
        }

        private async void OnPointerTouchReleased(object sender, PointerRoutedEventArgs e)
        {
            var exisitingPointer = _pointers.Where(p => p.PointerId == e.Pointer.PointerId);
            if (!exisitingPointer.Any())
                return;

            var releasedPointer = exisitingPointer.First();
            releasedPointer.Update(e.GetCurrentPoint(null).Position);
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
            pointer.Update(args.GetCurrentPoint(null).Position);

            if (_pointers.Count == 1)
            {
                if (Math.Abs(pointer.DeltaSinceLastUpdate.X) > 0 || Math.Abs(pointer.DeltaSinceLastUpdate.Y) > 0)
                    Translated?.Invoke(pointer, pointer.CurrentPoint, pointer.DeltaSinceLastUpdate);
            }
            if (_pointers.Count == 2)
            {
                var prevCenterPoint = _centerPoint;
                var prevDist = _twoFingerDist;
                UpdateCenterPoint();
                UpdateDist();
                var dx = _centerPoint.X - prevCenterPoint.X;
                var dy = _centerPoint.Y - prevCenterPoint.Y;
                var ds = (float)(_twoFingerDist / prevDist);
                PanZoomed?.Invoke(_centerPoint, new Vector2(dx, dy), ds);
            }

            PointerMoved?.Invoke(pointer);
        }
    }
}