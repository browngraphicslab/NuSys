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

namespace NuSysApp
{
    public class ResizeableWindowUIElement : DraggableWindowUIElement
    {

        public static ResizeableWindowUIElement CurrentlyDraggingWindow { get; private set; }

        

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
        /// True if the window should maintain the aspect ratio of its width and height when resizing
        /// false otherwise.
        /// </summary>
        public bool KeepAspectRatio;

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
        /// An enum describing the possible ResizerBorderPostions on a window
        /// </summary>
        public enum ResizerBorderPosition { Left, Right, Bottom, BottomRight, BottomLeft }

        /// <summary>
        /// The current drag position we are in.
        /// </summary>
        private ResizerBorderPosition? _resizePosition;

        /// <summary>
        /// private helper for public property ResizeHighlightColor
        /// </summary>
        private Color _resizeHighlightColor { get; set; }

        /// <summary>
        /// Color used to display resize highlighting
        /// </summary>
        public Color ResizeHighlightColor
        {
            get { return _resizeHighlightColor; }
            set
            {
                _resizeHighlightColor = value;
                if (_leftResizeHighlight != null && _rightResizeHighlight != null &&
                    _bottomResizeHighlight != null)
                {
                    _leftResizeHighlight.Background = ResizeHighlightColor;
                    _rightResizeHighlight.Background = ResizeHighlightColor;
                    _bottomResizeHighlight.Background = ResizeHighlightColor;
                }

            }
        }

        /// <summary>
        /// Rectangle used to display the left resize highlight
        /// </summary>
        private GradientBackgroundRectangleUIElement _leftResizeHighlight;

        /// <summary>
        /// Rectangle used to display the right resize highlight
        /// </summary>
        private GradientBackgroundRectangleUIElement _rightResizeHighlight;

        /// <summary>
        /// Rectangle used to display the bottom resize highlight
        /// </summary>
        private GradientBackgroundRectangleUIElement _bottomResizeHighlight;

        /// <summary>
        /// Rectagngle used to display the bottom left resize highlight
        /// </summary>
        private GradientBackgroundRectangleUIElement _bottomLeftResizeHighlight;

        /// <summary>
        /// Rectangle used to display the bottom right resize highlight
        /// </summary>
        private GradientBackgroundRectangleUIElement _bottomRightResizeHighlight;


        public ResizeableWindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
            : base(parent, resourceCreator)
        {
            IsResizeable = UIDefaults.WindowIsResizeable;
            KeepAspectRatio = UIDefaults.WindowKeepsAspectRatio;
            MaxWidth = UIDefaults.WindowMaxWidth;
            MaxHeight = UIDefaults.WindowMaxHeight;
            MinWidth = UIDefaults.WindowMinWidth;
            MinHeight = UIDefaults.WindowMinHeight;
            ResizeHighlightColor = UIDefaults.ResizeHighlightColor;

            _leftResizeHighlight = new GradientBackgroundRectangleUIElement(this, resourceCreator)
            {
                Background = ResizeHighlightColor,
                IsVisible = false,
                IsHitTestVisible = false,
                BackgroundGradients = CreateGradientList(false),
                Direction = GradientBackgroundRectangleUIElement.GradientDirection.RightLeft
            };
            AddChild(_leftResizeHighlight);
            _rightResizeHighlight = new GradientBackgroundRectangleUIElement(this, resourceCreator)
            {
                Background = ResizeHighlightColor,
                IsVisible = false,
                IsHitTestVisible = false,
                BackgroundGradients = CreateGradientList(false),
                Direction = GradientBackgroundRectangleUIElement.GradientDirection.LeftRight

            };
            AddChild(_rightResizeHighlight);
            _bottomResizeHighlight = new GradientBackgroundRectangleUIElement(this, resourceCreator)
            {
                Background = ResizeHighlightColor,
                IsVisible = false,
                IsHitTestVisible = false,
                BackgroundGradients = CreateGradientList(false),
                Direction = GradientBackgroundRectangleUIElement.GradientDirection.TopBottom
            };
            AddChild(_bottomResizeHighlight);
            _bottomLeftResizeHighlight = new GradientBackgroundRectangleUIElement(this, resourceCreator)
            {
                Background = ResizeHighlightColor,
                IsVisible = false,
                IsHitTestVisible = false,
                BackgroundGradients = CreateGradientList(true),
                Direction = GradientBackgroundRectangleUIElement.GradientDirection.UpperRightLowerLeft,
                //Type = GradientBackgroundRectangleUIElement.GradientType.Radial
            };
            AddChild(_bottomLeftResizeHighlight);
            _bottomRightResizeHighlight = new GradientBackgroundRectangleUIElement(this, resourceCreator)
            {
                Background = ResizeHighlightColor,
                IsVisible = false,
                IsHitTestVisible = false,
                BackgroundGradients = CreateGradientList(true),
                Direction = GradientBackgroundRectangleUIElement.GradientDirection.UpperLeftLowerRight,
                //Type = GradientBackgroundRectangleUIElement.GradientType.Radial
            };
            AddChild(_bottomRightResizeHighlight);


            // add manipulation events
            Dragged += ResizeableWindowUIElement_Dragged;
            Pressed += ResizeableWindowUIElement_Pressed;
            OnFocusGained += FocusGained;
            OnChildFocusGained += FocusGained;
            OnFocusLost += FocusLostHideHighlight;
            OnChildFocusLost += FocusLostHideHighlight;


        }

