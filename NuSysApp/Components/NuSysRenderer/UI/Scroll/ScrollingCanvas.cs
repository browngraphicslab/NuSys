using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class ScrollingCanvas : RectangleUIElement
    {
        /// <summary>
        /// Invoked when an element on the scrolling canvas is pressed
        /// </summary>
        public event PointerHandler ElementPressed;

        /// <summary>
        /// Invoked when an element on the scrolling canvas is released
        /// </summary>
        public event PointerHandler ElementReleased;

        /// <summary>
        /// Invoked when an element on the scrolling canvas is double tapped
        /// </summary>
        public event PointerHandler ElementDoubleTapped;

        /// <summary>
        /// Invoked when an element on the scrolling canvas is tapped
        /// </summary>
        public event PointerHandler ElementTapped;

        /// <summary>
        /// Invoked when an element on the scrolling canvas is dragged
        /// </summary>
        public event PointerHandler ElementDragged;

        /// <summary>
        /// Invoked when an element on the scrolling canvas drag event first starts
        /// </summary>
        public event PointerHandler ElementDragStarted;

        /// <summary>
        /// Invoked when an element on the scrolling canvas drag event completes
        /// </summary>
        public event PointerHandler ElementDragCompleted;

        /// <summary>
        /// Invoked when an element on the scrolling canvas has a pointer wheel changed
        /// </summary>
        public event PointerWheelHandler ElementPointWheelChanged;

        /// <summary>
        /// the horizontal scrollbar
        /// </summary>
        protected ScrollBarUIElement HorizontalScrollBar;

        /// <summary>
        /// the vertical scrollbar
        /// </summary>
        protected ScrollBarUIElement VerticalScrollBar;

        /// <summary>
        /// A list of all the elements that are contained in the scroll area
        /// </summary>
        private List<BaseInteractiveUIElement> _elements;

        /// <summary>
        /// The list of all the baserenderitems that have been added to the scroll area
        /// </summary>
        public List<BaseInteractiveUIElement> Elements => _elements;

        /// <summary>
        /// private helper variable for public field ScrollArea
        /// </summary>
        private Size _scrollAreaSize { get; set; } = new Size(UIDefaults.Width, UIDefaults.Height);

        /// <summary>
        /// the size of the scroll area
        /// </summary>
        public Size ScrollAreaSize
        {
            get { return _scrollAreaSize; }
            set
            {
                _scrollAreaSize = value;
                ComputeCrop();
                ComputeScrollHandleSize();
            }
        }

        /// <summary>
        /// The possible orientations of the scrolling
        /// </summary>
        public enum ScrollOrientation { Vertical, Horizontal, Both, Auto}

        /// <summary>
        /// The direction of the scrollbar. Vertical means that the vertical scroll bar is always visible, Horizontal means that the
        /// horizontal scroll bar is always visible, Both means that both scroll bars are always visible, Auto means that scroll bar visibility
        /// changes based on the needs of the user
        /// </summary>
        public ScrollOrientation ScrollDirection { get; }

        /// <summary>
        /// The possible places we can scroll to
        /// </summary>
        public enum ScrollTo { Left, Top, Right, Bottom}

        /// <summary>
        /// The actual scroll area used to add and remove elements from
        /// </summary>
        private MaskedRectangleUIElement _scrollAreaRect;

        /// <summary>
        /// The list of elements on the scrollarea that are currently visible
        /// </summary>
        private List<BaseInteractiveUIElement> _visibleElements;

        /// <summary>
        /// The crop rect defines the area within the ScrollingArea that is actually displayed to the user
        /// </summary>
        public Rect _cropRect { get; set; }

        /// <summary>
        /// The height of the scrolling canvas
        /// </summary>
        public override float Height
        {
            get { return base.Height; }
            set
            {
                base.Height = value;
                CheckSizeChanged();
            }
        }

        private float _previousHeight { get; set; }

        private float _previousWidth { get; set; }

        private void CheckSizeChanged()
        {
            if (Math.Abs(Height - _previousHeight) > .01)
            {
                _previousHeight = Height;
                OnSizeChanged();
            }

            if (Math.Abs(Width - _previousWidth) > .01)
            {
                _previousWidth = Width;
                OnSizeChanged();
            }
        }

        /// <summary>
        /// The width of the scrolling canvas
        /// </summary>
        public override float Width
        {
            get { return base.Width; }
            set
            {
                base.Width = value;
                CheckSizeChanged();
            }
        }

        /// <summary>
        /// Rectangle in the lower right corner of the scrolling canvas
        /// </summary>
        private RectangleUIElement _lowerRightCornerRect;

        /// <summary>
        /// The initial drag position when the user starts dragging the scrolling canvas
        /// </summary>
        private Vector2 _initialDragPosition;


        /// <summary>
        /// The background of the scrolling canvas
        /// </summary>
        public override Color Background
        {
            get { return _scrollAreaRect?.Background ?? base.Background; }
            set
            {
                if (_scrollAreaRect == null)
                {
                    base.Background = value;
                }
                else
                {
                    _scrollAreaRect.Background = value;
                }
            }
        }

        /// <summary>
        /// The width of the vertical scroll bar
        /// </summary>
        public float VerticalScrollBarWidth => VerticalScrollBar.Width;

        /// <summary>
        /// the height of the vertical scroll bar
        /// </summary>
        public float VerticalScrollBarHeight => VerticalScrollBar.Height;

        /// <summary>
        ///  the width of the horizontal scroll bar
        /// </summary>
        public float HorizontalScrollBarWidth => HorizontalScrollBar.Width;

        /// <summary>
        /// The height of the horizontal scrol bar
        /// </summary>
        public float HorizontalScrollBarHeight => HorizontalScrollBar.Height;

        public ScrollingCanvas(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, ScrollOrientation scrollDirection) : base(parent, resourceCreator)
        {
            // set the scroll direction based on the passed in scroll direction
            ScrollDirection = scrollDirection;

            // add the vertical and horizontal scrollbars
            VerticalScrollBar = new ScrollBarUIElement(this, Canvas, ScrollBarUIElement.Orientation.Vertical)
            {
                Width = UIDefaults.ScrollBarWidth
            };
            AddChild(VerticalScrollBar);
            HorizontalScrollBar = new ScrollBarUIElement(this, Canvas, ScrollBarUIElement.Orientation.Horizontal)
            {
                Height = UIDefaults.ScrollBarWidth
            };
            AddChild(HorizontalScrollBar);

            _scrollAreaRect = new MaskedRectangleUIElement(this, resourceCreator);
            AddChild(_scrollAreaRect);
            _lowerRightCornerRect = new RectangleUIElement(this, resourceCreator)
            {
                Background = Colors.Gray,
                Width = UIDefaults.ScrollBarWidth,
                Height = UIDefaults.ScrollBarWidth
            };
            AddChild(_lowerRightCornerRect);

            // initialize the elements list
            _elements = new List<BaseInteractiveUIElement>();
            _visibleElements = new List<BaseInteractiveUIElement>();

            OnSizeChanged();

            HorizontalScrollBar.ScrollBarPositionChanged += _horizontalScrollBar_ScrollBarPositionChanged;
            VerticalScrollBar.ScrollBarPositionChanged += _verticalScrollBar_ScrollBarPositionChanged;
            _scrollAreaRect.PointerWheelChanged += ScrollingCanvas_PointerWheelChanged;
            _scrollAreaRect.DragStarted += ScrollingCanvas_DragStarted;
            _scrollAreaRect.Dragged += ScrollingCanvas_Dragged;
        }

        private void _verticalScrollBar_ScrollBarPositionChanged(object source, float position)
        {
            IsDirty = true;
        }

        private void _horizontalScrollBar_ScrollBarPositionChanged(object source, float position)
        {
            IsDirty = true;
        }

        /// <summary>
        /// Adds an element to the ScrollArea at the specified position, the position does not include the border
        /// width of the scrollingcanvas
        /// </summary>
        /// <param name="element"></param>
        public void AddElement(BaseInteractiveUIElement element, Vector2 position)
        {
            
            _elements.Add(element);
            _scrollAreaRect.AddChild(element);
            AddElementEvents(element);
            element.Transform.LocalPosition = position;
            IsDirty = true;
        }

        private void AddElementEvents(BaseInteractiveUIElement element)
        {
            element.Tapped += OnElementTapped;
            element.Pressed += OnElementPressed;
            element.Released += OnElementReleased;
            element.DoubleTapped += OnElementDoubleTapped;
            element.Dragged += OnElementDragged;
            element.DragStarted += OnElementDragStarted;
            element.DragCompleted += OnElementDragCompleted;
            element.PointerWheelChanged += OnElementPointerWheelChanged;
        }

        private void OnElementPointerWheelChanged(InteractiveBaseRenderItem item, CanvasPointer pointer, float delta)
        {
            ElementPointWheelChanged?.Invoke(item, pointer, delta);
            ScrollingCanvas_PointerWheelChanged(item, pointer, delta);
        }

        private void OnElementDragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ElementDragCompleted?.Invoke(this, pointer);
        }

        private void OnElementDragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ElementDragStarted?.Invoke(this, pointer);
            ScrollingCanvas_DragStarted(item, pointer);
        }

        private void OnElementDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ElementDragged?.Invoke(this, pointer);
            ScrollingCanvas_Dragged(item, pointer);
        }

        protected virtual void OnElementDoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ElementDoubleTapped?.Invoke(this, pointer);
        }

        protected virtual void OnElementReleased(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ElementReleased?.Invoke(this, pointer);
        }

        protected virtual void OnElementPressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ElementPressed?.Invoke(this, pointer);
        }

        protected virtual void OnElementTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ElementTapped?.Invoke(item, pointer);
        }

        private void ScrollingCanvas_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // update the crop rect to reflect the drag
            _cropRect = new Rect(_initialDragPosition.X - pointer.Delta.X, _initialDragPosition.Y - pointer.Delta.Y, _cropRect.Width, _cropRect.Height);

            // determine based on the scroll direction whether horizontal scrolling is enabled
            var horizontalScrollingEnabled = new List<ScrollOrientation>
            {
                ScrollOrientation.Auto, ScrollOrientation.Both, ScrollOrientation.Horizontal
            }.Contains(ScrollDirection);

            // determine based on the scroll direction whether vertical scrolling is enabled
            var verticalScrollingEnabled = new List<ScrollOrientation>
            {
                ScrollOrientation.Auto, ScrollOrientation.Both, ScrollOrientation.Vertical
            }.Contains(ScrollDirection);

            // if horizontal scrolling is enabled shift the canvas horizontally
            if (horizontalScrollingEnabled)
            {
                HorizontalScrollBar.Position = (float)(_cropRect.Left / ScrollAreaSize.Width);
                BoundHorizontalScrollBarPosition();
            }

            // if vertical scrolling is enabled shift the canvas vertically
            if (verticalScrollingEnabled)
            {
                VerticalScrollBar.Position = (float)(_cropRect.Top / ScrollAreaSize.Height);
                BoundVerticalScrollBarPosition();
            }

            IsDirty = true;
        }

        private void ScrollingCanvas_DragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _initialDragPosition = new Vector2((float) _cropRect.X, (float) _cropRect.Y);
        }

        private void ScrollingCanvas_PointerWheelChanged(InteractiveBaseRenderItem item, CanvasPointer pointer, float delta)
        {
            if (ScrollDirection == ScrollOrientation.Auto || ScrollDirection == ScrollOrientation.Both ||
                ScrollDirection == ScrollOrientation.Vertical)
            {
                VerticalScrollBar.ChangePosition(delta > 0 ? -.05f : .05f);
            }
            else
            {
                HorizontalScrollBar.ChangePosition(delta > 0 ? -.05f : .05f);
            }
        }


        /// <summary>
        /// Removes an element from the ScrollArea
        /// </summary>
        /// <param name="element"></param>
        public void RemoveElement(BaseInteractiveUIElement element)
        {
            if (_elements.Contains(element))
            {
                _elements.Remove(element);
                _scrollAreaRect.RemoveChild(element);
                RemoveElementEvents(element);
            }
        }

        private void RemoveElementEvents(BaseInteractiveUIElement element)
        {
            element.Tapped -= OnElementTapped;
            element.Pressed -= OnElementPressed;
            element.Released -= OnElementReleased;
            element.DoubleTapped -= OnElementDoubleTapped;
            element.Dragged -= OnElementDragged;
            element.DragStarted -= OnElementDragStarted;
            element.DragCompleted -= OnElementDragCompleted;
            element.PointerWheelChanged -= OnElementPointerWheelChanged;
        }

        /// <summary>
        /// Scrolls the scrolling canvas to the specified location
        /// </summary>
        /// <param name="locationToScrollTo"></param>
        public void Scrollto(ScrollTo locationToScrollTo)
        {
            switch (locationToScrollTo)
            {
                case ScrollTo.Left:
                    HorizontalScrollBar.Position = 0;
                    break;
                case ScrollTo.Top:
                    VerticalScrollBar.Position = 0;
                    break;
                case ScrollTo.Right:
                    HorizontalScrollBar.Position = 1 - HorizontalScrollBar.Range;
                    break;
                case ScrollTo.Bottom:
                    VerticalScrollBar.Position = 1 - HorizontalScrollBar.Range;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(locationToScrollTo), locationToScrollTo, null);
            }
        }

        /// <summary>
        /// Computes the size of the scroll handle, this should be called whenever the height or width of the ScrollingCanvas
        /// or ScrollingArea changes
        /// </summary>
        private void ComputeScrollHandleSize()
        {
            // determine based on the scroll direction whether horizontal scrolling is enabled
            var horizontalScrollingEnabled = new List<ScrollOrientation>
            {
                ScrollOrientation.Auto, ScrollOrientation.Both, ScrollOrientation.Horizontal
            }.Contains(ScrollDirection);

            // determine based on the scroll direction whether horizontal scrolling is required
            var horizontalScrollingRequired = new List<ScrollOrientation>
            {
                ScrollOrientation.Both, ScrollOrientation.Horizontal
            }.Contains(ScrollDirection);

            // determine based on the scroll direction whether vertical scrolling is enabled
            var verticalScrollingEnabled = new List<ScrollOrientation>
            {
                ScrollOrientation.Auto, ScrollOrientation.Both, ScrollOrientation.Vertical
            }.Contains(ScrollDirection);

            // determine based on the scroll direction whether vertical scrolling is required
            var verticalScrollingRequired = new List<ScrollOrientation>
            {
                ScrollOrientation.Both, ScrollOrientation.Vertical
            }.Contains(ScrollDirection);


            if (horizontalScrollingEnabled && HorizontalScrollBar != null)
            {
                // calculate the ratio of the width needed for the horizontal scroll handle
                var horizontalRatio = Math.Min(1, _cropRect.Width/ScrollAreaSize.Width);

                // if the width is large enough then  hide the horizontal scroll bar
                if (Math.Abs(horizontalRatio - 1) < .001)
                {
                    HorizontalScrollBar.IsVisible = horizontalScrollingRequired; // only hide it if horizontal scrolling is not required
                }
                else
                {
                    HorizontalScrollBar.IsVisible = true;
                }

                // check to see if hiding the vertical scrollbar would make the width large enough
                // as long as vertical scrolling is not required
                if (!verticalScrollingRequired)
                {
                    var hiddenVertHorizontalRatio = Math.Min(1, (_cropRect.Width + VerticalScrollBar.Width)/ScrollAreaSize.Width);
                    if (Math.Abs(hiddenVertHorizontalRatio - 1) < .001)
                    {
                        horizontalRatio = hiddenVertHorizontalRatio;
                        HorizontalScrollBar.IsVisible = horizontalScrollingRequired;
                        VerticalScrollBar.IsVisible = false;
                    }
                }

                // set the new range of the _scrollHandle
                HorizontalScrollBar.Range = (float) horizontalRatio;

                // update the position of the scroll bar so the crop rect is maintained
                var normalizedOffset = _cropRect.Left/ScrollAreaSize.Width;
                HorizontalScrollBar.Position = (float) normalizedOffset;

                // bound the scroll bar's position
                BoundHorizontalScrollBarPosition();
            }

            //todo if we make the vertical scroll bar visible below here, but we made it invisible above
            //todo then we have to recalculate the horizontal ratio above to reflect that change
            if (verticalScrollingEnabled && VerticalScrollBar != null)
            {
                // calculate the ratio of the width needed for the vertical scroll handle
                var verticalRatio = Math.Min(1, _cropRect.Height/ScrollAreaSize.Height);

                // if the width is large enough then  hide the vertical scroll bar
                if (Math.Abs(verticalRatio - 1) < .001)
                {
                    VerticalScrollBar.IsVisible = verticalScrollingRequired; // only hide it if verticalScrolling is not required
                }
                else
                {
                    VerticalScrollBar.IsVisible = true;
                }

                // check to see if hiding the horizontal scrollbar would make the width large enough
                // as long as horizontal scrolling is not required
                if (!horizontalScrollingRequired)
                {
                    // check to see if hiding the horizontal scrollbar would make the height large enough
                    var hiddenHorzVerticalRatio = Math.Min(1, (_cropRect.Height + HorizontalScrollBar.Height)/ScrollAreaSize.Height);
                    if (Math.Abs(hiddenHorzVerticalRatio - 1) < .001)
                    {
                        verticalRatio = hiddenHorzVerticalRatio;
                        VerticalScrollBar.IsVisible = verticalScrollingRequired;
                        HorizontalScrollBar.IsVisible = false;
                    }
                }


                // set the new width of the _scrollHandle
                VerticalScrollBar.Range = (float) verticalRatio;

                // update the position of the scroll bar so the crop rect is maintained
                var normalizedOffset = _cropRect.Top/ScrollAreaSize.Height;
                VerticalScrollBar.Position = (float) normalizedOffset;

                // bound the scroll bar's position
                BoundVerticalScrollBarPosition();
            }

            if (VerticalScrollBar != null && HorizontalScrollBar != null)
            {
                // recompute crop rect
                _cropRect = new Rect(_cropRect.X, _cropRect.Y, _cropRect.Width + (VerticalScrollBar.IsVisible ? 0 : VerticalScrollBar.Width), _cropRect.Height + (HorizontalScrollBar.IsVisible ? 0 : HorizontalScrollBar.Height));
               
                UpdateScrollBars();
            }
        }

        /// <summary>
        /// Makes sure that the horizontal scroll bar is staying within the proper bounds
        /// </summary>
        private void BoundHorizontalScrollBarPosition()
        {
            // bound the position plus the range to 1
            if (HorizontalScrollBar.Position + HorizontalScrollBar.Range > 1)
            {
                HorizontalScrollBar.Position = 1 - HorizontalScrollBar.Range;
            }

            if (HorizontalScrollBar.Position < 0)
            {
                HorizontalScrollBar.Position = 0;
            }
        }

        /// <summary>
        /// makes sure that the vertical scroll bar is staying within the proper bounds
        /// </summary>
        private void BoundVerticalScrollBarPosition()
        {
            // bound the position plus the range to 1
            if (VerticalScrollBar.Position + VerticalScrollBar.Range > 1)
            {
                VerticalScrollBar.Position = 1 - VerticalScrollBar.Range;
            }

            if (VerticalScrollBar.Position < 0)
            {
                VerticalScrollBar.Position = 0;
            }
        }

        private void UpdateScrollBars()
        {
            if (VerticalScrollBar == null || HorizontalScrollBar == null || _lowerRightCornerRect == null)
            {
                Debug.Fail("Don't call this method if these are null");
                return;
            }
            //Ignore the corner if one of the scroll bars is not present
            bool needsRightCornerRect = HorizontalScrollBar.IsVisible && VerticalScrollBar.IsVisible;

            if (needsRightCornerRect)
            {
                _lowerRightCornerRect.IsVisible = true;
                //Place the corner in the right spot
                _lowerRightCornerRect.Transform.LocalPosition =
                    new Vector2(Width - _lowerRightCornerRect.Width - BorderWidth,
                        Height - _lowerRightCornerRect.Height - BorderWidth);

                //Shorten the scroll bars
                HorizontalScrollBar.Width = Width - _lowerRightCornerRect.Width - 2 * BorderWidth;
                VerticalScrollBar.Height = Height - _lowerRightCornerRect.Height - 2 * BorderWidth;

            }
            else
            {
                //Hide the corner
                _lowerRightCornerRect.IsVisible = false;

                // place the horizontal scroll bar in the correct position
                HorizontalScrollBar.Transform.LocalPosition = new Vector2(BorderWidth, Height - BorderWidth - HorizontalScrollBar.Height);

                // place the vertical scrollbar in the correct position
                VerticalScrollBar.Transform.LocalPosition = new Vector2(Width - BorderWidth - VerticalScrollBar.Width, BorderWidth);

                //give the scrollbars their full size
                HorizontalScrollBar.Width = Width - 2 * BorderWidth;
                VerticalScrollBar.Height = Height - 2 * BorderWidth;


            }
        }

        /// <summary>
        /// Called whenever the size of the scrolling canvas is changed
        /// </summary>
        protected virtual void OnSizeChanged()
        {
            if (_lowerRightCornerRect != null && HorizontalScrollBar != null && VerticalScrollBar != null)
            {
                UpdateScrollBars();


            }

            if (_scrollAreaRect != null && _cropRect != null)
            {
                _scrollAreaRect.Width = (float) _cropRect.Width;
                _scrollAreaRect.Height = (float) _cropRect.Height;
            }


            ComputeScrollHandleSize();

            IsDirty = true;
        }

        /// <summary>
        /// Rerenders the entire scrollingcanvas, including all the elements inside it and the scrollbars
        /// </summary>
        private void ReRender()
        {
            // if we do not have to rerender then just return
            if (!IsDirty)
            {
                return;
            }

            // compute the part of the ScrollArea that we are going to show
            ComputeCrop();
            ComputeScrollHandleSize();

            // make all the currently visible elements invisible
            foreach (var element in _visibleElements.ToArray())
            {
                element.IsVisible = false;
                _visibleElements.Remove(element);
            }


            // make the newly visible elements visible
            Vector2 upperLeft;
            Vector2 lowerRight;
            foreach (var element in Elements.ToArray())
            {
                upperLeft = element.Transform.LocalPosition;
                lowerRight = new Vector2(element.Transform.LocalX + element.Width, element.Transform.LocalY + element.Height);
                if (IsPartiallyContained(upperLeft, lowerRight, _cropRect))
                {
                    element.IsVisible = true;
                    _visibleElements.Add(element);
                }
                else
                {
                    element.IsVisible = false;
                }
            }

            // update the mask
            _scrollAreaRect.Mask = new Rect(_cropRect.X, _cropRect.Y, _cropRect.Width, _cropRect.Height);

            _scrollAreaRect.Transform.LocalPosition = new Vector2((float) -_cropRect.X, (float) -_cropRect.Y);
            
            // stop refreshing the ui it was just refreshed
            IsDirty = false;
        }

        /// <summary>
        /// Returns true if the rectangle defined by the upperLeft and lowerRight are partially contained in the cropRect
        /// </summary>
        /// <param name="upperLeft"></param>
        /// <param name="lowerRight"></param>
        /// <param name="cropRect"></param>
        /// <returns></returns>
        private bool IsPartiallyContained(Vector2 upperLeft, Vector2 lowerRight, Rect cropRect)
        {
            var checkRect = new Rect(new Point(upperLeft.X, upperLeft.Y), new Point(lowerRight.X, lowerRight.Y));
            if (checkRect.Left < cropRect.Right && checkRect.Right > cropRect.Left && checkRect.Top < cropRect.Bottom && checkRect.Bottom > cropRect.Top)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// based on the scrollbars positions computes the area that is going to be displayed on the
        /// scrolling canvas, call this whenever the ScrollBarWidth is changed
        /// </summary>
        private void ComputeCrop()
        {
            var cropWidth = Width - VerticalScrollBar.Width - 2*BorderWidth;
            var cropHeight = Height - HorizontalScrollBar.Height - 2*BorderWidth;

            // bound the scroll bar positions
            BoundVerticalScrollBarPosition();
            BoundHorizontalScrollBarPosition();

            _cropRect = new Rect(ScrollAreaSize.Width*HorizontalScrollBar.Position, ScrollAreaSize.Height*VerticalScrollBar.Position, cropWidth, cropHeight);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
            {
                return;
            }
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            base.Draw(ds);

            ds.Transform = orgTransform;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            ReRender();
            base.Update(parentLocalToScreenTransform);
        }

        public override void Dispose()
        {
            _scrollAreaRect.PointerWheelChanged -= ScrollingCanvas_PointerWheelChanged;
            _scrollAreaRect.DragStarted -= ScrollingCanvas_DragStarted;
            _scrollAreaRect.Dragged -= ScrollingCanvas_Dragged;
            HorizontalScrollBar.ScrollBarPositionChanged -= _horizontalScrollBar_ScrollBarPositionChanged;
            VerticalScrollBar.ScrollBarPositionChanged -= _verticalScrollBar_ScrollBarPositionChanged;

            foreach (var element in _elements)
            {
                RemoveElementEvents(element);
            }
            base.Dispose();
        }
    }
}
