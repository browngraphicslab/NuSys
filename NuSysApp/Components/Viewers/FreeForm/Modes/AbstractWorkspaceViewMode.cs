
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public abstract class AbstractWorkspaceViewMode
    {
        protected FrameworkElement _view;

        protected AbstractWorkspaceViewMode(FrameworkElement view)
        {
            _view = view;
        }

       
            
        public abstract Task Activate();
        public abstract Task Deactivate();
        
    }
}