        /// <summary>
        /// Returns a list of gradients
        /// </summary>
        /// <returns></returns>
        private List<CanvasGradientStop> CreateGradientList(bool isCorner)
        {
            var gradientColor = ResizeHighlightColor;
            gradientColor.A = 100;

            return new List<CanvasGradientStop>
            {
                new CanvasGradientStop {Color = gradientColor, Position = 0},
                new CanvasGradientStop {Color = Colors.Transparent, Position = isCorner ? .7f/2: .7f}
            };
        }

        /// <summary>
        /// event fired whenever this or a child gains focus.
        /// </summary>
        /// <param name="item"></param>
        private void FocusGained(BaseRenderItem item)
        {
            ToggleResizeHighlight(true);
            if(Parent == SessionController.Instance.NuSessionView)
            {
                SessionController.Instance.NuSessionView.MakeTopWindow(this);
            }
        }

        private void FocusLostHideHighlight(BaseRenderItem item)
        {
            ToggleResizeHighlight(false);
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
            OnFocusGained -= FocusGained;
            OnChildFocusGained -= FocusGained;
            OnFocusLost -= FocusLostHideHighlight;
            OnFocusLost -= FocusLostHideHighlight;

            

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
                    sizeDelta.X = -pointer.DeltaSinceLastUpdate.X;
                    offsetDelta.X = 1;
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
                    offsetDelta.X = 1;
                    sizeDelta.Y += pointer.DeltaSinceLastUpdate.Y;
                    break;
                default:
                    // make sure the pointer is null here, to indicate that we are not on a resizer. 
                    //If it isn't we may not have added support for an enum value.
                    Debug.Assert(_resizePosition == null, $"We do not support {nameof(_resizePosition)} yet. Please add support or check call");
                    return;
            }
            ApplyResizeChanges(offsetDelta, sizeDelta);
        }

