using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class DetailViewUIElement : SnappableWindowUIElement
    {
        public DetailViewUIElement(RectangleUIElement parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }


    }
}
