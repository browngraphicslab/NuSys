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
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class RectangleUIElement : BaseInteractiveUIElement
    {

        public bool DropShadow { get; set; }

        public float DropShadowHeight { get; set; }

        public float DropShadowWidth { get; set; }

        public float DropShadowXOffset { get; set; }

        public float DropShadowYOffset { get; set; }

        public Color DropShadowColor { get; set; }

        private CanvasRenderTarget _dryStrokesTarget;




        public RectangleUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set default values
            Width = UIDefaults.Width;
            Height = UIDefaults.Height;
            Background = UIDefaults.Background;
            BorderWidth = UIDefaults.Borderwidth;
            BorderColor = UIDefaults.Bordercolor;
            BorderType = UIDefaults.BorderType;
            DropShadowColor = UIDefaults.DropShadowColor;

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
        /// The image to be displayed on the rectangle
        /// </summary>
        public override ICanvasImage Image { get; set; }

        public override BorderType BorderType { get; set; }

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
        /// The bounds to draw the image in the rectangle, these are normalized coordinates
        /// </summary>
        public override Rect? ImageBounds { get; set; }

        /// <summary>
        /// The BorderColor of the Rectangle
        /// </summary>
        public override Color BorderColor { get; set; }

        /// <summary>
        /// Draws the background and the border
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            // return if the item is disposed or is not visible
            if (IsDisposed || !IsVisible)
            {
                return;
            }

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            //  ds.Transform = Matrix3x2.Identity;

            if (DropShadow)
            {
                DrawDropShadow(ds);
            }

            // draw the background of the rectangle
            DrawBackground(ds);

            // draw the image over the background
            DrawImage(ds);

            // draw text used by elements which inherit from this
            DrawText(ds);

            // draw the border in the rectangle
            DrawBorder(ds);

            ds.Transform = orgTransform;

            base.Draw(ds);
        }

        /// <summary>
        /// Not implemented in the rectangleuielement
        /// but used by classes which inherit from this
        /// </summary>
        /// <param name="ds"></param>
        protected virtual void DrawText(CanvasDrawingSession ds){}



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
                ds.DrawRectangle(new Rect(BorderWidth / 2, BorderWidth / 2, Math.Max(Width - BorderWidth, 0), Math.Max(Height - BorderWidth,0)), BorderColor, BorderWidth);
            }else if (BorderType == BorderType.Outside)
            {
                // draw the border outside the rectangle
                ds.DrawRectangle(new Rect(-BorderWidth/2, -BorderWidth/2, Width + BorderWidth, Height + BorderWidth), BorderColor, BorderWidth);

            }

            ds.Transform = orgTransform;
        }

        /// <summary>
        /// Draws the image on the Rectangle UIElement
        /// </summary>
        /// <param name="ds"></param>
        protected override void DrawImage(CanvasDrawingSession ds)
        {
            if (Image != null)
            {
                var orgTransform = ds.Transform;
                ds.Transform = Transform.LocalToScreenMatrix;

                using (ds.CreateLayer(1, CanvasGeometry.CreateRectangle(ResourceCreator, GetLocalBounds())))
                {
                    ds.DrawImage(Image, GetImageBounds() ?? GetLocalBounds(), Image.GetBounds(ResourceCreator));
                }

                ds.Transform = orgTransform;
            }
        }

        /// <summary>
        /// Draws the background of the UI Elements
        /// </summary>
        /// <param name="ds"></param>
        protected override void DrawBackground(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            if (BorderType == BorderType.Inside||BorderType == BorderType.Outside)
            {
                //draw the background of the rectangle
                ds.FillRectangle(0, 0, Width, Height, Background);

            }
            ds.Transform = orgTransform;
        }


        private async void DrawDropShadow(CanvasDrawingSession ds)
        {
            //DropShadowXOffset = 150;
            //int change = 2;
            //var alphaGradientBrush = new CanvasRadialGradientBrush(ResourceCreator, Colors.Black, Colors.Transparent)
            //{
            //    Center = new Vector2(DropShadowXOffset + DropShadowWidth/2, DropShadowYOffset + DropShadowHeight / 2),

            //    RadiusX = DropShadowWidth,
            //    RadiusY = DropShadowHeight
            //};

            //Color newColor = Color.FromArgb(10, DropShadowColor.R, DropShadowColor.G, DropShadowColor.B);
            //ds.FillRectangle(DropShadowXOffset, DropShadowYOffset, DropShadowWidth, DropShadowHeight, alphaGradientBrush);
            //double greaterOffset = DropShadowXOffset > DropShadowYOffset ? DropShadowXOffset : DropShadowYOffset;
            //for (int i = 0; i < greaterOffset; i++)
            //{
            //    ds.FillRectangle(DropShadowXOffset, DropShadowYOffset, DropShadowWidth - i, DropShadowHeight - i, newColor);
            //}

            var orgTransform = ds.Transform;
            if (_dryStrokesTarget != null)
            {
                using (var dss = _dryStrokesTarget.CreateDrawingSession())
                {
                    dss.Clear(Colors.Transparent);
                    dss.Transform = ds.Transform;
                    //dss.DrawImage(_bmp, new Rect(DropShadowXOffset, DropShadowYOffset, DropShadowWidth, DropShadowHeight));
                    dss.FillRectangle(DropShadowXOffset, DropShadowYOffset, DropShadowWidth, DropShadowHeight,
                        Colors.Black);

                    //var blur = new GaussianBlurEffect();
                    //blur.Source = _dryStrokesTarget;
                    //blur.BlurAmount = 3.0f;

                    ds.Transform = Matrix3x2.Identity;
                    ds.DrawImage(_dryStrokesTarget);
                }

            }
            else
            {
                var canvas = (CanvasAnimatedControl)ResourceCreator;
                _dryStrokesTarget = new CanvasRenderTarget(ResourceCreator, canvas.Size);
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
        /// Gets the boudns in local coordinates to the draw the rectangle's image in
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
