using System.Threading.Tasks;

namespace NuSysApp.Views.Workspace
{
    public class SaveMode : AbstractWorkspaceViewMode
    {
        public SaveMode(WorkspaceView view) : base(view)
        {

        }

        public override async Task Activate()
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            vm.SaveWorkspace();
        }

        public override async Task Deactivate()
        {

        }

    }
}