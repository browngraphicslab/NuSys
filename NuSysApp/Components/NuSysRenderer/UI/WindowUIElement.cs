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

namespace NuSysApp
{
    public class WindowUIElement : RectangleUIElement
    {
        /// <summary>
        /// The height of the TopBar.
        /// </summary>
        private float _topBarHeight;

        /// <summary>
        /// The height of the TopBar. Must be a value greater than or equal to zero.
        /// </summary>
        public float TopBarHeight
        {
            get { return _topBarHeight; }
            set
            {
                Debug.Assert(value >= 0);
                _topBarHeight = value >= 0 ? value : 0;
            }
        }

        /// <summary>
        /// The Color of the TopBar
        /// </summary>
        public Color TopBarColor;

        /// <summary>
        /// A margin of error extending beyond the width and height of the window
        /// to provide a bufferzone for touch events. Any touch in the ErrorMargin
        /// pixels extending beyond the height and width of the UI Element will
        /// trigger a touch event.
        /// </summary>
        public float ErrorMargin;

        public WindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }

        /// <summary>
        /// Draws the window onto the canvas drawing session
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            // return if the item is disposed or is not visible
            if (IsDisposed || !IsVisible)
                return;

            base.Draw(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            // draw the top bar onto the window
            ds.FillRectangle(new Rect(0, 0, Width, TopBarHeight), TopBarColor);

            ds.Transform = orgTransform;
        }

        /// <summary>
        /// Returns the bounds within which child elements should exist. Child elements should exist 
        /// in the margins of the window and below the top bar. Bounds are defined as
        /// upper left x, upper left y, lower right x, lower right y
        /// </summary>
        /// <returns></returns>
        public override Vector4 ReturnBounds()
        {
            return new Vector4(0 + BorderWidth,TopBarHeight, Width - BorderWidth, Height - BorderWidth);
        }

        /// <summary>
        /// Calculates the LocalBounds of the window by returning a Rect with coordinates relative
        /// to the LocalTransform. The override here is to provide support for the ErrorMargin.
        /// </summary>
        /// <returns></returns>
        public override Rect GetLocalBounds()
        {
            return new Rect(-ErrorMargin, 0, Width + ErrorMargin * 2, Height + ErrorMargin);
        }


    }
}
