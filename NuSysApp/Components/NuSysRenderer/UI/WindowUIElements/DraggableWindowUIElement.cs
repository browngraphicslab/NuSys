using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class DraggableWindowUIElement : WindowUIElement
    {
        /// <summary>
        /// Set this to true to support Dragging the DraggableWindowUIElement around the screen using the top bar.
        /// </summary>
        public bool IsDraggable;

        /// <summary>
        /// The initial point of the window when the drag event starts, new positions are calculated as the delta from this point
        /// </summary>
        private Vector2 _initialDragPosition;

        public DraggableWindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            IsDraggable = UIDefaults.WindowIsDraggable;
            TopBarDragged += OnTopBarDragged;
            TopBarDragStarted += OnTopBarDragStarted;
            TopBarDragCompleted += OnTopBarDragCompleted;
        }

        /// <summary>
        /// Called whenever the top bar is dragged and has stopped dragging
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        protected virtual void OnTopBarDragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer){}

        protected virtual void OnTopBarDragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _initialDragPosition = Transform.LocalPosition;
        }

        protected virtual void OnTopBarDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            //// if we are not currently dragging then set dragging to IsDraggable
            //if (!_dragging)
            //{
            //    _dragging = IsDraggable;
            //}
            //else
            //{
            //    // if we are dragging then move the window around the screen
            //    Transform.LocalPosition += pointer.DeltaSinceLastUpdate;
            //}
            ApplyDragDelta(pointer.Delta);
        }

        /// <summary>
        /// protected method to apply a drag delta to the draggable window.
        /// Should take care of checking if isDraggable.
        /// </summary>
        /// <param name="delta"></param>
        protected void ApplyDragDelta(Vector2 delta)
        {

            if (IsDraggable)
            {
                Transform.LocalPosition = _initialDragPosition + delta;
            }
        }

        /// <summary>
        /// Called when the DraggableWindowUIElement is disposed.
        /// Remove event handlers here
        /// </summary>
        public override void Dispose()
        {
            TopBarDragged -= OnTopBarDragged;
            TopBarDragStarted -= OnTopBarDragStarted;
            TopBarDragCompleted -= OnTopBarDragCompleted;
            base.Dispose();
        }
    }
}
