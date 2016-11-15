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
using SharpDX;

namespace NuSysApp
{
    public class RectangleUIElement : BaseInteractiveUIElement
    {
        public RectangleUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set default values
            Width = UIDefaults.Width;
            Height = UIDefaults.Height;
            Background = UIDefaults.Background;
            BorderWidth = UIDefaults.Borderwidth;
            Bordercolor = UIDefaults.Bordercolor;
        }

        /// <summary>
        /// The width of the rectangle
        /// </summary>
        private float _width;

        /// <summary>
        /// The width of the rectangle. Must be greater than or equal to zero.
        /// </summary>
        public override float Width
        {
            get { return _width; }
            set
            {
                Debug.Assert(value >= 0);
                _width = value >= 0 ? value : 0;
            }
        }

        internal void Update(SharpDX.Matrix3x2 parentLocalToScreenTransform)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The image to be displayed on the rectangle
        /// </summary>
        public ICanvasImage Image { get; set; }

        /// <summary>
        /// The height of the rectangle
        /// </summary>
        private float _height;
        /// <summary>
        /// The height of the rectangle. Must be greater than or equal to zero.
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
        /// The background of the rectangle
        /// </summary>
        public override Windows.UI.Color Background { get; set; }

        /// <summary>
        /// The width of the border of the rectangle
        /// </summary>
        private float _borderWidth;

        /// <summary>
        /// The Width of the Border of the Rectangle. Extends into the Rectangle.
        /// Must be greater than or equal to zero.
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
        /// The BorderColor of the Rectangle
        /// </summary>
        public override Windows.UI.Color Bordercolor { get; set; }


        /// <summary>
        /// Draws the background and the border
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            // return if the item is disposed or is not visible
            if (IsDisposed || !IsVisible)
                return;
                            
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            // draw the background of the rectangle
            DrawBackground(ds);

            DrawImage(ds);

            // draw the border in the rectangle
            DrawBorder(ds);

            ds.Transform = orgTransform;

            base.Draw(ds);
        }

        /// <summary>
        /// Draws the border inside the Rectangle UIElement
        /// </summary>
        /// <param name="ds"></param>
        protected override void DrawBorder(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            // draw the border inside the rectangle
            ds.DrawRectangle(new Rect(BorderWidth / 2, BorderWidth / 2, Width - BorderWidth, Height - BorderWidth), Bordercolor, BorderWidth);

            ds.Transform = orgTransform;
        }

        /// <summary>
        /// Draws the image on the Rectangle UIElement
        /// </summary>
        /// <param name="ds"></param>
        public virtual void DrawImage(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            if (Image != null)
            {
                ds.DrawImage(Image, GetLocalBounds(), Image.GetBounds(Canvas));
            }

            ds.Transform = orgTransform;
        }

        /// <summary>
        /// Draws the background of the UI Eelents
        /// </summary>
        /// <param name="ds"></param>
        protected override void DrawBackground(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            // draw the background of the rectangle
            ds.FillRectangle(new Rect(0, 0, Width, Height), Background);

            ds.Transform = orgTransform;
        }

        /// <summary>
        /// Returns the LocalBounds of the base render item, used for hit testing. The bounds are given with the offset
        /// of the local matrix assumed to be zero. If the matrix is offset, then the local bounds must be offset accordingly
        /// </summary>
        /// <returns></returns>
        public override Rect GetLocalBounds()
        {
            return new Rect(0, 0, Width, Height);
        }

    }
}
