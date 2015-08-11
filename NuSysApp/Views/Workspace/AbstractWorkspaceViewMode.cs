
namespace NuSysApp
{
    public abstract class AbstractWorkspaceViewMode
    {
        protected WorkspaceView _view;

        protected AbstractWorkspaceViewMode(WorkspaceView view)
        {
            _view = view;
        }

        public abstract void Activate();
        public abstract void Deactivate();
        
    }
}
