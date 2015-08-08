using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.Views.Workspace
{
    public abstract class AbstractWorkspaceViewMode
    {
        protected WorkspaceView _view;

        public AbstractWorkspaceViewMode(WorkspaceView view)
        {
            _view = view;
        }

        public abstract void Activate();
        public abstract void Deactivate();
        
    }
}
