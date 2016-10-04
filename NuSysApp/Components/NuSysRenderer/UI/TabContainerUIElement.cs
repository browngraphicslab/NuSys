using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class TabContainerUIElement<T> : RectangleUIElement
    {
        private List<TabButtonUIElement<T>> tablist;

        public TabContainerUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }
    }
}
