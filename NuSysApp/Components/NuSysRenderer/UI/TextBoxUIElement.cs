using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class TextBoxUIElement : RectangleUIElement
    {
        private CanvasTextFormat _format;


        private CanvasTextLayout _textLayout;


        public TextBoxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            

        }

        public override Task Load()
        {
            _format = new CanvasTextFormat();
            _textLayout = new CanvasTextLayout(Canvas, "Hello World", _format, Width, Height);

            return base.Load();
        }

        public override void Draw(CanvasDrawingSession ds)
        {

            base.Draw(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            ds.DrawTextLayout(_textLayout, 0, 0, Colors.Black);

            ds.Transform = orgTransform;

        }
    }
}
