using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class InteractiveBaseRenderItem : BaseRenderItem
    {

        public InteractiveBaseRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            
        }
        public virtual void OnPressed(CanvasPointer pointer)
        {

        }

        public virtual void OnReleased(CanvasPointer pointer)
        {

        }

        public virtual void OnDoubleTapped(CanvasPointer pointer)
        {

        }

        public virtual void OnTapped(CanvasPointer pointer)
        {
            
        }

        public virtual void OnDragged(CanvasPointer pointer)
        {
            
        }
    }
}
