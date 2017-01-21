﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace NuSysApp
{
    public class RoundedRectangleUIElement : BaseInteractiveUIElement
    {
        public RoundedRectangleUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
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
        public override Color Background { get; set; }

        /// <summary>
        /// The width of the border of the rectangle.
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
        public override Color BorderColor { get; set; }

        /// <summary>
        /// The image to display on the rounded rectangle
        /// </summary>
        public override ICanvasImage Image { get; set; }

        /// <summary>
        /// The radius of the corner of the Rectangle
        /// </summary>
        private float _radius;

        /// <summary>
        /// The radius of the corner of the Rectangle. Must be greater than or equal to zero.
        /// </summary>
        public float Radius
        {
            get { return _radius; }
            set
            {
                Debug.Assert(value >= 0);
                _radius = value >= 0 ? value : 0;
            }
        }

        public override BorderType BorderType { set; get; } = UIDefaults.BorderType;


        /// <summary>
        /// Draws the rectangle onto the canvas drawing session
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
            if (BorderType == BorderType.Inside)
            {
                // draw the border inside the rectangle
                ds.DrawRoundedRectangle(
                    new Rect(BorderWidth/2, BorderWidth/2, Width - BorderWidth, Height - BorderWidth), Radius, Radius,
                    BorderColor);
            }
            else
            {
                // draw the border outside the rectangle
                ds.DrawRoundedRectangle(new Rect(-BorderWidth / 2, -BorderWidth / 2, Width + BorderWidth, Height + BorderWidth), Radius, Radius, BorderColor);

            }

            ds.Transform = orgTransform;
        }

        /// <summary>
        /// Draw the background of the Rectangleu UI Eleemnt
        /// </summary>
        /// <param name="ds"></param>
        protected override void DrawBackground(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            ds.FillRoundedRectangle(new Rect(BorderWidth / 2, BorderWidth / 2, Width - BorderWidth, Height - BorderWidth), Radius, Radius, Background);

            ds.Transform = orgTransform;
        }

        protected override void DrawImage(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            if (Image != null)
            {
                using (
                    ds.CreateLayer(1,
                        CanvasGeometry.CreateRoundedRectangle(Canvas, BorderWidth/2, BorderWidth/2, Width - BorderWidth,
                            Height - BorderWidth, Radius, Radius)))
                {
                    ds.DrawImage(Image, GetImageBounds() ?? GetLocalBounds(), Image.GetBounds(Canvas));
                }
            }

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

        /// <summary>
        /// The bounds in normal coordinates within which to draw the image
        /// </summary>
        public override Rect? ImageBounds { get; set; }


        /// <summary>
        /// gets the bounds in local coordinates within which to draw the image, if no bounds have been set returns null
        /// </summary>
        /// <returns></returns>
        public override Rect? GetImageBounds()
        {
            if (ImageBounds == null)
            {
                return null;
            }
            return new Rect(Width * ImageBounds.Value.Left, Height * ImageBounds.Value.Top, Width * ImageBounds.Value.Width, Height * ImageBounds.Value.Height);
        }
    }
}
