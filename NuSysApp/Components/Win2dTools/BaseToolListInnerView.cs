using System.Collections.Generic;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class BaseToolListInnerView : BaseToolInnerView
    {
        
        public BaseToolListInnerView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }

        public override void SetProperties(List<string> propertiesList)
        {
            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public override void SetVisualSelection(HashSet<string> itemsToSelect)
        {
            throw new System.NotImplementedException();
        }
    }
}