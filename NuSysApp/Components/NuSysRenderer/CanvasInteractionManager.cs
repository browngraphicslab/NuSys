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
            e.OnPressed(_canvas, args);

        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            if (!_renderItems.ContainsKey(args.Pointer.PointerId))
            {
                return;
            }

            InteractiveBaseRenderItem e = _renderItems[args.Pointer.PointerId];
            e.OnMoved(_canvas, args);
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            if (!_renderItems.ContainsKey(args.Pointer.PointerId))
            {
                return;
            }

            InteractiveBaseRenderItem e = _renderItems[args.Pointer.PointerId];
            e.OnReleased(_canvas, args);
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs args)
        {
            if (!_renderItems.ContainsKey(args.Pointer.PointerId))
            {
                return;
            }

            InteractiveBaseRenderItem e = _renderItems[args.Pointer.PointerId];
            e.OnExited(_canvas, args);
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            if (!_renderItems.ContainsKey(args.Pointer.PointerId))
            {
                return;
            }

            InteractiveBaseRenderItem e = _renderItems[args.Pointer.PointerId];
            e.OnPointerWheelChanged(_canvas, args);
        }
    }
}