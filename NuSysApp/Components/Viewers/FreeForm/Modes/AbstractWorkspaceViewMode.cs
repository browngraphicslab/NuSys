
using System.Threading.Tasks;

namespace NuSysApp
{
    public abstract class AbstractWorkspaceViewMode
    {
        protected FreeFormViewer _view;

        protected AbstractWorkspaceViewMode(FreeFormViewer view)
        {
            _view = view;
        }

        public abstract Task Activate();
        public abstract Task Deactivate();
        
    }
}
