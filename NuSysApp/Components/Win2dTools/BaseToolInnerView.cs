using System.Collections.Generic;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public abstract class BaseToolInnerView : BaseRenderItem
    {
        public BaseToolInnerView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }

        public abstract void SetProperties(List<string> propertiesList);

        public abstract override void Dispose();

        public abstract void SetVisualSelection(HashSet<string> itemsToSelect);

    }
}