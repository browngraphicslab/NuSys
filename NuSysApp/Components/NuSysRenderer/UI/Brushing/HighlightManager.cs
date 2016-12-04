using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.Components.NuSysRenderer.UI.Brushing
{
    class HighlightManager
    {
        public void ApplyHighlight()
        {
            var renderItems =
                SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.GetChildren()
                    .OfType<ElementRenderItem>();
        }

    }
}
