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
        /// The current _snapPosition of the WindowBaseRenderItem
        /// </summary>
        private SnapPosition _snapPosition;

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
        public enum SnapPosition { Top, Left, Right, Bottom }

        /// <summary>
        /// Boolean which drescibes whether or not the current window instance
        /// is snappable. True if the window supports snapping. False
        /// if the window does not support snapping. Default is true.
        /// </summary>
        public bool SnapEnabled { get; set; }

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
            // set SnapEnabled to true, this is the default
            SnapEnabled = true;

            // snap the window to the rigth side of the screen
            SnapTo(SnapPosition.Right);

            // create the _resizer and _topBar and _preview
            _resizer = new WindowResizerRenderItem(this, _canvas);
            _topBar = new WindowTopBarRenderItem(this, _canvas);
            _preview = new WindowSnapPreviewRenderItem(this, _canvas);

            // listen to the _resizer event
            _resizer.ResizerDragged += ResizerOnResizerDragged;

            // listen to the _topBar events
            _topBar.TopBarDragged += TopBarDragged;
            _topBar.TopBarReleased += TopBarReleased;

            // add the manipulation mode methods
            Dragged += WindowBaseRenderItem_Dragged;
            Tapped += WindowBaseRenderItem_Tapped;
            DoubleTapped += WindowBaseRenderItem_DoubleTapped;

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
            // if the pointer is on the right bound of the screen snap right
            if (pointer.CurrentPoint.X > SessionController.Instance.ScreenWidth - _snapMargin)
            {
                SnapTo(SnapPosition.Right);
                _preview.HidePreview();
            }
            // else if the pointer is on the bottom bound of the screen snap bottom
            else if (pointer.CurrentPoint.Y > SessionController.Instance.ScreenHeight - _snapMargin)
            {
                SnapTo(SnapPosition.Bottom);
                _preview.HidePreview();
            }
            // else if the pointer is on the left bound of the screen snap left
            else if (pointer.CurrentPoint.X < 0 + _snapMargin)
            {
                SnapTo(SnapPosition.Left);
                _preview.HidePreview();
            }
            // else if the pointer is on the top bound of the screen snap top
            else if (pointer.CurrentPoint.Y < 0 + _snapMargin)
            {
                SnapTo(SnapPosition.Top);
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
            });
            

            // if the pointer is on the right bound of the screen show preview right
            if (pointer.CurrentPoint.X > SessionController.Instance.ScreenWidth - _snapMargin)
            {
                ShowPreview(SnapPosition.Right);
            }
            // else if the pointer is on the bottom bound of the show preview bottom
            else if (pointer.CurrentPoint.Y > SessionController.Instance.ScreenHeight - _snapMargin)
            {
                ShowPreview(SnapPosition.Bottom);
            }
            // else if the pointer is on the left bound of the show preview left
            else if (pointer.CurrentPoint.X < 0 + _snapMargin)
            {
                ShowPreview(SnapPosition.Left);
            }
            // else if the pointer is on the top bound of the show preview top
            else if (pointer.CurrentPoint.Y < 0 + _snapMargin)
            {
                ShowPreview(SnapPosition.Top);
            }
            // else hide the preview
            else
            {
                _preview.HidePreview();
            }
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
            Dragged -= WindowBaseRenderItem_Dragged;
            Tapped -= WindowBaseRenderItem_Tapped;
            DoubleTapped -= WindowBaseRenderItem_DoubleTapped;
            _resizer.ResizerDragged -= ResizerOnResizerDragged;
            _topBar.TopBarDragged -= TopBarDragged;
            _topBar.TopBarReleased -= TopBarReleased;


            // call base.Dispose to continue disposing items down the stack
            base.Dispose();
        }

        /// <summary>
        /// Fired when the WindowBaseRenderItem is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void WindowBaseRenderItem_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ;
        }

        private void WindowBaseRenderItem_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_snapPosition == SnapPosition.Top)
            {
                SnapTo(SnapPosition.Right);
            } else if (_snapPosition == SnapPosition.Right)
            {
                SnapTo(SnapPosition.Bottom);
            } else if (_snapPosition == SnapPosition.Bottom)
            {
                SnapTo(SnapPosition.Left);
            } else if (_snapPosition == SnapPosition.Left)
            {
                SnapTo(SnapPosition.Top);
            }
            else
            {
                Debug.Fail("Some snap case was not considered");
            }
        }

        /// <summary>
        /// Fired when the WindowBaseRenderItem is dragged
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void WindowBaseRenderItem_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ;
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
                    Size = new Size(SessionController.Instance.ScreenWidth, SessionController.Instance.ScreenHeight/2);
                    Transform.LocalPosition = new Vector2(0, 0);
                    break;
                case SnapPosition.Left:
                    Size = new Size(SessionController.Instance.ScreenWidth / 2, SessionController.Instance.ScreenHeight);
                    Transform.LocalPosition = new Vector2(0, 0);
                    break;
                case SnapPosition.Right:
                    Size = new Size(SessionController.Instance.ScreenWidth/2, SessionController.Instance.ScreenHeight);
                    Transform.LocalPosition = new Vector2((float)SessionController.Instance.ScreenWidth / 2, 0);
                    break;
                case SnapPosition.Bottom:
                    Size = new Size(SessionController.Instance.ScreenWidth, SessionController.Instance.ScreenHeight/2);
                    Transform.LocalPosition = new Vector2(0, (float) SessionController.Instance.ScreenHeight / 2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }

            // set the current position to the new position
            _snapPosition = position;

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
                    previewSize = new Size(SessionController.Instance.ScreenWidth, SessionController.Instance.ScreenHeight / 2);
                    previewOffset = new Vector2(-Transform.LocalPosition.X, -Transform.LocalPosition.Y);
                    break;
                case SnapPosition.Left:
                    previewSize = new Size(SessionController.Instance.ScreenWidth / 2, SessionController.Instance.ScreenHeight);
                    previewOffset = new Vector2(-Transform.LocalPosition.X, -Transform.LocalPosition.Y);
                    break;
                case SnapPosition.Right:
                    previewSize = new Size(SessionController.Instance.ScreenWidth / 2, SessionController.Instance.ScreenHeight);
                    previewOffset = new Vector2(-Transform.LocalPosition.X + (float)SessionController.Instance.ScreenWidth / 2, -Transform.LocalPosition.Y);
                    break;
                case SnapPosition.Bottom:
                    previewSize = new Size(SessionController.Instance.ScreenWidth, SessionController.Instance.ScreenHeight / 2);
                    previewOffset = new Vector2(-Transform.LocalPosition.X, -Transform.LocalPosition.Y + (float)SessionController.Instance.ScreenHeight / 2);
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
