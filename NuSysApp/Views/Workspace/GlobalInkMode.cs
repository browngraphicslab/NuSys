
using System.Threading.Tasks;

namespace NuSysApp.Views.Workspace
{
    public class GlobalInkMode : AbstractWorkspaceViewMode
    {

        public GlobalInkMode(WorkspaceView view) : base(view) { }

        public override async Task Activate()
        {
            _view.InqCanvas.IsEnabled = true;
        }

        public override async Task Deactivate()
        {
            _view.InqCanvas.IsEnabled = false;
        }
    }
}
