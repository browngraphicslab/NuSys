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
        /// True if the window is currently be dragged false otherwise. Set in the DraggableWindowUIElement_Pressed method.
        /// </summary>
        protected bool _dragging { get; private set; }

        /// <summary>
        /// Set this to true to support Dragging the DraggableWindowUIElement around the screen using the top bar.
        /// </summary>
        public bool IsDraggable;

        public DraggableWindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            IsDraggable = UIDefaults.WindowIsDraggable;
            OnTopBarDragged += DraggableWindowUIElement_OnTopBarDragged;
        }

        private void DraggableWindowUIElement_OnTopBarDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // if we are not currently dragging then set dragging to IsDraggable
            if (!_dragging)
            {
                _dragging = IsDraggable;
            }
            else
            {
                // if we are dragging then move the window around the screen
                Transform.LocalPosition += pointer.DeltaSinceLastUpdate;
            }
        }

        /// <summary>
        /// Called when the DraggableWindowUIElement is disposed.
        /// Remove event handlers here
        /// </summary>
        public override void Dispose()
        {
            OnTopBarDragged -= DraggableWindowUIElement_OnTopBarDragged;
            base.Dispose();
        }
    }
}
