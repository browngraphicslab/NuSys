using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class ScrollingCanvas : RectangleUIElement
    {
        /// <summary>
        /// the horizontal scrollbar
        /// </summary>
        private ScrollBarUIElement _horizontalScrollBar;

        /// <summary>
        /// the vertical scrollbar
        /// </summary>
        private ScrollBarUIElement _verticalScrollBar;

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
        /// The direction of the scrollbar
        /// </summary>
        public ScrollOrientation ScrollDirection { get; }

        /// <summary>
        /// The actual scroll area used to add and remove elements from
        /// </summary>
        private MaskedRectangleUIElement _scrollAreaRect;

        /// <summary>
        /// This is true whenever the ui needs to be rerendered
        /// </summary>
        private bool _refreshUI;

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
            if (Height - _previousHeight > .01)
            {
                _previousHeight = Height;
                OnSizeChanged();
            }

            if (Width - _previousWidth > .01)
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

        public ScrollingCanvas(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, ScrollOrientation scrollDirection) : base(parent, resourceCreator)
        {
            // set the scroll direction based on the passed in scroll direction
            ScrollDirection = scrollDirection;

            // add the vertical and horizontal scrollbars
            _verticalScrollBar = new ScrollBarUIElement(this, Canvas, ScrollBarUIElement.Orientation.Vertical)
            {
                Width = UIDefaults.ScrollBarWidth
            };
            AddChild(_verticalScrollBar);
            _horizontalScrollBar = new ScrollBarUIElement(this, Canvas, ScrollBarUIElement.Orientation.Horizontal)
            {
                Height = UIDefaults.ScrollBarWidth
            };
            AddChild(_horizontalScrollBar);

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

            _horizontalScrollBar.ScrollBarPositionChanged += _horizontalScrollBar_ScrollBarPositionChanged;
            _verticalScrollBar.ScrollBarPositionChanged += _verticalScrollBar_ScrollBarPositionChanged;
        }

        private void _verticalScrollBar_ScrollBarPositionChanged(object source, float position)
        {
            _refreshUI = true;
        }

        private void _horizontalScrollBar_ScrollBarPositionChanged(object source, float position)
        {
            _refreshUI = true;
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
            element.Transform.LocalPosition = position;
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
                ScrollOrientation.Auto,
                ScrollOrientation.Both,
                ScrollOrientation.Horizontal
            }.Contains(ScrollDirection);

            if (horizontalScrollingEnabled && _horizontalScrollBar != null)
            {
                // calculate the ratio of the width needed for the horizontal scroll handle
                var horizontalRatio = Math.Min(1, _cropRect.Width / ScrollAreaSize.Width);

                // if the width is large enough then  hide the horizontal scroll bar
                if (Math.Abs(horizontalRatio - 1) < .001)
                {
                    _horizontalScrollBar.IsVisible = false;
                }
                else
                {
                    _horizontalScrollBar.IsVisible = true;

                }

                // set the new range of the _scrollHandle
                _horizontalScrollBar.Range = (float) horizontalRatio;

                // update the position of the scroll bar so the crop rect is maintained
                var normalizedOffset = _cropRect.Left / ScrollAreaSize.Width;
                _horizontalScrollBar.Position = (float) normalizedOffset;
            }

            // determine based on the scroll direction whether horizontal scrolling is enabled
            var verticalScrollingEnabled = new List<ScrollOrientation>
            {
                ScrollOrientation.Auto,
                ScrollOrientation.Both,
                ScrollOrientation.Vertical
            }.Contains(ScrollDirection);

            if (verticalScrollingEnabled && _verticalScrollBar != null)
            {
                // calculate the ratio of the width needed for the vertical scroll handle
                var verticalRatio = Math.Min(1, _cropRect.Height / ScrollAreaSize.Height);

                // if the width is large enough then  hide the vertical scroll bar
                if (Math.Abs(verticalRatio - 1) < .001)
                {
                    _verticalScrollBar.IsVisible = false;
                }
                else
                {
                    _verticalScrollBar.IsVisible = true;
                }

                // set the new width of the _scrollHandle
                _verticalScrollBar.Range = (float)verticalRatio;

                // update the position of the scroll bar so the crop rect is maintained
                var normalizedOffset = _cropRect.Top / ScrollAreaSize.Height;
                _verticalScrollBar.Position = (float)(normalizedOffset * _cropRect.Height);
            }

        }

        /// <summary>
        /// Called whenever the size of the scrolling canvas is changed
        /// </summary>
        private void OnSizeChanged()
        {
            if (_lowerRightCornerRect != null && _horizontalScrollBar != null && _verticalScrollBar != null)
            {
                _lowerRightCornerRect.Transform.LocalPosition = new Vector2(Width - _lowerRightCornerRect.Width - BorderWidth, Height - _lowerRightCornerRect.Height - BorderWidth);

                // place the horizontal scroll bar in the correct position
                _horizontalScrollBar.Width = Width - _lowerRightCornerRect.Width - 2 * BorderWidth;
                _horizontalScrollBar.Transform.LocalPosition = new Vector2(BorderWidth, Height - BorderWidth - _horizontalScrollBar.Height);

                // place the vertical scrollbar in the correct position
                _verticalScrollBar.Height = Height - _lowerRightCornerRect.Height - 2 * BorderWidth;
                _verticalScrollBar.Transform.LocalPosition = new Vector2(Width - BorderWidth - _verticalScrollBar.Width, BorderWidth);
            }

            ComputeScrollHandleSize();

            _refreshUI = true;
        }

        /// <summary>
        /// Rerenders the entire scrollingcanvas, including all the elements inside it and the scrollbars
        /// </summary>
        private void ReRender()
        {
            // if we do not have to rerender then just return
            if (!_refreshUI)
            {
                return;
            }

            // compute the part of the ScrollArea that we are going to show
            ComputeCrop();

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

            _scrollAreaRect.Transform.LocalPosition = new Vector2((float)- _cropRect.X, (float)-_cropRect.Y);

            // stop refreshing the ui it was just refreshed
            _refreshUI = false;
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
            if (checkRect.Left < cropRect.Right && checkRect.Right > cropRect.Left &&
                checkRect.Top < cropRect.Bottom && checkRect.Bottom > cropRect.Top)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// based on the scrollbars positions computes the area that is going to be displayed on the
        /// scrolling canvas, call this whenever the ScrollBarWidth is changed
        /// </summary>
        private void ComputeCrop()
        {
            var _cropWidth = (_verticalScrollBar.IsVisible ? Width - _verticalScrollBar.Width : Width) - 2*BorderWidth;
            var _cropHeight = (_horizontalScrollBar.IsVisible ? Height - _horizontalScrollBar.Height : Height) - 2*BorderWidth;

            _cropRect = new Rect(ScrollAreaSize.Width*_horizontalScrollBar.Position,
                ScrollAreaSize.Height* _verticalScrollBar.Position, _cropWidth, _cropHeight);

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
    }
}
