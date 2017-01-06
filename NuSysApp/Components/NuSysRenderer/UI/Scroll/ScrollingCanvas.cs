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

namespace NuSysApp
{
    public class ScrollingCanvas : RectangleUIElement
    {

        /// <summary>
        /// the background of the scrollbar, tap this to scroll to a position
        /// </summary>
        private RectangleUIElement _verticalScrollBarBackground;

        /// <summary>
        /// The handle of the scrollbar, grab this and move up and down
        /// </summary>
        private RectangleUIElement _verticalScrollHandle;

        /// <summary>
        /// Button at the top of the scrollbar used to scroll to the top
        /// </summary>
        private ButtonUIElement _upScrollButton;

        /// <summary>
        /// Button at the bottom of the scrollbar used to scroll to the bottom 
        /// </summary>
        private ButtonUIElement _downScrollButton;

        /// <summary>
        /// the background of the scrollbar, tap this to scroll to a position
        /// </summary>
        private RectangleUIElement _horizontalScrollBarBackground;

        /// <summary>
        /// The handle of the scrollbar, grab this and move left and right
        /// </summary>
        private RectangleUIElement _horizontalScrollHandle;

        /// <summary>
        /// Button at the left of the scrollbar used to scroll to the left
        /// </summary>
        private ButtonUIElement _leftScrollButton;

        /// <summary>
        /// Button at the bottom of the scrollbar used to scroll to the right
        /// </summary>
        private ButtonUIElement _rightScrollButton;

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
        /// The possible orientation sof the scrollbar
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
        /// private helper variable for the public field ScrollBarWidth
        /// </summary>
        private float _scrollBarWidth { get; set; }

        /// <summary>
        /// The width of the scrollbar, like border width, this is implied to mean the distance from the inside of the ScrollingCanvas to
        /// the outside of the ScrollingCanvas without regard to the scrollbars orientation. 
        /// </summary>
        public float ScrollBarWidth
        {
            get { return _scrollBarWidth; }
            set
            {
                _scrollBarWidth = value;
                UpdateScrollBarUI();
            }
        }

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
                OnSizeChanged();
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

        /// <summary>
        /// The color of the background of the scroll handle
        /// </summary>
        public Color ScrollHandleBackground { get; set; } = UIDefaults.ScrollHandleBackground;

        /// <summary>
        /// The color of the background of the scroll bar
        /// </summary>
        public Color ScrollBarBackground { get; set; } = UIDefaults.ScrollBarBackground;

        public Color ScrollButtonColor { get; set; } = UIDefaults.ScrollButtonColor;

        public ScrollingCanvas(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, ScrollOrientation scrollDirection) : base(parent, resourceCreator)
        {
            // set the scroll direction based on the passed in scroll direction
            ScrollDirection = scrollDirection;

            // add the vertical and horizontal scrollbars
            SetupVerticalScrollBar();
            SetupHorizontalScrollBar();

            _scrollAreaRect = new MaskedRectangleUIElement(this, resourceCreator);
            AddChild(_scrollAreaRect);
            _lowerRightCornerRect = new RectangleUIElement(this, resourceCreator)
            {
                Background = ScrollButtonColor
            };
            AddChild(_lowerRightCornerRect);

            // initialize the elements list
            _elements = new List<BaseInteractiveUIElement>();
            _visibleElements = new List<BaseInteractiveUIElement>();

            // set the ui defaults
            ScrollBarWidth = UIDefaults.ScrollBarWidth;
        }

        /// <summary>
        /// Sets up the horizontal scrollbar and adds all the horizontal scroll elements as children
        /// </summary>
        private void SetupHorizontalScrollBar()
        {
            _horizontalScrollBarBackground = new RectangleUIElement(this, ResourceCreator)
            {
                Background = ScrollBarBackground
            };
            AddChild(_horizontalScrollBarBackground);

            _horizontalScrollHandle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = ScrollHandleBackground
            };
            AddChild(_horizontalScrollHandle);