        /// <summary>
        /// private method to apply a pre-calculated offset and size delta to the window.
        /// This takes into account the aspect ratio logic for the window resizing.
        /// </summary>
        /// <param name="offsetDelta">A vector containing an amount to scale the translation in each dimention by
        /// (0 if it shouldn't be translated, 1 if it should be translated)</param>
        /// <param name="resizeDelta">A vector containing the amount to resize in each dimension</param>
        private void ApplyResizeChanges(Vector2 offsetDelta, Vector2 sizeDelta)
        {

            // get the old width and height for calculating the ratio
            var oldWidth = Width;
            var oldHeight = Height;
            var ratio = (float)oldWidth / oldHeight;

            // if we are keeping the aspect ratio do this code
            if (KeepAspectRatio)
            {
                // if we only change the x direction 
                if (Math.Abs(sizeDelta.Y) < .001)
                {
                    Width += sizeDelta.X;
                    Height = Width / ratio;
                    // Set Width again to account for the fact that Height might have been clamped
                    Width = Height * ratio;
                    // Update the position of the window depending on offsetDelta and how much the size changed
                    Transform.LocalPosition += offsetDelta * new Vector2(oldWidth - Width, oldHeight - Height);
                }
                else
                // otherwise if we change the x and y direction or just the y direction
                {
                    Height += sizeDelta.Y;
                    Width = Height * ratio;
                    // Set Height again to account for the fact that Width might have been clamped
                    Height = Width / ratio;
                    // Update the position of the window depending on offsetDelta and how much the size changed
                    Transform.LocalPosition += offsetDelta * new Vector2(oldWidth - Width, oldHeight - Height);
                }
            }
            // otherwise just use simple code
            else
            {
                Width += sizeDelta.X;
                Height += sizeDelta.Y;
                // The Width and Height properties auto clamp to their min and max
                // Check how much was actually scaled by and potentially move the window by that much depending on which ResizeBorderPosition
                Transform.LocalPosition += offsetDelta * new Vector2(oldWidth - Width, oldHeight - Height);
            }
        }

        /// <summary>D
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
            if (currentPoint.X > Width - Math.Max(BorderWidth, ErrorMargin))
            {
                right = true;
            }
            // the pointer is on the bottom bound of the window
            if (currentPoint.Y > Height - Math.Max(BorderWidth, ErrorMargin))
            {
                bottom = true;
            }
            // the pointer is on the left bound of the window
            if (currentPoint.X < 0 + Math.Max(BorderWidth, ErrorMargin))
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



        private void ToggleResizeHighlight(bool visible)
        {
            _leftResizeHighlight.IsVisible = visible;
            _rightResizeHighlight.IsVisible = visible;
            _bottomResizeHighlight.IsVisible = visible;
            _bottomLeftResizeHighlight.IsVisible = visible;
            _bottomRightResizeHighlight.IsVisible = visible;

        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _leftResizeHighlight.Transform.LocalPosition = new Vector2(-ErrorMargin, 0);
            _leftResizeHighlight.Width = ErrorMargin;
            _leftResizeHighlight.Height = Height;
            _rightResizeHighlight.Transform.LocalPosition = new Vector2(Width, 0);
            _rightResizeHighlight.Width = ErrorMargin;
            _rightResizeHighlight.Height = Height;
            _bottomResizeHighlight.Transform.LocalPosition = new Vector2(0, Height);
            _bottomResizeHighlight.Width = Width;
            _bottomResizeHighlight.Height = ErrorMargin;
            _bottomLeftResizeHighlight.Transform.LocalPosition = new Vector2(-ErrorMargin, Height);
            _bottomLeftResizeHighlight.Width = ErrorMargin;
            _bottomLeftResizeHighlight.Height = ErrorMargin;
            _bottomRightResizeHighlight.Transform.LocalPosition = new Vector2(Width, Height);
            _bottomRightResizeHighlight.Width = ErrorMargin;
            _bottomRightResizeHighlight.Height = ErrorMargin;

            // check gradient visibility 
            if ((HasFocus == true || ChildHasFocus == true) && _leftResizeHighlight.IsVisible == false)
            {
                ToggleResizeHighlight(true);
            }
            if ((HasFocus == false && ChildHasFocus == false) && _leftResizeHighlight.IsVisible == true)
            {
                ToggleResizeHighlight(false);
            }


            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// Calculates the LocalBounds of the window by returning a Rect with coordinates relative
        /// to the LocalTransform. The override here is to provide support for the ErrorMargin.
        /// </summary>
        /// <returns></returns>
        public override Rect GetLocalBounds()
        {
            return new Rect(-ErrorMargin, -ErrorMargin, Width + ErrorMargin * 2, Height + ErrorMargin * 2);
        }

        /// <summary>
        /// Method called whenever the top bar has started a drag
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        protected override void OnTopBarDragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            CurrentlyDraggingWindow = this;
            base.OnTopBarDragStarted(item, pointer);
        }


