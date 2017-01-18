using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class MarkdownTester : RectangleUIElement
    {
        public MarkdownTester(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Background = Colors.Transparent;

            var st = new ScrollableTextboxUIElement(this, ResourceCreator, true, true)
            {
                Width = 300,
                Height = 300
            };
            AddChild(st);

            var dt = new DynamicTextboxUIElement(this, ResourceCreator)
            {
                Width = 300
            };
            AddChild(dt);

            var mct = new MarkdownConvertingTextbox(this, ResourceCreator)
            {
                Width = 300,
                Height = 300
            };
            AddChild(mct);

            st.Transform.LocalPosition = new Vector2(300, 300);
            dt.Transform.LocalPosition = new Vector2(300 + st.Width, 300);
            mct.Transform.LocalPosition = new Vector2(300 + st.Width + dt.Width, 300);


            st.TextChanged += delegate (InteractiveBaseRenderItem item, string text)
            {
                dt.Text = CommonMark.CommonMarkConverter.Convert(text);
                mct.UpdateMarkdown(text);
            };

        }
    }
}
