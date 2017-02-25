using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using ReverseMarkdown.Converters;

namespace NuSysApp
{
    public class CanvasInteractionManager : IDisposable
    {
        private FrameworkElement _canvas;
        private CanvasRenderEngine _renderEngine;
        private Dictionary<uint, InteractiveBaseRenderItem> _renderItems;

        public CanvasInteractionManager(CanvasRenderEngine renderEngine, FrameworkElement pointerEventSource)
        {
            _canvas = pointerEventSource;
            _canvas.PointerPressed += OnPointerPressed;
            _canvas.PointerMoved += OnPointerMoved;
            _canvas.PointerReleased += OnPointerReleased;

            _canvas.PointerCaptureLost += OnPointerExited;
            _canvas.PointerCanceled += OnPointerExited;
            _canvas.PointerExited += OnPointerExited;

            _canvas.PointerWheelChanged += OnPointerWheelChanged;

            _renderEngine = renderEngine;
            _renderItems = new Dictionary<uint, InteractiveBaseRenderItem>();
        }


        public virtual void Dispose()
        {
            _canvas.PointerPressed -= OnPointerPressed;
            _canvas.PointerMoved -= OnPointerMoved;
            _canvas.PointerReleased -= OnPointerReleased;

            _canvas.PointerCaptureLost -= OnPointerExited;
            _canvas.PointerCanceled -= OnPointerExited;
            _canvas.PointerExited -= OnPointerExited;

            _canvas.PointerWheelChanged -= OnPointerWheelChanged;
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            InteractiveBaseRenderItem e = _renderEngine.GetRenderItemAt(args.GetCurrentPoint(_canvas).Position.ToSystemVector2(),
                _renderEngine.Root) as InteractiveBaseRenderItem;
            _renderItems[args.Pointer.PointerId] = e;
            CanvasPointer p = new CanvasPointer(args.GetCurrentPoint(_canvas), _canvas, args);
            e.OnPressed(p);
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            if (!_renderItems.ContainsKey(args.Pointer.PointerId))
            {
                return;
            }

            InteractiveBaseRenderItem e = _renderItems[args.Pointer.PointerId];
            CanvasPointer p = new CanvasPointer(args.GetCurrentPoint(_canvas), _canvas, args);
            e.OnMoved(p);
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            if (!_renderItems.ContainsKey(args.Pointer.PointerId))
            {
                return;
            }

            InteractiveBaseRenderItem e = _renderItems[args.Pointer.PointerId];
            CanvasPointer p = new CanvasPointer(args.GetCurrentPoint(_canvas), _canvas, args);
            e.OnReleased(p);
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs args)
        {
            if (!_renderItems.ContainsKey(args.Pointer.PointerId))
            {
                return;
            }

            InteractiveBaseRenderItem e = _renderItems[args.Pointer.PointerId];
            CanvasPointer p = new CanvasPointer(args.GetCurrentPoint(_canvas), _canvas, args);
            e.OnReleased(p);
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            if (!_renderItems.ContainsKey(args.Pointer.PointerId))
            {
                return;
            }

            InteractiveBaseRenderItem e = _renderItems[args.Pointer.PointerId];
            CanvasPointer p = new CanvasPointer(args.GetCurrentPoint(_canvas), _canvas, args);
            e.OnPointerWheelChanged(p);
        }
    }
}