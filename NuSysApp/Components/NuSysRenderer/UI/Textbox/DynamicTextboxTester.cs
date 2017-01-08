using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp.Components.NuSysRenderer.UI.Textbox
{
    public class DynamicTextboxTester : RectangleUIElement
    {
        public DynamicTextboxTester(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            var dynamicTextbox = new DynamicTextboxUIElement(this, resourceCreator);
            dynamicTextbox.Transform.LocalPosition = new Vector2(0);
            dynamicTextbox.Width = 300;

            var textboxInput = new ScrollableTextboxUIElement(this, Canvas, true, true)
            {
                Width = 300,
                Height = 300,
                Background = Colors.Azure
            };
            textboxInput.Transform.LocalPosition = new Vector2(0, 300);
            textboxInput.TextChanged += delegate (InteractiveBaseRenderItem item, string text)
            {
                dynamicTextbox.Text = text;
            };

            AddChild(dynamicTextbox);
            AddChild(textboxInput);
        }
    }
}
