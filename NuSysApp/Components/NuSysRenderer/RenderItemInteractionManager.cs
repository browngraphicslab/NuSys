using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class RenderItemInteractionManager : CanvasInteractionManager
    {
        private CanvasRenderEngine _renderEngine;
        private InteractiveBaseRenderItem _hit;
        private bool _isPressed;

        public RenderItemInteractionManager(CanvasRenderEngine renderEngine, FrameworkElement pointerEventSource) : base(pointerEventSource)
        {
            _renderEngine = renderEngine;
            Root = renderEngine.Root;
            PointerPressed += OnPointerPressed;
            Translated += OnTranslated;
            ItemTapped += OnItemTapped;
            ItemDoubleTapped += OnItemDoubleTapped;
            PointerReleased += OnPointerReleased;
            AllPointersReleased += OnAllPointersReleased;
            PointerWheelChanged += OnPointerWheelChanged;
            Holding += OnHolding;

            
        }

        private void OnHolding(Vector2 point)
        {
            _hit = _renderEngine.GetRenderItemAt(point, _renderEngine.Root) as InteractiveBaseRenderItem;
            _hit?.OnHolding(point);
        }

        private void OnPointerWheelChanged(CanvasPointer pointer, float delta)
        {
            _hit = _renderEngine.GetRenderItemAt(pointer.CurrentPoint, _renderEngine.Root) as InteractiveBaseRenderItem;
            _hit?.OnPointerWheelChanged(pointer, delta);
        }

        public static void SetDrag(Func<CanvasPointer, BaseRenderItem, bool> func, Uri drag)
        {
            Debug.Assert(func != null && drag != null);
            if (DragElement == null && DropFunc == null)
            {
                Task.Run(async delegate {
                    DragElement = await MediaUtil.LoadCanvasBitmapAsync(Root.ResourceCreator, drag);
                });
                DropFunc = func;
            }
        }
        public static Func<CanvasPointer, BaseRenderItem, bool> DropFunc { get; private set; } = null;
        public static ICanvasImage DragElement { get; private set; } = null;
        public static Point DragPoint;
        public static BaseRenderItem Root { get; private set; }
        private void OnPointerReleased(CanvasPointer pointer)
        {
            _hit?.OnReleased(pointer);
            if (DropFunc != null)
            {
                var droppedOnto = _renderEngine.GetRenderItemAt(pointer.CurrentPoint, _renderEngine.Root, int.MaxValue, _hit);
                DragElement = null;
                var f = DropFunc;
                DropFunc = null;
                f(pointer, droppedOnto);
                DropFunc = null;
                f = null;
            }
        }

        private void OnAllPointersReleased()
        {
            _hit = null;
            _isPressed = false;
        }

        private void OnPointerPressed(CanvasPointer pointer)
        {
            if (_isPressed)
                return;
            _hit = _renderEngine.GetRenderItemAt(pointer.CurrentPoint, _renderEngine.Root, 500 ) as InteractiveBaseRenderItem;
            _isPressed = true;
            _hit?.OnPressed(pointer);
        }

        private void OnTranslated(CanvasPointer pointer, Vector2 point, Vector2 delta)
        {
            if (_isPressed && _hit != null)
            {
                _hit.OnDragged(pointer);
            }
        }

        public override void Dispose()
        {
            PointerPressed -= OnPointerPressed;
            Translated -= OnTranslated;
            ItemTapped -= OnItemTapped;
            ItemDoubleTapped -= OnItemDoubleTapped;
            PointerReleased -= OnPointerReleased;
            AllPointersReleased -= OnAllPointersReleased;
            PointerWheelChanged -= OnPointerWheelChanged;
            Holding -= OnHolding;
            _renderEngine = null;
            base.Dispose();
        }

        private void OnItemTapped(CanvasPointer pointer)
        {
            var hit = _renderEngine.GetRenderItemAt(pointer.CurrentPoint, _renderEngine.Root) as InteractiveBaseRenderItem;
            if (hit != null)
            {
                hit.OnTapped(pointer);
            }
        }

        private void OnItemDoubleTapped(CanvasPointer pointer)
        {
            var hit = _renderEngine.GetRenderItemAt(pointer.CurrentPoint, _renderEngine.Root) as InteractiveBaseRenderItem;
            if (hit != null)
            {
                hit.OnDoubleTapped(pointer);
            }
        }
    }
}
