using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using NuSysApp.Components.NuSysRenderer.UI;

namespace NuSysApp
{
    public class SnappableWindowUIElement : ResizeableWindowUIElement
    {

        /// <summary>
        /// Enum describing the different positions the SnappableWindowUIElement
        /// can snap to. The SnappableWindowUIElement can be snapped to
        /// any of these positions using the function SnapTo
        /// </summary>
        public enum SnapPosition { Top, Left, Right, TopLeft, TopRight, BottomLeft, BottomRight, Center }

        /// <summary>
        /// The distance in pixels from the edge of the parent RectangleUIElement that the 
        /// SnappableWindowUIElement will start previewing a Snap event. Distance from the edge
        /// of the screen if parent is null.
        /// </summary>
        public float SnapMargin { get; set; }

        /// <summary>
        /// The color of the preview window used for snapping. Try to make this a see through color
        /// </summary>
        public Color PreviewColor { get; set; }

        /// <summary>
        /// True if the Window is snappable. False otherwise
        /// </summary>
        public bool IsSnappable { get; set; }

        /// <summary>
        /// The size of the _preview window, private variable stored for draw calls
        /// </summary>
        private Vector2 _previewSize;

        /// <summary>
        /// The offset of the _preview window, private variable stored for draw calls.
        /// </summary>
        private Vector2 _previewOffset;

        /// <summary>
        /// Boolean to determine whether or not the preview should be drawn
        /// </summary>
        private bool _isPreviewVisible;

        
        public SnappableWindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

        }

        /// <summary>
        /// Initializer method. Add Event handlers here
        /// </summary>
        /// <returns></returns>
        public override Task Load()
        {
            Released += SnappableWindowUIElement_Released;
            Dragged += SnappableWindowUIElement_Dragged;
            return base.Load();
        }

        /// <summary>
        /// The draw method, draws the preview onto the window
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            // call base.Draw cause the preview has to be drawn on top of all the other items
            base.Draw(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Transform.ScreenToLocalMatrix;

            // draw the preview window if it is currently visible
            if (_isPreviewVisible)
            {
                DrawPreview(ds);
            }

            ds.Transform = orgTransform;
        }

