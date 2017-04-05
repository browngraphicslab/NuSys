using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp.Components.NuSysRenderer.DetailView.TabAndPageFramework
{
    class TabPageUIElement : RectangleUIElement
    {
        public string Name
        {
            get; set;
        }

        public TabPageUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, string name) : base(parent, resourceCreator)
        {
            Name = name;
        }

        public TabPageUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, string name, RectangleUIElement content) : base(parent, resourceCreator)
        {
            Name = name;
            AddChild(content);
        }
    }
}
