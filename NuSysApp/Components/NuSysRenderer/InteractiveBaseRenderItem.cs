using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

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

        private bool _isDragging;

        
        public delegate void PointerWheelHandler(InteractiveBaseRenderItem item, CanvasPointer pointer, float delta);

        public event PointerWheelHandler PointerWheelChanged;

        public InteractiveBaseRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            
        }
        public virtual void OnPressed(CanvasPointer pointer)
        {
            Pressed?.Invoke(this, pointer);
        }

        public virtual void OnReleased(CanvasPointer pointer)
        {
            // if we are currently dragging fire drag completed and make sure we are no longer dragging
            if (_isDragging)
            {
                OnDragCompleted(pointer);
                _isDragging = false;
            }
            
            Released?.Invoke(this, pointer);
        }

        public virtual void OnPointerWheelChanged(CanvasPointer pointer, float delta)
        {
            PointerWheelChanged?.Invoke(this, pointer, delta);
        }

        public virtual void OnDoubleTapped(CanvasPointer pointer)
        {
            DoubleTapped?.Invoke(this, pointer);
        }

        public virtual void OnTapped(CanvasPointer pointer)
        {
            Tapped?.Invoke(this, pointer);
        }

        public virtual void OnDragged(CanvasPointer pointer)
        {
            // if we are not currently dragging, call the drag started method
            if (!_isDragging)
            {
                OnDragStarted(pointer);
                _isDragging = true;
            }
            
            Dragged?.Invoke(this, pointer);
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
