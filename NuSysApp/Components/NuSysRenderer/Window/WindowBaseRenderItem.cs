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
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class WindowBaseRenderItem : InteractiveBaseRenderItem
    {

        /// <summary>
        ///  The canvas the WindowBaseRenderItem is drawn on
        /// </summary>
        private CanvasAnimatedControl _canvas;

        /// <summary>
        /// The window's resizer
        /// </summary>
        private WindowResizerRenderItem _resizer;

        /// <summary>
        /// The preview of the window for snapping
        /// </summary>
        private WindowSnapPreviewRenderItem _preview;

        /// <summary>
        /// The window's top bar. Used for moving the window and snappnig
        /// if snapping is enable
        /// </summary>
        private WindowTopBarRenderItem _topBar;

        /// <summary>
        /// The size of the WindowBaseRenderItem in pixels.
        /// DO NOT SET THIS DIRECTLY. It would fail to populate SizeChanged event in window stack
        /// </summary>
        private Size _size;

        /// <summary>
        /// A size representing the full size of the screen in pixels
        /// </summary>
        private Size _fullScreen => new Size(SessionController.Instance.ScreenWidth, SessionController.Instance.ScreenHeight);

        /// <summary>
        /// A size representing half the size of the screen in pixels
        /// </summary>
        private Size _halfScreen
            => new Size(SessionController.Instance.ScreenWidth/2, SessionController.Instance.ScreenHeight);

        private Size _quarterScreen => new Size(SessionController.Instance.ScreenWidth / 2, SessionController.Instance.ScreenHeight / 2);

        /// <summary>
        /// The acceptable margin of error in pixels between the pointer position
        /// and the edge of the screen for a snap event to occur. 
        /// Should probably be set somewhere between 2 and 5 pixels.
        /// </summary>
        private float _snapMargin = 3;

        /// <summary>
        /// The current size of the WindowBaseRenderItem in pixels.
        /// Contains Width and Height variables
        /// </summary>
        public Size Size
        {
            get { return _size; }
            set
            {
                _size = value;
                SizeChanged?.Invoke(_size);
            }
        }

        /// <summary>
        /// The size in pixels of the window's resizer border, can only be set on initialization
        /// </summary>
        public float ResizerSize = 3; // default value

        /// <summary>
        /// The size in pixels of the window's TopBar, can only be set on initialization
        /// </summary>
        public float TopBarSize = 25; // default value

        /// <summary>
        /// Delegate used when the size of the window is changed
        /// </summary>
        /// <param name="size"></param>
        public delegate void WindowSizeChangedHandler(Size size);

        /// <summary>
        /// Event fired whenever the size of the window changes
        /// </summary>
        public event WindowSizeChangedHandler SizeChanged;

        /// <summary>
        /// Enum describing the different positions the WindowBaseRenderItem
        /// can snap to. The WindowBaseRenderItem can be snapped to
        /// any of these positions using the function SnapTo
        /// </summary>
        public enum SnapPosition { Top, Left, Right, TopLeft, TopRight, BottomLeft, BottomRight, Center}

        public WindowBaseRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the canvas equal to the passed in resourceCreator
            _canvas = resourceCreator as CanvasAnimatedControl;
            Debug.Assert(_canvas != null, "The passed in canvas should be an AnimatedCanvas if not add support for other types here");
        }

        /// <summary>
        /// An initializer method
        /// </summary>
        public override async Task Load() //todo find out exactly when this is called
        {
            // snap the window to the rigth side of the screen
            SnapTo(SnapPosition.Right);

            // create the _resizer and _topBar and _preview
            _resizer = new WindowResizerRenderItem(this, _canvas, ResizerSize);
            _topBar = new WindowTopBarRenderItem(this, _canvas, TopBarSize);
            _preview = new WindowSnapPreviewRenderItem(this, _canvas);

            // listen to the _resizer event
            _resizer.ResizerDragged += ResizerOnResizerDragged;

            // listen to the _topBar events
            _topBar.TopBarDragged += TopBarDragged;
            _topBar.TopBarReleased += TopBarReleased;

            // add the _reizer and _topBar and the _preview to the children of the WindowBaseRenderItem
            AddChild(_resizer);
            AddChild(_topBar);
            AddChild(_preview);

            base.Load();
        }

        /// <summary>
        /// Fired whenever a pointer on the Top Bar is released. Handles snapping.
        /// </summary>
        /// <param name="pointer"></param>
        private void TopBarReleased(CanvasPointer pointer)
        {
            // get the SnapPosition if the pointer was released in a SnapPosition
            SnapPosition? position = GetSnapPosition(pointer);
            if (position != null)
            {
                // snap to the SnapPosition if one existed
                SnapTo(position.Value);
                // Hide the preview since it is no longer necessary
                _preview.HidePreview();
            }
        }

        /// <summary>
        /// Fired whenever the Top Bar Is Dragged. Handles moving the window around the screen.
        /// Handles showing and hiding the preview window.
        /// </summary>
        /// <param name="offsetDelta"></param>
        /// <param name="pointer"></param>
        private void TopBarDragged(Vector2 offsetDelta, CanvasPointer pointer)
        {
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                // shift the transform by the amount the topBar was moved
                Transform.LocalPosition += offsetDelta;
            

                // get the SnapPosition if the pointer is in a SnapPosition
                SnapPosition? position = GetSnapPosition(pointer);

                // if the pointer is in a SnapPosition then show the preview
                if (position != null)
                {
                    ShowPreview(position.Value);

                }
                 // otherwise hide the preview
                else
                {
                    _preview.HidePreview();
                }

            });
        }

        /// <summary>
        /// Takes in two normalized points x and y, and returns a vector 2 representing the screen coordinate of that point in pixels
        /// VERY USEFUL for setting normalized coordinates
        /// </summary>
        /// <param name="x">must be a value between 0 and 1 inclusive</param>
        /// <param name="y">must be a value between 0 and 1 inclusive</param>
        /// <returns></returns>
        private Vector2 NormalizedToScreen(float x, float y)
        {
            Debug.Assert(x <= 1 && x >= 0);
            Debug.Assert(y <= 1 && y >= 0);
            return new Vector2((float) SessionController.Instance.ScreenWidth * x, (float) SessionController.Instance.ScreenHeight * y);
        }

        /// <summary>
        /// Takes in two normalized points x and y, and returns a vector 2 representing the inverse vector used to transform the
        /// Transform.localposition to the normalized position on the screen
        /// VERY USEFUL for setting normalized coordinates
        /// </summary>
        /// <param name="x">must be a value between 0 and 1 inclusive</param>
        /// <param name="y">must be a value between 0 and 1 inclusive</param>
        /// <returns></returns>
        private Vector2 InverseNormalizedToScreen(float x, float y)
        {
            Vector2 NormalizedPostion = NormalizedToScreen(x, y);
            return NormalizedPostion - Transform.LocalPosition;
        }

        /// <summary>
        /// Takes in a CanvasPointer and returns the SnapPosition that pointer is on, null otherwise
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        private SnapPosition? GetSnapPosition(CanvasPointer pointer)
        {

            // create booleans for all the sides an initialize them to false
            bool right = false, left = false, top = false, bottom = false;

            // the pointer is on the right bound of the screen
            if (pointer.CurrentPoint.X > SessionController.Instance.ScreenWidth - _snapMargin)
            {
                right = true;
            }
            // else if the pointer is on the bottom bound of the show preview bottom
            if (pointer.CurrentPoint.Y > SessionController.Instance.ScreenHeight - _snapMargin)
            {
                bottom = true;
            }
            // else if the pointer is on the left bound of the show preview left
            if (pointer.CurrentPoint.X < 0 + _snapMargin)
            {
                left = true;
            }
            // else if the pointer is on the top bound of the show preview top
            if (pointer.CurrentPoint.Y < 0 + _snapMargin)
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
        /// Draws the window onto the screen with the offset of the Local
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            // todo explain why we need this
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            ds.FillRectangle(new Rect(0, 0, Size.Width, Size.Height), Colors.Red );

            ds.Transform = orgTransform;

            base.Draw(ds);
        }

        /// <summary>
        /// Returns the LocalBounds of the base render item, used for hit testing. The bounds are given with the offset
        /// of the local matrix assumed to be zero. If the matrix is offset, then the local bounds must be offset accordingly
        /// </summary>
        /// <returns></returns>
        public override Rect GetLocalBounds()
        {
            return new Rect(0, 0, Size.Width, Size.Height);
        }

        /// <summary>
        /// Dispose of any event handlers here and take care of clean exit
        /// </summary>
        public override void Dispose()
        {
            // remove event handlers
            _resizer.ResizerDragged -= ResizerOnResizerDragged;
            _topBar.TopBarDragged -= TopBarDragged;
            _topBar.TopBarReleased -= TopBarReleased;


            // call base.Dispose to continue disposing items down the stack
            base.Dispose();
        }

        /// <summary>
        /// Snaps the WindowBaseRenderItem to the passed in position
        /// </summary>
        /// <param name="position"></param>
        private void SnapTo(SnapPosition position)
        {
            // snap the Window to the correct position
            switch (position)
            {
                case SnapPosition.Top:
                    Size = _fullScreen;
                    Transform.LocalPosition = NormalizedToScreen(0, 0);
                    break;
                case SnapPosition.Left:
                    Size = _halfScreen;
                    Transform.LocalPosition = NormalizedToScreen(0, 0);
                    break;
                case SnapPosition.Right:
                    Size = _halfScreen;
                    Transform.LocalPosition = NormalizedToScreen(.5f, 0);
                    break;
                case SnapPosition.TopLeft:
                    Size = _quarterScreen;
                    Transform.LocalPosition = NormalizedToScreen(0, 0);
                    break;
                case SnapPosition.TopRight:
                    Size = _quarterScreen;
                    Transform.LocalPosition = NormalizedToScreen(.5f, 0);
                    break;
                case SnapPosition.BottomLeft:
                    Size = _quarterScreen;
                    Transform.LocalPosition = NormalizedToScreen(0, .5f);
                    break;
                case SnapPosition.BottomRight:
                    Size = _quarterScreen;
                    Transform.LocalPosition = NormalizedToScreen(.5f, .5f);
                    break;
                case SnapPosition.Center:
                    Size = _halfScreen;
                    Transform.LocalPosition = NormalizedToScreen(.5f, .5f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }
        }

        /// <summary>
        /// Show a preview of the WindowBaseRenderItem at the passed in position
        /// </summary>
        /// <param name="position"></param>
        protected void ShowPreview(SnapPosition position)
        {
            Size previewSize;
            Vector2 previewOffset;

            // preview the Window at the correct position
            switch (position)
            {
                case SnapPosition.Top:
                    previewSize = _fullScreen;
                    previewOffset = InverseNormalizedToScreen(0, 0);
                    break;
                case SnapPosition.Left:
                    previewSize = _halfScreen;
                    previewOffset = InverseNormalizedToScreen(0, 0);
                    break;
                case SnapPosition.Right:
                    previewSize = _halfScreen;
                    previewOffset = InverseNormalizedToScreen(.5f, 0);
                    break;
                case SnapPosition.TopLeft:
                    previewSize = _quarterScreen;
                    previewOffset = InverseNormalizedToScreen(0, 0);
                    break;
                case SnapPosition.TopRight:
                    previewSize = _quarterScreen;
                    previewOffset = InverseNormalizedToScreen(.5f, 0);
                    break;
                case SnapPosition.BottomLeft:
                    previewSize = _quarterScreen;
                    previewOffset = InverseNormalizedToScreen(0, .5f);
                    break;
                case SnapPosition.BottomRight:
                    previewSize = _quarterScreen;
                    previewOffset = InverseNormalizedToScreen(.5f, .5f);
                    break;
                case SnapPosition.Center:
                    previewSize = _halfScreen;
                    previewOffset = InverseNormalizedToScreen(.25f, .25f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }

            _preview.ShowPreview(previewSize, previewOffset);

        }

        /// <summary>
        /// Fired whenver the resizers are dragged takes cares of resizing the window properly
        /// </summary>
        /// <param name="sizeDelta"></param>
        /// <param name="offsetDelta"></param>
        private void ResizerOnResizerDragged(Vector2 sizeDelta, Vector2 offsetDelta)
        {
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                Size newSize = Size;

                // make sure that the size can never be set to a negative
                if (newSize.Width + sizeDelta.X < 0 || newSize.Height + sizeDelta.Y < 0)
                {
                    return;
                }

                newSize.Width += sizeDelta.X;
                newSize.Height += sizeDelta.Y;
                Transform.LocalPosition += offsetDelta;
                Size = newSize;
            });
        }


    }
}
