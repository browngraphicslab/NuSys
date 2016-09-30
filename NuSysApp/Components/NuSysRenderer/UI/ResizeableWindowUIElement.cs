using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;

namespace NuSysApp.Components.NuSysRenderer.UI
{
    class ResizeableWindowUIElement : WindowUIElement
    {
        /// <summary>
        /// The maximum width of the resizable window
        /// </summary>
        private float? _maxWidth;

        /// <summary>
        /// True if the window is resizeable false otherwise. Checked in the GetResizerBorderPosition
        /// method
        /// </summary>
        public bool IsResizeable;

        /// <summary>
        /// The maximum width of the resizable window. Must be a value
        /// Greater than or equal to MinWidth. Must be a value greater than
        /// or equal to zero. Is float.MaxValue if set to null, otherwise
        /// whatever the user sets it to.
        /// </summary>
        public float? MaxWidth
        {
            get
            {
                return _maxWidth ?? float.MaxValue;
            }
            set
            {
                Debug.Assert(value == null || value >= MinWidth);
                Debug.Assert(value == null || value >= 0);
                _maxWidth = value >= 0 ? value : 0;
            }
        }

        /// <summary>
        /// The minimum width of the resizable window
        /// </summary>
        private float? _minWidth;

        /// <summary>
        /// The minimum width of the resizable window. Must be a value
        /// less than or equal to MaximumWidth. Must be a value greater than
        /// or equal to BorderWidth. Is BorderWidth if set to null, otherwise
        /// whatever the user sets it to.
        /// </summary>
        public float? MinWidth
        {
            get
            {
                return _minWidth ?? BorderWidth;
            }
            set
            {
                Debug.Assert(value == null || value <= MaxWidth);
                Debug.Assert(value == null || value >= 0);
                _minWidth = value >= BorderWidth ? value : BorderWidth;
            }
        }

        /// <summary>
        /// The maximum height of the resizable window
        /// </summary>
        private float? _maxHeight;

        /// <summary>
        /// The maximum height of the resizable window. Must be a value
        /// Greater than or equal to MinHeight. Must be a value greater than
        /// or equal to zero. Is float.MaxValue if set to null, otherwise
        /// whatever the user sets it to.
        /// </summary>
        public float? MaxHeight
        {
            // the getter returns the maximum float value if MaxHeight has been set to null
            get
            {
                return _maxHeight ?? float.MaxValue;
            }
            set
            {
                Debug.Assert(value == null || value >= MinHeight);
                Debug.Assert(value == null || value >= 0);
                _maxHeight = value >= 0 ? value : 0;
            }
        }

        /// <summary>
        /// The minimum height of the resizable window
        /// </summary>
        private float? _minHeight;

        /// <summary>
        /// The minimum height of the resizable window. Must be a value
        /// less than or equal to MaxHeight. Must be a value greater than
        /// or equal to the maximum of  BorderWidth and TopBarHeight. 
        /// Is the maximum of BorderWidth and TopBarHeight if set to null, otherwise
        /// whatever the user sets it to.
        /// </summary>
        public float? MinHeight
        {
            // the getter returns 0 if the getter has been set to null
            get
            {
                return _minHeight ?? Math.Max(BorderWidth, TopBarHeight);
            }
            set
            {
                Debug.Assert(value == null || value <= MaxHeight);
                Debug.Assert(value == null || value >= 0);
                _minHeight = value >= Math.Max(BorderWidth, TopBarHeight) ? value : Math.Max(BorderWidth, TopBarHeight);
            }
        }

        /// <summary>
        /// The width of the rectangle
        /// </summary>
        private float _width;

        /// <summary>
        /// The width of the rectangle.
        /// Must be greater than or equal to MinWidth. Must be less than or
        /// equal to MaxWidth.
        /// </summary>
        public override float Width
        {
            get { return _width; }
            set
            {
                Debug.Assert(MaxWidth != null, "Make sure MaxWidth never returns null");
                Debug.Assert(MinWidth != null, "Make sure MinWidth never returns null");
                if (value >= MaxWidth)
                {
                    value = MaxWidth.Value;
                } else if (value <= MinWidth)
                {
                    value = MinWidth.Value;
                }

                _width = value;

            }
        }

        /// <summary>
        /// The height of the rectangle
        /// </summary>
        private float _height;
        /// <summary>
        /// The width of the rectangle.
        /// Must be greater than or equal to MinHeight. Must be less than or
        /// equal to MaxHeight.
        /// </summary>
        public override float Height
        {
            get { return _height; }
            set
            {
                Debug.Assert(MaxHeight != null, "Make sure MaxHeight never returns null");
                Debug.Assert(MinHeight != null, "Make sure MinHeight never returns null");
                if (value >= MaxHeight)
                {
                    value = MaxHeight.Value;
                }
                else if (value <= MinHeight)
                {
                    value = MinHeight.Value;
                }

                _height = value;
            }
        }

        /// <summary>
        /// A margin of error extending beyond the width and height of the window
        /// to provide a bufferzone for the resizers. Any touch in the border or
        /// in the ErrorMargin pixels extending past the border, will trigger resizing.
        /// </summary>
        public float ErrorMargin;

        /// <summary>
        /// An enum describing the possible ResizerBorderPostions on a window
        /// </summary>
        public enum ResizerBorderPosition { Left, Right, Bottom, BottomRight, BottomLeft }

