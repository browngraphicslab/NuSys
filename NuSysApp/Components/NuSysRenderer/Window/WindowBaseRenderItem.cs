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
        /// The window's top bar. Used for moving the window and snappnig
        /// if snapping is enable
        /// </summary>
        private WindowTopBarRenderItem _topBar;

        private Size _size;

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

            // add the manipulation mode methods
            Dragged += WindowBaseRenderItem_Dragged;
            Tapped += WindowBaseRenderItem_Tapped;
            DoubleTapped += WindowBaseRenderItem_DoubleTapped;
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

            // create the _resizer and _topBar
            _resizer = new WindowResizerRenderItem(this, _canvas);
            _topBar = new WindowTopBarRenderItem(this, _canvas);

            // listen to the _resizer event
            _resizer.ResizerDragged += ResizerOnResizerDragged;

            // add the _reizer and _topBar to the children of the WindowBaseRenderItem
            AddChild(_resizer);
            AddChild(_topBar);

            base.Load();
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
        /// Fireed when the WindowBaseRenderItem is dragged, moves the window around the screen
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void WindowBaseRenderItem_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            Transform.LocalPosition += pointer.DeltaSinceLastUpdate;
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

        private void ResizerOnResizerDragged(Vector2 sizeDelta, Vector2 offsetDelta)
        {
            Size newSize = Size;
            newSize.Width += sizeDelta.X;
            newSize.Height += sizeDelta.Y;
            Size = newSize;
            Transform.LocalPosition += offsetDelta;

        }


    }
}
