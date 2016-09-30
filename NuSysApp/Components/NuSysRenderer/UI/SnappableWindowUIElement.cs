using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using NuSysApp.Components.NuSysRenderer.UI;

namespace NuSysApp
{
    class SnappableWindowUIElement : ResizeableWindowUIElement
    {
        public SnappableWindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }
    }
}
