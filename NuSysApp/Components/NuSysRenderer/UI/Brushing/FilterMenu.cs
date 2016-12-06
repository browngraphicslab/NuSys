using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class FilterMenu : ResizeableWindowUIElement
    {



        public FilterMenu(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set default ui values
            TopBarHeight = 0;
            Height = 500;
            Width = 300;
            MinWidth = 300;
            MinHeight = 300;
            BorderWidth = 3;
            Bordercolor = Colors.Black;

        }
    }
}