        /// <summary>
        /// The current drag position we are in.
        /// </summary>
        private ResizerBorderPosition? _resizePosition;

        public ResizeableWindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            IsResizeable = true;
        }

        /// <summary>
        /// The initializer method of the ResizeableWindowUIElement. Add events here if possible.
        /// </summary>
        /// <returns></returns>
        public override Task Load()
        {
            // add manipulation events
            Dragged += ResizeableWindowUIElement_Dragged;
            Pressed += ResizeableWindowUIElement_Pressed;

            return base.Load();
        }

        /// <summary>
        /// Fired when a pointer is pressed on the ResizeableWindowUIElement.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ResizeableWindowUIElement_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // set the _resizePosition if the pointer is on a resizer
            _resizePosition = GetResizerBorderPosition(pointer);
        }

        /// <summary>
        /// The dispose method of the ResizeableWindowUIElement. Remove all events here.
        /// </summary>
        public override void Dispose()
        {
            Dragged -= ResizeableWindowUIElement_Dragged;
            Pressed -= ResizeableWindowUIElement_Pressed;

            base.Dispose();
        }


        /// <summary>
        /// Fired when a pointer that was initially placed on the ResizeableWindowUIElement is dragged.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ResizeableWindowUIElement_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // create variables for storing the calculated change in size and offset created by resizing.
            Vector2 sizeDelta = new Vector2();
            Vector2 offsetDelta = new Vector2();

            // calculate change in size and offset based on the resizer that is being dragged
            switch (_resizePosition)
            {
                // in this case we are changing the size and the offset. Size decreases by drag x amount, offset increases
                // by drag x amount
                case ResizerBorderPosition.Left:
                    sizeDelta.X -= pointer.DeltaSinceLastUpdate.X;
                    offsetDelta.X += pointer.DeltaSinceLastUpdate.X;
                    break;
                // in this case we are changing the size only. Size increases by the drag x amount
                case ResizerBorderPosition.Right:
                    sizeDelta.X += pointer.DeltaSinceLastUpdate.X;
                    break;
                // in this case we are changing the size only. Size increases by the drag y amount
                case ResizerBorderPosition.Bottom:
                    sizeDelta.Y += pointer.DeltaSinceLastUpdate.Y;
                    break;
                case ResizerBorderPosition.BottomRight:
                    sizeDelta.Y += pointer.DeltaSinceLastUpdate.Y;
                    sizeDelta.X += pointer.DeltaSinceLastUpdate.X;
                    break;
                case ResizerBorderPosition.BottomLeft:
                    sizeDelta.X -= pointer.DeltaSinceLastUpdate.X;
                    offsetDelta.X += pointer.DeltaSinceLastUpdate.X;
                    sizeDelta.Y += pointer.DeltaSinceLastUpdate.Y;
                    break;
                default:
                    // make sure the pointer is null here, to indicate that we are not on a resizer. 
                    //If it isn't we may not have added support for an enum value.
                    Debug.Assert(_resizePosition == null, $"We do not support {nameof(_resizePosition)} yet. Please add support or check call");
                    return;
            }
            

            // Make sure the changes in size and offset are performed on the UI thread to avoid jerkiness
            Canvas.RunOnGameLoopThreadAsync(() =>
            {
                Width += sizeDelta.X;
                Height += sizeDelta.Y;
                // check the offset otherwise resizing the window below minwidth will just move the window across the screen
                if (Width != MinWidth)
                {
                    Transform.LocalPosition += offsetDelta;
                }
            });


        }


        /// <summary>
        /// Takes in a CanvasPointer and returns the ResizerBorderPosition
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        private ResizerBorderPosition? GetResizerBorderPosition(CanvasPointer pointer)
        {

            if (!IsResizeable)
            {
                return null;
            }

            // create booleans for all the sides an initialize them to false
            bool right = false, left = false, bottom = false;

            // transform the pointers current point into the local coordinate system
            var currentPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);

            // the pointer is on the right bound of the window
            if (currentPoint.X > Width - BorderWidth && currentPoint.Y > TopBarHeight)
            {
                right = true;
            }
            // the pointer is on the bottom bound of the window
            if (currentPoint.Y > Height - BorderWidth)
            {
                bottom = true;
            }
            // the pointer is on the left bound of the window
            if (currentPoint.X < 0 + BorderWidth && currentPoint.Y > TopBarHeight)
            {
                left = true;
            }


            // these booleans should be self evident, check that we are in corners
            // if we are not in a corner but we are in a side return a side
            // if we are not in a side return null
            if (bottom && right)
            {
                return ResizerBorderPosition.BottomRight;
            }
            if (bottom && left)
            {
                return ResizerBorderPosition.BottomLeft;
            }
            if (left)
            {
                return ResizerBorderPosition.Left;
            }
            if (right)
            {
                return ResizerBorderPosition.Right;
            }
            if (bottom)
            {
                return ResizerBorderPosition.Bottom;
            }
            return null;
        }

        /// <summary>
        /// Calculates the LocalBounds of the window by returning a Rect with coordinates relative
        /// to the LocalTransform. The override here is to provide support for the resizer's ErrorMargin.
        /// </summary>
        /// <returns></returns>
        public override Rect GetLocalBounds()
        {
            return new Rect(-ErrorMargin, 0, Width + ErrorMargin * 2, Height + ErrorMargin);
        }
    }
}
