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

        /// <summary>
        /// The buffer we use to stop users from dragging windows off the screen
        /// </summary>
        private const float WindowDragBuffer = 100;

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
            // shift the window
            ApplyDragDelta(pointer.Delta);
        }

        /// <summary>
        /// protected method to apply a drag delta to the draggable window.
        /// Should take care of checking if isDraggable.
        /// </summary>
        /// <param name="delta">pointer.Delta</param>
        protected void ApplyDragDelta(Vector2 delta)
        {
            if (IsDraggable)
            {
                Transform.LocalPosition = _initialDragPosition + delta;
            }
        }

        /// <summary>
        /// Bounds the WindowUIElement to the window
        /// </summary>
        private void BoundToWindow()
        {
            // get the upper left corner of the window on the screen
            var upperLeftCornerPositionOnScreen = Vector2.Transform(new Vector2(0, 0), Transform.LocalToScreenMatrix);

            // bound the window so that its top bar is always at the top of the screen
            if (upperLeftCornerPositionOnScreen.Y < 0)
            {
                var vectordiff = Vector2.Transform(new Vector2(0, 0), Transform.ScreenToLocalMatrix).Y;
                Transform.LocalPosition = new Vector2(Transform.LocalX, Transform.LocalY + vectordiff);
            }

            if (upperLeftCornerPositionOnScreen.Y > SessionController.Instance.ScreenHeight - WindowDragBuffer)
            {
                var vectordiff =
                    Vector2.Transform(new Vector2(0, (float) SessionController.Instance.ScreenHeight - WindowDragBuffer),
                        Transform.ScreenToLocalMatrix).Y;
                Transform.LocalPosition = new Vector2(Transform.LocalX, Transform.LocalY + vectordiff);
            }

            // bound the window so that its upper left corner is never beyond the width of the screen
            if (upperLeftCornerPositionOnScreen.X > SessionController.Instance.ScreenWidth - WindowDragBuffer)
            {
                var vectordiff =
                    Vector2.Transform(
                        new Vector2((float) (SessionController.Instance.ScreenWidth - WindowDragBuffer), 0),
                        Transform.ScreenToLocalMatrix).X;
                Transform.LocalPosition = new Vector2(Transform.LocalX + vectordiff, Transform.LocalY);
            }

            // get the upper right corner of the window on the screen
            var upperRightCornerPositionOnScreen = Vector2.Transform(new Vector2(Width, 0), Transform.LocalToScreenMatrix);

            // bound the window so that its upper right corner is never beyond the left side of the screen
            if (upperRightCornerPositionOnScreen.X < WindowDragBuffer)
            {
                var vectordiff = Vector2.Transform(new Vector2(WindowDragBuffer, 0), Transform.ScreenToLocalMatrix).X - Width;
                Transform.LocalPosition = new Vector2(Transform.LocalX + vectordiff, Transform.LocalY);
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

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            BoundToWindow();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
