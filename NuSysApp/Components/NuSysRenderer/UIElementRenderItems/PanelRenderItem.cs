using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class PanelRenderItem : UIElementRenderItem
    {
        public PanelRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, double width, double height)
            : base(parent, resourceCreator, width, height)
        {
            
        }

        /// <summary>
        /// draw rectangle panel
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);
        }

    }
}
