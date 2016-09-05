using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class RenderItemInteractionManager : CanvasInteractionManager
    {
        private CanvasRenderEngine _renderEngine;

        public RenderItemInteractionManager(CanvasRenderEngine renderEngine, FrameworkElement pointerEventSource) : base(pointerEventSource)
        {
            _renderEngine = renderEngine;
            ItemTapped += OnItemTapped;
        }

        public override void Dispose()
        {
            ItemTapped -= OnItemTapped;
            _renderEngine = null;
            base.Dispose();
        }

        private void OnItemTapped(CanvasPointer pointer)
        {
            var hit = _renderEngine.GetRenderItemAt(pointer.CurrentPoint, _renderEngine.Root) as InteractiveBaseRenderItem;
            if (hit != null)
            {
                hit.OnTapped(pointer);
            }
        }
    }
}