            _leftScrollButton = new RectangleButtonUIElement(this, ResourceCreator)
            {
                Background = ScrollButtonColor
            };
            AddChild(_leftScrollButton);

            _rightScrollButton = new RectangleButtonUIElement(this, ResourceCreator)
            {
                Background = ScrollButtonColor
            };
            AddChild(_rightScrollButton);
        }

        /// <summary>
        /// Sets up the vertical scrollbar and adds all the vertical scroll elements as children
        /// </summary>
        private void SetupVerticalScrollBar()
        {
            _verticalScrollBarBackground = new RectangleUIElement(this, ResourceCreator)
            {
                Background = ScrollBarBackground
            };
            AddChild(_verticalScrollBarBackground);

            _verticalScrollHandle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = ScrollHandleBackground
            };
            AddChild(_verticalScrollHandle);

            _upScrollButton = new RectangleButtonUIElement(this, ResourceCreator)
            {
                Background = ScrollButtonColor
            };
            AddChild(_upScrollButton);

            _downScrollButton = new RectangleButtonUIElement(this, ResourceCreator)
            {
                Background = ScrollButtonColor
            };
            AddChild(_downScrollButton);
        }

        /// <summary>
        /// Safely updates the ui for all the scrollbars, called whenever a public field that controls the ui is changed
        /// </summary>
        private void UpdateScrollBarUI()
        {
            // update the horizontal scroll bar
            if (_horizontalScrollBarBackground != null)
            {
                _horizontalScrollBarBackground.Height = ScrollBarWidth;
            }
            if (_horizontalScrollHandle != null)
            {
                _horizontalScrollHandle.Height = ScrollBarWidth;
            }
            if (_leftScrollButton != null)
            {
                _leftScrollButton.Height = ScrollBarWidth;
                _leftScrollButton.Width = ScrollBarWidth;
            }
            if (_rightScrollButton != null)
            {
                _rightScrollButton.Height = ScrollBarWidth;
                _rightScrollButton.Width = ScrollBarWidth;
            }

            //update the vertical scroll bar
            if (_verticalScrollBarBackground != null)
            {
                _verticalScrollBarBackground.Width = ScrollBarWidth;
            }
            if (_verticalScrollHandle != null)
            {
                _verticalScrollHandle.Width = ScrollBarWidth;
            }
            if (_upScrollButton != null)
            {
                _upScrollButton.Height = ScrollBarWidth;
                _upScrollButton.Width = ScrollBarWidth;
            }
            if (_downScrollButton != null)
            {
                _downScrollButton.Height = ScrollBarWidth;
                _downScrollButton.Width = ScrollBarWidth;
            }

            if (_lowerRightCornerRect != null)
            {
                _lowerRightCornerRect.Width = ScrollBarWidth;
                _lowerRightCornerRect.Height = ScrollBarWidth;
            }


            OnSizeChanged();
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

            if (horizontalScrollingEnabled && _horizontalScrollBarBackground != null &&
                _horizontalScrollHandle != null)
            {
                // calculate the ratio of the width needed for the horizontal scroll handle
                var horizontalRatio = Math.Min(1, _cropRect.Width / ScrollAreaSize.Width);

                // if the width is large enough then  hide the horizontal scroll bar
                if (Math.Abs(horizontalRatio - 1) < .001)
                {
                    _horizontalScrollBarBackground.IsVisible = false;
                    _horizontalScrollHandle.IsVisible = false;
                }
                else
                {
                    _horizontalScrollBarBackground.IsVisible = true;
                    _horizontalScrollHandle.IsVisible = true;
                }

                // set the new width of the _scrollHandle
                _horizontalScrollHandle.Width = (float)(_cropRect.Width * horizontalRatio);

                // update the position of the scroll bar so the crop rect is maintained
                var normalizedOffset = _cropRect.Left / ScrollAreaSize.Width;
                _horizontalScrollHandle.Transform.LocalPosition = new Vector2((float) (normalizedOffset * _cropRect.Width), _horizontalScrollHandle.Transform.LocalY);
                BoundHorizontalScrollHandle();
            }

            // determine based on the scroll direction whether horizontal scrolling is enabled
            var verticalScrollingEnabled = new List<ScrollOrientation>
            {
                ScrollOrientation.Auto,
                ScrollOrientation.Both,
                ScrollOrientation.Vertical
            }.Contains(ScrollDirection);

            if (verticalScrollingEnabled && _verticalScrollBarBackground != null &&
                _verticalScrollHandle != null)
            {
                // calculate the ratio of the width needed for the vertical scroll handle
                var verticalRatio = Math.Min(1, _cropRect.Height / ScrollAreaSize.Height);

                // if the width is large enough then  hide the vertical scroll bar
                if (Math.Abs(verticalRatio - 1) < .001)
                {
                    _verticalScrollBarBackground.IsVisible = false;
                    _verticalScrollHandle.IsVisible = false;
                }
                else
                {
                    _verticalScrollBarBackground.IsVisible = true;
                    _verticalScrollHandle.IsVisible = true;
                }

                // set the new width of the _scrollHandle
                _verticalScrollHandle.Height = (float)(_cropRect.Height * verticalRatio);

                // update the position of the scroll bar so the crop rect is maintained
                var normalizedOffset = _cropRect.Top / ScrollAreaSize.Height;
                _verticalScrollHandle.Transform.LocalPosition = new Vector2(_verticalScrollHandle.Transform.LocalX, (float)(normalizedOffset * _cropRect.Height));
                BoundVerticalScrollHandle();
            }

        }

        /// <summary>
        /// bounds the vertical handle's top so that it is within the top of the vertical scroll background
        /// and so that its bottom is within the bottom of the vertical scroll background
        /// </summary>
        private void BoundVerticalScrollHandle()
        {
            if (_verticalScrollHandle.Transform.LocalY < _verticalScrollBarBackground.Transform.LocalY)
            {
                _verticalScrollHandle.Transform.LocalPosition = new Vector2(_verticalScrollHandle.Transform.LocalX, _verticalScrollBarBackground.Transform.LocalY);
            }
            else if (_verticalScrollHandle.Transform.LocalY + _verticalScrollHandle.Height > _verticalScrollBarBackground.Transform.LocalY + _verticalScrollBarBackground.Height)
            {
                _horizontalScrollHandle.Transform.LocalPosition = new Vector2(_horizontalScrollHandle.Transform.LocalX, _verticalScrollBarBackground.Transform.LocalY + _verticalScrollBarBackground.Height - _verticalScrollHandle.Height);
            }
        }


        /// <summary>
        /// bounds the horizontal handle's left side so that it is within the left side of the horizontal scroll background
        /// and so that its right side is within the right side of the horizontal scroll background
        /// </summary>
        private void BoundHorizontalScrollHandle()
        {
            if (_horizontalScrollHandle.Transform.LocalX < _horizontalScrollBarBackground.Transform.LocalX)
            {
                _horizontalScrollHandle.Transform.LocalPosition = new Vector2(_horizontalScrollBarBackground.Transform.LocalX, _horizontalScrollHandle.Transform.LocalY);
            }
            else if (_horizontalScrollHandle.Transform.LocalX + _horizontalScrollHandle.Width > _horizontalScrollBarBackground.Transform.LocalX + _horizontalScrollBarBackground.Width)
            {
                _horizontalScrollHandle.Transform.LocalPosition = new Vector2(_horizontalScrollBarBackground.Transform.LocalX + _horizontalScrollBarBackground.Width - _horizontalScrollHandle.Width, _horizontalScrollHandle.Transform.LocalY);
            }
        }

        /// <summary>
        /// Called whenever the size of the scrolling canvas is changed
        /// </summary>
        private void OnSizeChanged()
        {
            if (_lowerRightCornerRect != null)
            {
                _lowerRightCornerRect.Transform.LocalPosition = new Vector2(Width - ScrollBarWidth - BorderWidth, Height - ScrollBarWidth - BorderWidth);
            }

            if (_horizontalScrollBarBackground != null & _leftScrollButton != null &&
                _rightScrollButton != null && _horizontalScrollHandle != null && _lowerRightCornerRect != null)
            {
                // update the positions of all the horizontal elements
                var horizontalScrollYPos = Height - BorderWidth - ScrollBarWidth;
                _leftScrollButton.Transform.LocalPosition = new Vector2(BorderWidth, horizontalScrollYPos);
                _horizontalScrollBarBackground.Transform.LocalPosition = new Vector2(BorderWidth + _leftScrollButton.Width, horizontalScrollYPos);
                _horizontalScrollHandle.Transform.LocalPosition = new Vector2(_horizontalScrollHandle.Transform.LocalX, horizontalScrollYPos);
                _rightScrollButton.Transform.LocalPosition = new Vector2(Width - BorderWidth - _rightScrollButton.Width - _lowerRightCornerRect.Width, horizontalScrollYPos);

                // update the size of the horizontal scroll bar background
                _horizontalScrollBarBackground.Width = Width - 2*BorderWidth - _rightScrollButton.Width -
                                                       _leftScrollButton.Width - _lowerRightCornerRect.Width;
            }

            if (_verticalScrollBarBackground != null & _upScrollButton != null &&
                _downScrollButton != null && _verticalScrollHandle != null && _lowerRightCornerRect != null)
            {
                //update the positions of all the vertical elements
                var verticalScrollXPos = Width - BorderWidth - ScrollBarWidth;
                _upScrollButton.Transform.LocalPosition = new Vector2(verticalScrollXPos, BorderWidth);
                _verticalScrollBarBackground.Transform.LocalPosition = new Vector2(verticalScrollXPos, BorderWidth + _upScrollButton.Height);
                _verticalScrollHandle.Transform.LocalPosition = new Vector2(verticalScrollXPos, _verticalScrollHandle.Transform.LocalY);
                _downScrollButton.Transform.LocalPosition = new Vector2(verticalScrollXPos, Height - _downScrollButton.Height - BorderWidth - _lowerRightCornerRect.Height);

                // update the size of the vertical scroll bar background
                _verticalScrollBarBackground.Height = Height - 2*BorderWidth - _upScrollButton.Height -
                                                      _downScrollButton.Height - _lowerRightCornerRect.Height;
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
            _scrollAreaRect.Mask = new Rect(BorderWidth, BorderWidth, _cropRect.Width, _cropRect.Height);

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
            return lowerRight.X > cropRect.Left && lowerRight.X < cropRect.Right ||
                upperLeft.X < cropRect.Right && upperLeft.X > cropRect.Left;
        }

        /// <summary>
        /// based on the scrollbars positions computes the area that is going to be displayed on the
        /// scrolling canvas, call this whenever the ScrollBarWidth is changed
        /// </summary>
        private void ComputeCrop()
        {
            var _cropWidth = (_verticalScrollBarBackground.IsVisible ? Width - _verticalScrollBarBackground.Width : Width) - 2*BorderWidth;
            var _cropHeight = (_horizontalScrollBarBackground.IsVisible ? Height - _horizontalScrollBarBackground.Height : Height) - 2*BorderWidth;

            _cropRect = new Rect(ScrollAreaSize.Width*(_horizontalScrollHandle.Transform.LocalX - _horizontalScrollBarBackground.Transform.LocalX)/_cropWidth,
                ScrollAreaSize.Height* (_verticalScrollHandle.Transform.LocalY - _verticalScrollBarBackground.Transform.LocalY) / _cropHeight, _cropWidth, _cropHeight);

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
