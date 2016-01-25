
using System.Threading.Tasks;

namespace NuSysApp
{
    public abstract class AbstractWorkspaceViewMode
    {
        protected WorkspaceView _view;

        protected AbstractWorkspaceViewMode(WorkspaceView view)
        {
            _view = view;
        }

        public abstract Task Activate();
        public abstract Task Deactivate();
        
    }
}