        /// <summary>
        /// event handler called whenever the base class drag event from the top bar has completed.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        protected override void OnTopBarDragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            CurrentlyDraggingWindow = null;
            base.OnTopBarDragCompleted(item, pointer);
        }

        /// <summary>
        /// Method that can be called when the top bar is pinch zoomed.
        /// This should internally resize the window.
        /// </summary>
        /// <param name="deltaTranslation"></param>
        /// <param name="deltaZoom"></param>
        public void ResizeFromPinch(Vector2 deltaTranslation, float deltaZoom)
        {
            var diff = 100*(deltaZoom - 1);
            ApplyResizeChanges(new Vector2(0,0), new Vector2(diff, diff));
        }

        /// <summary>
        /// Sets the dimensions and offset of the snap preview window
        /// </summary>
        protected override void SetSnapPreviewDimensions()
        {
            if (SnapPreviewRect == null)
            {
                return;
            }

            // set the snap preview rect dimensions
            switch (CurrentSnapPosition)
            {
                case SnapPosition.Left:
                    SnapPreviewRect.Width = Math.Min(MaxWidth ?? float.MaxValue,Math.Max(MinWidth ?? 0, (float)SessionController.Instance.ScreenWidth / 2));
                    SnapPreviewRect.Height = Math.Min(MaxHeight ?? float.MaxValue, Math.Max(MinHeight ?? 0, (float)SessionController.Instance.ScreenHeight));
                    break;
                case SnapPosition.Top:
                    SnapPreviewRect.Width = Math.Min(MaxWidth ?? float.MaxValue, Math.Max(MinWidth ?? 0, (float)SessionController.Instance.ScreenWidth));
                    SnapPreviewRect.Height = Math.Min(MaxHeight ?? float.MaxValue, Math.Max(MinHeight ?? 0, (float)SessionController.Instance.ScreenHeight));
                    break;
                case SnapPosition.Right:
                    SnapPreviewRect.Width = Math.Min(MaxWidth ?? float.MaxValue, Math.Max(MinWidth ?? 0, (float)SessionController.Instance.ScreenWidth / 2));
                    SnapPreviewRect.Height = Math.Min(MaxHeight ?? float.MaxValue, Math.Max(MinHeight ?? 0, (float)SessionController.Instance.ScreenHeight));
                    break;
                case SnapPosition.Bottom:
                    SnapPreviewRect.Width = Math.Min(MaxWidth ?? float.MaxValue, Math.Max(MinWidth ?? 0, (float)SessionController.Instance.ScreenWidth));
                    SnapPreviewRect.Height = Math.Min(MaxHeight ?? float.MaxValue, Math.Max(MinHeight ?? 0, (float)SessionController.Instance.ScreenHeight /2));
                    break;
                case SnapPosition.TopLeft:
                case SnapPosition.TopRight:
                case SnapPosition.BottomLeft:
                case SnapPosition.BottomRight:
                    SnapPreviewRect.Width = Math.Min(MaxWidth ?? float.MaxValue, Math.Max(MinWidth ?? 0, (float)SessionController.Instance.ScreenWidth / 2));
                    SnapPreviewRect.Height = Math.Min(MaxHeight ?? float.MaxValue, Math.Max(MinHeight ?? 0, (float)SessionController.Instance.ScreenHeight / 2));
                    break;

                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
