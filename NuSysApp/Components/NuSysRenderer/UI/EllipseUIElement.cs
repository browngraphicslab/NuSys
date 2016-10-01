using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class EllipseUIElement : BaseInteractiveUIElement
    {
        public EllipseUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }

        /// <summary>
        /// The X Radius of the ellipse
        /// </summary>
        private float _width;

        /// <summary>
        /// The X radius of the ellipse. Must be greater than or equal to 0.
        /// </summary>
        public override float Width {
            get { return _width; }
            set
            {
                Debug.Assert(value >= 0);
                _width = value >= 0 ? value : 0;
            }
        }

        /// <summary>
        /// The Y Radius of the ellipse
        /// </summary>
        private float _height;

        /// <summary>
        /// The Y Radius of the ellipse. Must be greater than or equal to 0.
        /// </summary>
        public override float Height
        {
            get { return _height; }
            set
            {
                Debug.Assert(value >= 0);
                _height = value >= 0 ? value : 0;
            }
        }
        /// <summary>
        /// The background color of the ellipse
        /// </summary>
        public override Color Background { get; set; }

        /// <summary>
        /// The width of the border of the ellipse.
        /// </summary>
        private float _borderWidth;

        /// <summary>
        /// The width of the border of the ellipse. Extends into the ellipse.
        /// Must be greater than or equal to 0.
        /// </summary>
        public override float BorderWidth
        {
            get { return _borderWidth; }
            set
            {
                Debug.Assert(value >= 0);
                _borderWidth = value >= 0 ? value : 0;
            }
        }
        /// <summary>
        /// The color of the border of the ellipse
        /// </summary>
        public override Color Bordercolor { get; set; }

        /// <summary>
        /// The center of the ellipse
        /// </summary>
        private Vector2 CenterPoint => new Vector2(Width/2, Height/2);

        /// <summary>
        /// Draws the ellipse onto the canvas drawing session
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            // return if the item is disposed or is not visible
            if (IsDisposed || !IsVisible)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            // draw the background of the ellipse
            ds.FillEllipse(CenterPoint, Width, Height, Background);

            // draw the border in the ellipse
            DrawBorder(ds);

            ds.Transform = orgTransform;

            base.Draw(ds);


        }

        /// <summary>
        /// Draws the border in the Ellipse
        /// </summary>
        /// <param name="ds"></param>
        protected override void DrawBorder(CanvasDrawingSession ds)
        {
            ds.DrawEllipse(CenterPoint, Width - BorderWidth / 2, Height - BorderWidth / 2, Bordercolor, BorderWidth);
        }

        /// <summary>
        /// The initializer method for the ellipse
        /// </summary>
        /// <returns></returns>
        public override Task Load()
        {
            // shift the ellipse by its width and height so its center point is draw on the upper
            // let corner of the parent initially
            Transform.LocalPosition -= new Vector2(Width, Height);
            return base.Load();
        }

        /// <summary>
        /// The Initial Offset of the Ellipses center point from the parent's upper left corner. 
        /// Offsets the cetner point from the upper left corner of the screen if the parent is null.
        /// </summary>
        public override Vector2 InitialOffset { get; set; }
    }
}
