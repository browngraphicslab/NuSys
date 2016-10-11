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

namespace NuSysApp
{
    public class RectangleUIElement : BaseInteractiveUIElement
    {
        public RectangleUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
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
        public override Color Bordercolor { get; set; }

        /// <summary>
        /// the initializer method add event handlers here
        /// </summary>
        /// <returns></returns>
        public override Task Load()
        {
            Transform.LocalPosition += InitialOffset;

            return base.Load();
        }


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
            ds.FillRectangle(new Rect(0, 0, Width, Height), Background);

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
            // draw the border inside the rectangle
            ds.DrawRectangle(new Rect(BorderWidth / 2, BorderWidth / 2, Width - BorderWidth, Height - BorderWidth), Bordercolor, BorderWidth);

        }

        /// <summary>
        /// The InitialOffset of the Upper Left Corner of the Rectangle
        /// From the parentss upper left corner. Offsets from the UpperLeft corner of
        /// the screen if the parent is null.
        /// </summary>
        public override Vector2 InitialOffset { get; set; }

        /// <summary>
        /// Returns the bounds of the rectangle in which objects can be contained.
        /// The vector 4 has upper left x, upper left y, lower right x, lower right y
        /// </summary>
        /// <returns></returns>
        public override Vector4 ReturnBounds()
        {
            return new Vector4(BorderWidth,BorderWidth,Width - BorderWidth, Height - BorderWidth);
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
