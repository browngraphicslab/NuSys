
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GlobalInkMode : AbstractWorkspaceViewMode
    {

        public GlobalInkMode(WorkspaceView view) : base(view) { }

        public override async Task Activate()
        {
            // TODO: delegate to workspaceview
//            _view.InqCanvas.IsEnabled = true;
        }

        public override async Task Deactivate()
        {
            // TODO: delegate to workspaceview

//            _view.InqCanvas.IsEnabled = false;
        }
    }
}
