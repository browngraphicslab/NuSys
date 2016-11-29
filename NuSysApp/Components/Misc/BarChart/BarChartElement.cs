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



        public BarChartElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }

       

        public override void Dispose()
        {
            base.Dispose();
        }
        public override void Draw(CanvasDrawingSession ds)
        {
        
            DrawText(ds);
            base.Draw(ds);
        }

        private void DrawText(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            var p = new Vector2(0, 250);
            var text = Item;
            ds.DrawText(
                text,
                p,
                Colors.Black,
                new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
                {
                    FontSize = 12
                });


            ds.Transform = orgTransform;

            //var orgTransform = ds.Transform;
            //ds.Transform = Transform.LocalToScreenMatrix;

            //ds.Transform = orgTransform;
        }
    }
}
