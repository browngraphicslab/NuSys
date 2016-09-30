using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class DraggableWindowUIElement : WindowUIElement
    {
        /// <summary>
        /// True if the window is currently be dragged false otherwise. Set in the DraggableWindowUIElement_Pressed method.
        /// </summary>
        private bool _dragging;

        /// <summary>
        /// Set this to true to support Dragging the DraggableWindowUIElement around the screen using the top bar.
        /// </summary>
        public bool IsDraggable;

        public DraggableWindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }

        /// <summary>
        /// Called when the DraggableWindowUIElement is initialized.
        /// Add event handlers here.
        /// </summary>
        /// <returns></returns>
        public override Task Load()
        {
            Dragged += DraggableWindowUIElement_Dragged;
            Pressed += DraggableWindowUIElement_Pressed;
            return base.Load();
        }

        /// <summary>
        /// Called when the DraggableWindowUIElement is disposed.
        /// Remove event handlers here
        /// </summary>
        public override void Dispose()
        {
            Dragged -= DraggableWindowUIElement_Dragged;
            Pressed -= DraggableWindowUIElement_Pressed;
            base.Dispose();
        }

        /// <summary>
        /// Fired when the pointer is pressed on the DraggableWindowUIElement
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void DraggableWindowUIElement_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // transform the pointers current point into the local coordinate system
            var currentPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);

            // if the Window supports dragging and the pointer has been pressed on the top bar, set _dragging to true
            if (IsDraggable && currentPoint.Y <= TopBarHeight)
            {
                _dragging = true;
            }
            else
            {
                _dragging = false;
            }
        }

        /// <summary>
        /// Fired when the pointer is dragged on the DraggableWindowUIElement
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void DraggableWindowUIElement_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // if we are currently dragging then move the window around the screen
            if (_dragging)
            {
                Transform.LocalPosition += pointer.DeltaSinceLastUpdate;
            }
        }
    }
}
