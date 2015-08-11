
namespace NuSysApp.Views.Workspace
{
    public class GlobalInkMode : AbstractWorkspaceViewMode
    {

        public GlobalInkMode(WorkspaceView view) : base(view) { }

        public override void Activate()
        {
            _view.InqCanvas.IsEnabled = true;
        }

        public override void Deactivate()
        {
            _view.InqCanvas.IsEnabled = false;
        }
    }
}
