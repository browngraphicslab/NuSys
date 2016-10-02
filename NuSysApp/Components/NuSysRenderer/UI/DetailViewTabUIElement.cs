using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class DetailViewTabUIElement : RectangleUIElement
    {

        private string _libraryElementId;
        public DetailViewPageType CurrentPage { get; set; }

        public enum DetailViewPageType { Home, Metadata, Link, Region}

        public DetailViewTabUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }
    }
}
