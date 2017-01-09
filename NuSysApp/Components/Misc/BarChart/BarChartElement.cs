using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{

    public class BarChartElement : RectangleUIElement
    {
        public string Item
        {
            set; get;

        }
        public float Value
        { set; get; }
        public Color Color { get; set; }

        /// <summary>
        /// Determines whether BarChartElement should draw its own label. BarChartUIElement should be the one editing this
        /// </summary>
        public bool CanDrawLabel { set; get; }
        /// <summary>
        /// Determines whether this BarChartElement is selected. BarChartUIElement should be the one updating this.
        /// </summary>
        public bool IsSelected { set; get; }


        public BarChartElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            CanDrawLabel = false;
            IsSelected = false;
            Bordercolor = Constants.ALMOST_BLACK;
        }



        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (IsSelected)
            {
                BorderWidth = Math.Min(Width, 4);
            }
            else
            {
                BorderWidth = 0;
            }
            base.Update(parentLocalToScreenTransform);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (CanDrawLabel)
            {
                DrawText(ds);
            }

            base.Draw(ds);
        }


        private void DrawText(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            var p = new Vector2(Width / 2, Height + 20);
            var text = Item;
            ds.DrawText(
                text,
                p,
                Colors.Black,
                new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
                {
                    FontSize = 12,
                    HorizontalAlignment = CanvasHorizontalAlignment.Center
                });


            ds.Transform = orgTransform;

        }
    }
}