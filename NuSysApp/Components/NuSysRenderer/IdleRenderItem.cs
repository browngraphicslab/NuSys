using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class IdleRenderItem : ElementRenderItem
    {
        public IdleRenderItem(ElementViewModel vm, CollectionRenderItem parent, CanvasAnimatedControl resourceCreator) : base(vm, parent, resourceCreator)
        {
        }
    }
}
