using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;

namespace NuSysApp
{
    public class GradientBackgroundRectangleUIElement : RectangleUIElement
    {

        /// <summary>
        /// The list of background gradients used to draw the gradient background of the rectangle
        /// </summary>
        public List<CanvasGradientStop> BackgroundGradients { get; set; }

        /// <summary>
        /// The direction of the gradient, starts from the first location ends at the second location
        /// </summary>
        public enum GradientDirection { TopBottom, LeftRight, RightLeft, BottomTop, UpperLeftLowerRight, UpperRightLowerLeft, LowerRightUpperLeft, LowerLeftUpperRight}

        /// <summary>
        /// The direction of the gradient
        /// </summary>
        public GradientDirection Direction { get; set; }

        /// <summary>
        /// The type of gradient used
        /// </summary>
        public enum GradientType { Linear, Radial}


        /// <summary>
        /// The type of the gradient
        /// </summary>
        public GradientType Type { get; set; }

        public GradientBackgroundRectangleUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }

        /// <summary>
        /// Draws the background of the UI Elements
        /// </summary>
        /// <param name="ds"></param>
        protected override void DrawBackground(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            // draw the background of the rectangle
            if (Type == GradientType.Linear)
            {
                ds.FillRectangle(new Rect(0, 0, Width, Height), CreateCanvasLinearGradientBrush());
            }
            else
            {
                ds.FillRectangle(new Rect(0, 0, Width, Height), CreateCanvasRadialGradientBrush());
            }

            ds.Transform = orgTransform;
        }

        private CanvasRadialGradientBrush CreateCanvasRadialGradientBrush()
        {
            var brush = new CanvasRadialGradientBrush(Canvas, BackgroundGradients.ToArray());
            var diagonalDistance = (float) Math.Sqrt(Math.Pow(Width, 2) + Math.Pow(Height, 2));
            switch (Direction)
            {
                case GradientDirection.TopBottom:
                    brush.Center = new Vector2(Width / 2, 0);
                    brush.RadiusX = Height;
                    brush.RadiusY = Height;
                    break;
                case GradientDirection.LeftRight:
                    brush.Center = new Vector2(0, Height / 2);
                    brush.RadiusX = Width;
                    brush.RadiusY = Width;
                    break;
                case GradientDirection.RightLeft:
                    brush.Center = new Vector2(Width, Height / 2);
                    break;
                case GradientDirection.BottomTop:
                    brush.Center = new Vector2(Width / 2, Height);
                    brush.RadiusX = Height;
                    brush.RadiusY = Height;
                    break;
                case GradientDirection.UpperLeftLowerRight:
                    brush.Center = new Vector2(0);
                    brush.RadiusX = diagonalDistance;
                    brush.RadiusY = diagonalDistance;
                    break;
                case GradientDirection.UpperRightLowerLeft:
                    brush.Center = new Vector2(Width, 0);
                    brush.RadiusX = diagonalDistance;
                    brush.RadiusY = diagonalDistance;
                    break;
                case GradientDirection.LowerRightUpperLeft:
                    brush.Center = new Vector2(Width, Height);
                    brush.RadiusX = diagonalDistance;
                    brush.RadiusY = diagonalDistance;
                    break;
                case GradientDirection.LowerLeftUpperRight:
                    brush.Center = new Vector2(0, Height);
                    brush.RadiusX = diagonalDistance;
                    brush.RadiusY = diagonalDistance;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return brush;

        }

        private CanvasLinearGradientBrush CreateCanvasLinearGradientBrush()
        {
            var brush = new CanvasLinearGradientBrush(Canvas, BackgroundGradients.ToArray());
            switch (Direction)
            {
                case GradientDirection.TopBottom:
                    brush.StartPoint = new Vector2(Width/2, 0);
                    brush.EndPoint = new Vector2(Width/2, Height);
                    break;
                case GradientDirection.LeftRight:
                    brush.StartPoint = new Vector2(0, Height/2);
                    brush.EndPoint = new Vector2(Width, Height / 2);
                    break;
                case GradientDirection.RightLeft:
                    brush.EndPoint = new Vector2(0, Height / 2);
                    brush.StartPoint = new Vector2(Width, Height / 2);
                    break;
                case GradientDirection.BottomTop:
                    brush.EndPoint = new Vector2(Width / 2, 0);
                    brush.StartPoint = new Vector2(Width / 2, Height);
                    break;
                case GradientDirection.UpperLeftLowerRight:
                    brush.StartPoint = new Vector2(0);
                    brush.EndPoint = new Vector2(Width, Height);
                    break;
                case GradientDirection.UpperRightLowerLeft:
                    brush.StartPoint = new Vector2(Width, 0);
                    brush.EndPoint = new Vector2(0, Height);
                    break;
                case GradientDirection.LowerRightUpperLeft:
                    brush.StartPoint = new Vector2(Width, Height);
                    brush.EndPoint = new Vector2(0);
                    break;
                case GradientDirection.LowerLeftUpperRight:
                    brush.StartPoint = new Vector2(0, Height);
                    brush.EndPoint = new Vector2(Width, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return brush;
        }
    }
}
