using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

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
            PointerPressed += OnPointerPressed;
            Translated += OnTranslated;
            ItemTapped += OnItemTapped;
            ItemDoubleTapped += OnItemDoubleTapped;
            PointerReleased += OnPointerReleased;
            AllPointersReleased += OnAllPointersReleased;
            PointerWheelChanged += OnPointerWheelChanged;
        }

        private void OnPointerWheelChanged(CanvasPointer pointer, float delta)
        {
            _hit = _renderEngine.GetRenderItemAt(pointer.CurrentPoint, _renderEngine.Root) as InteractiveBaseRenderItem;
            _hit?.OnPointerWheelChanged(pointer, delta);
        }

        private void OnPointerReleased(CanvasPointer pointer)
        {
            //_hit = _renderEngine.GetRenderItemAt(pointer.CurrentPoint, _renderEngine.Root) as InteractiveBaseRenderItem;
            _hit?.OnReleased(pointer);
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
            _hit = _renderEngine.GetRenderItemAt(pointer.CurrentPoint, _renderEngine.Root) as InteractiveBaseRenderItem;
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
            ItemTapped -= OnItemTapped;
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