        /// <summary>
        /// Fired when a pointer initially pressed on the SnappableWindowUIElement is dragged.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void SnappableWindowUIElement_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // if we are not currently dragging the window we cannot perform a snap
            if (!_dragging || !IsSnappable)
            {
                return;
            }
            // get position of the Snap. Null if we are not in a SnapPosition.
            var position = GetSnapPosition(pointer);
            // call ShowPreview which calculates size and offset of preview based on position
            // or does nothing if position is null
            ShowPreview(position);
        }

        /// <summary>
        /// Dispose event. Remove Event handlers here.
        /// </summary>
        public override void Dispose()
        {
            Released -= SnappableWindowUIElement_Released;
            Dragged -= SnappableWindowUIElement_Dragged;
            GetParentBounds = null;
            base.Dispose();
        }

        /// <summary>
        /// Called when a pointer pressed on a SnappableWindowUIElement is released
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void SnappableWindowUIElement_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // if we are not currently dragging the window we cannot perform a snap
            if (!_dragging || !IsSnappable)
            {
                return;
            }

            // get the position of the pointer. null if we are not in a snap position
            var position = GetSnapPosition(pointer);
            // snap the window to the position. if the position was null this does nothing
            SnapTo(position);

        }


        /// <summary>
        /// Takes in a CanvasPointer and returns the SnapPosition that pointer is on, null otherwise.
        /// If you want to change what positions on the screen are considered snap positions you should override this.
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        protected virtual SnapPosition? GetSnapPosition(CanvasPointer pointer)
        {

            // create booleans for all the sides an initialize them to false
            bool right = false, left = false, top = false, bottom = false;

            
            Vector2 currentPoint;
            currentPoint = Vector2.Transform(pointer.CurrentPoint, GetParentScreenToLocalMatrix.Invoke());
            Vector4 bounds = GetParentBounds.Invoke();

            // the pointer is on the right bound
            if (currentPoint.X > bounds.Z - SnapMargin)
            {
                right = true;
            }
            // else if the pointer is on the bottom bound
            if (currentPoint.Y > bounds.W - SnapMargin)
            {
                bottom = true;
            }
            // if the pointer is on the left bound
            if (currentPoint.X < bounds.X + SnapMargin)
            {
                left = true;
            }
            // else if the pointer is on the top bound
            if (currentPoint.Y < bounds.Y + SnapMargin)
            {
                top = true;
            }

            // these booleans should be self evident, check that we are in corners
            // if we are not in a corner but we are in a side return a side
            // if we are not in a side return null
            if (top && left)
            {
                return SnapPosition.TopLeft;
            }
            if (top && right)
            {
                return SnapPosition.TopRight;
            }
            if (top)
            {
                return SnapPosition.Top;
            }
            if (bottom && right)
            {
                return SnapPosition.BottomRight;
            }
            if (bottom && left)
            {
                return SnapPosition.BottomLeft;
            }
            if (left)
            {
                return SnapPosition.Left;
            }
            if (right)
            {
                return SnapPosition.Right;
            }
            return null;
        }


        /// <summary>
        /// Snaps the SnappableWindowUIElement to the passed in SnapPosition. If
        /// you want to change where the different SnapPositions snap the window to tthen
        /// override this.
        /// </summary>
        /// <param name="position"></param>
        protected virtual void SnapTo(SnapPosition? position)
        {
            // if the passed in position is null do nothing
            if (position == null)
            {
                return;
            }

            // store the size and offset of the snap position
            Vector2 size;
            Vector2 offset;

            // calculate size and offset based on the passed in SnapPosition
            switch (position)
            {
                case SnapPosition.Top:
                    size = NormalizedToParentSize(1, 1);
                    offset = NormalizedToParentOffset(0, 0);
                    break;
                case SnapPosition.Left:
                    size = NormalizedToParentSize(.5f, 1);
                    offset = NormalizedToParentOffset(0, 0);
                    break;
                case SnapPosition.Right:
                    size = NormalizedToParentSize(.5f, 1);
                    offset = NormalizedToParentOffset(.5f, 0);
                    break;
                case SnapPosition.TopLeft:
                    size = NormalizedToParentSize(.5f, .5f);
                    offset = NormalizedToParentOffset(0, 0);
                    break;
                case SnapPosition.TopRight:
                    size = NormalizedToParentSize(.5f, .5f);
                    offset = NormalizedToParentOffset(.5f, 0);
                    break;
                case SnapPosition.BottomLeft:
                    size = NormalizedToParentSize(.5f, .5f);
                    offset = NormalizedToParentOffset(0, .5f);
                    break;
                case SnapPosition.BottomRight:
                    size = NormalizedToParentSize(.5f, .5f);
                    offset = NormalizedToParentOffset(.5f, .5f);
                    break;
                case SnapPosition.Center:
                    size = NormalizedToParentSize(.5f, .5f);
                    offset = NormalizedToParentOffset(.5f, .5f);
                    break;
                default:
                    // if we hit this, then we probably do not have support for some SnapPosition
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }

            // We have a valid SnapPosition so assign values to the Transform, Height, and Width dimensions
            // and make sure the preview is not drawn anymore
            _isPreviewVisible = false;
            Width = size.X;
            Height = size.Y;
            Transform.LocalPosition = offset;          
        }

        /// <summary>
        /// Calculates the relative offset and size of the preview rectangle in relation to the LocalTransform.
        /// Override this if you want to change the preview position.
        /// </summary>
        /// <param name="position"></param>
        protected virtual void ShowPreview(SnapPosition? position)
        {
            // return if the position is null, and set _isPreviewVisible to false since
            // we are no longer in a preview position
            if (position == null)
            {
                _isPreviewVisible = false;
                return;
            }

            // store the size and offset of the preview rectangle for use in switch statement
            Vector2 size;
            Vector2 offset;

            // calculate the offset and size of the preview rectangle based on the
            // passed in position
            switch (position)
            {
                case SnapPosition.Top:
                    size = NormalizedToParentSize(1, 1);
                    offset = InverseNormalizedPosition(0, 0);
                    break;
                case SnapPosition.Left:
                    size = NormalizedToParentSize(.5f, 1);
                    offset = InverseNormalizedPosition(0, 0);
                    break;
                case SnapPosition.Right:
                    size = NormalizedToParentSize(.5f, 1);
                    offset = InverseNormalizedPosition(.5f, 0);
                    break;
                case SnapPosition.TopLeft:
                    size = NormalizedToParentSize(.5f, .5f);
                    offset = InverseNormalizedPosition(0, 0);
                    break;
                case SnapPosition.TopRight:
                    size = NormalizedToParentSize(.5f, .5f);
                    offset = InverseNormalizedPosition(.5f, 0);
                    break;
                case SnapPosition.BottomLeft:
                    size = NormalizedToParentSize(.5f, .5f);
                    offset = InverseNormalizedPosition(0, .5f);
                    break;
                case SnapPosition.BottomRight:
                    size = NormalizedToParentSize(.5f, .5f);
                    offset = InverseNormalizedPosition(.5f, .5f);
                    break;
                case SnapPosition.Center:
                    size = NormalizedToParentSize(.5f, .5f);
                    offset = InverseNormalizedPosition(.5f, .5f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }

            // set the private variable for drawing the preview rectangle
            _previewSize = size;
            _previewOffset = offset;
            _isPreviewVisible = true;
        }

        /// <summary>
        /// Draws the preview rectangle
        /// </summary>
        /// <param name="ds"></param>
        private void DrawPreview(CanvasDrawingSession ds)
        {
            if (_isPreviewVisible)
            {
                var orgTransform = ds.Transform;
                ds.Transform = Transform.LocalToScreenMatrix;

                ds.FillRectangle(new Rect(_previewOffset.X, _previewOffset.Y, _previewSize.X, _previewSize.Y), PreviewColor);

                ds.Transform = orgTransform;
            }
        }

        /// <summary>
        /// Takes in normalized coordinates and outputs the size of the parents bounds corresponding to those coordinates
        /// for example 1, 1 produces the entire parent's bounds, while .5 .5 returns half the parents bounds.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        private Vector2 NormalizedToParentSize(float X, float Y)
        {
            Debug.Assert(X <= 1 && X >= 0);
            Debug.Assert(Y <= 1 && Y >= 0);

            var bounds = GetParentBounds.Invoke();

            var boundsWidth = bounds.Z - bounds.X;
            var boundsHeight = bounds.W - bounds.Y;

            var output = new Vector2(boundsWidth, boundsHeight);

            output.X *= X;
            output.Y *= Y;

            return output;
        }

        /// <summary>
        /// Takes in normalized coordinates and returns the offset necessary to move the local transform to
        /// those pixels in the parents bounds. For example .5, .5 will return an offset that would move
        /// the local transform to the center of the paren'ts bounds
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        private Vector2 NormalizedToParentOffset(float X, float Y)
        {
            Debug.Assert(X <= 1 && X >= 0);
            Debug.Assert(Y <= 1 && Y >= 0);

            var bounds = GetParentBounds.Invoke();

            var boundsWidth = bounds.Z - bounds.X;
            var boundsHeight = bounds.W - bounds.Y;

            var output = new Vector2(boundsWidth, boundsHeight);

            output.X *= X;
            output.Y *= Y;

            output.X += bounds.X;
            output.Y += bounds.Y;
            return output;
        }

        /// <summary>
        /// Takes in normalized coordinates and returns the offset necessary to draw an item at that point
        /// on the parent's bounds without moving the local transform.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        private Vector2 InverseNormalizedPosition(float X, float Y)
        {
            Vector2 NormalizedParentOffset = NormalizedToParentOffset(X, Y);
            return NormalizedParentOffset - Transform.LocalPosition;
        }
    }
}
