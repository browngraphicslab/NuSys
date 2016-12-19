using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.UI;

namespace NuSysApp
{

    public class BarChartUIElement : RectangleUIElement
    {
        public List<Color> Palette { set; get; }

        public BarChartUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Palette = new List<Color>(new[] { Colors.DarkSalmon, Colors.Azure, Colors.LemonChiffon, Colors.Honeydew, Colors.Pink });
            
        }

        public void AddElement()
        {

        }

        public override void Draw(CanvasDrawingSession ds)
        {
            DrawBars(ds);
            base.Draw(ds);
        }

        private void DrawBars(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;



            ds.Transform = orgTransform;

        }
    }
}
