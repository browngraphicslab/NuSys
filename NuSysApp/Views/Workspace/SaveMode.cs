using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class SaveMode : AbstractWorkspaceViewMode
    {
        public SaveMode(WorkspaceView view) : base(view)
        {

        }

        public override async Task Activate()
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            vm.LoadWorkspace();
        }

        public override async Task Deactivate()
        {

        }

    }
}