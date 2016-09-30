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
    class WindowUIElement : RectangleUIElement
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

        public WindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // todo remove this line it is for testing
            Transform.LocalPosition += new Vector2((float) (SessionController.Instance.ScreenWidth / 4), (float) (SessionController.Instance.ScreenHeight/ 4));
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
    }
}
