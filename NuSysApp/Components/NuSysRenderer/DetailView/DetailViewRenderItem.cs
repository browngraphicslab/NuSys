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
    public class DetailViewRenderItem : InteractiveBaseRenderItem
    {
        /// <summary>
        /// Enum describing the different positions the DetailViewRenderItem
        /// can snap to. The DetailViewRenderItem can be snapped to
        /// any of these positions using the function SnapTo
        /// </summary>
        public enum SnapPosition { Top, Left, Right, Bottom }

        /// <summary>
        ///  The canvas the DetailViewRenderItem is drawn on
        /// </summary>
        private CanvasAnimatedControl _canvas;

        /// <summary>
        /// The current _snapPosition of the DetailViewRenderItem
        /// </summary>
        private SnapPosition _snapPosition;

        /// <summary>
        /// The current size of the DetailViewRenderItem in pixels.
        /// Contains Width and Height variables
        /// </summary>
        public Size Size;

        public DetailViewRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the canvas equal to the passed in resourceCreator
            _canvas = resourceCreator as CanvasAnimatedControl;
            Debug.Assert(_canvas != null);

            // snap the detail viewer to the rigth side of the screen
            SnapTo(SnapPosition.Right);

            // add the manipulation mode methods
            Dragged += DetailViewRenderItem_Dragged;
            Tapped += DetailViewRenderItem_Tapped;
            DoubleTapped += DetailViewRenderItem_DoubleTapped;
        }

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

        public override Rect GetLocalBounds()
        {
            return new Rect(0, 0, Size.Width, Size.Height);
        }

        private void DetailViewRenderItem_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ;
        }

        private void DetailViewRenderItem_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
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

        private void DetailViewRenderItem_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            Transform.LocalPosition += pointer.DeltaSinceLastUpdate;
        }

        /// <summary>
        /// Snaps the DetailViewRenderItem to the passed in position
        /// </summary>
        /// <param name="position"></param>
        private void SnapTo(SnapPosition position)
        {
            // snap the detail view to the correct position
            switch (position)
            {
                case SnapPosition.Top:
                    Size.Width = SessionController.Instance.ScreenWidth;
                    Size.Height = SessionController.Instance.ScreenHeight / 2;
                    Transform.LocalPosition = new Vector2(0, 0);
                    break;
                case SnapPosition.Left:
                    Size.Width = SessionController.Instance.ScreenWidth / 2;
                    Size.Height = SessionController.Instance.ScreenHeight;
                    Transform.LocalPosition = new Vector2(0, 0);
                    break;
                case SnapPosition.Right:
                    Size.Width = SessionController.Instance.ScreenWidth / 2;
                    Size.Height = SessionController.Instance.ScreenHeight;
                    Transform.LocalPosition = new Vector2((float)SessionController.Instance.ScreenWidth / 2, 0);
                    break;
                case SnapPosition.Bottom:
                    Size.Width = SessionController.Instance.ScreenWidth;
                    Size.Height = SessionController.Instance.ScreenHeight / 2;
                    Transform.LocalPosition = new Vector2(0, (float) SessionController.Instance.ScreenHeight / 2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }

            // set the current position to the new position
            _snapPosition = position;

        }

       
    }
}
