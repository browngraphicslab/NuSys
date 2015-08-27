using System.Threading.Tasks;

namespace NuSysApp.Views.Workspace
{
    public class LoadMode : AbstractWorkspaceViewMode
    {
        public LoadMode(WorkspaceView view) : base(view)
        {

        }

        public override async Task Activate()
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            await vm.LoadWorkspace();
        }

        public override async Task Deactivate()
        {

        }

    }
}