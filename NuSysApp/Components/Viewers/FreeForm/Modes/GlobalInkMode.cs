
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GlobalInkMode : AbstractWorkspaceViewMode
    {
        private FreeFormViewer _cview;

        public GlobalInkMode(FreeFormViewer view) : base(view)
        {
            _cview = (FreeFormViewer)view;
        }

        public override async Task Activate()
        {
            _cview.InqCanvas.IsEnabled = true;
            
        }

        public override async Task Deactivate()
        {
            _cview.InqCanvas.IsEnabled = false;
        }
    }
}
