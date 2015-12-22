using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class SelectMode : AbstractWorkspaceViewMode
    {
        public SelectMode(WorkspaceView view) : base(view) { }

        public override async Task Activate()
        {
            _view.IsDoubleTapEnabled = true;
           _view.DoubleTapped += OnDoubleTapped;
            _view.PointerPressed += OnPointerPressed;

            _view.ManipulationMode = ManipulationModes.All;
        }

        public override async Task Deactivate()
        {
            _view.IsDoubleTapEnabled = false;
            _view.PointerPressed -= OnPointerPressed;
            _view.DoubleTapped -= OnDoubleTapped;

            _view.ManipulationMode = ManipulationModes.None;
  
            var vm = _view.DataContext as WorkspaceViewModel;
            vm.ClearSelection();
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
           
            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dc is WorkspaceViewModel) { 
                var vm = (WorkspaceViewModel)_view.DataContext;
                vm.ClearSelection();
            }
            else if (dc is NodeViewModel)
            {
                var vm = (NodeViewModel)dc;
                List<string> locks = new List<string>();
                locks.Add(vm.Id);
                NetworkConnector.Instance.CheckLocks(locks);
                NetworkConnector.Instance.RequestLock(vm.Id);
            }
          //  e.Handled = true;
        }

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dc is NodeViewModel)
            {
                var vm = (NodeViewModel)dc;
                SessionController.Instance.SessionView.ShowFullScreen((NodeModel)vm.Model);
               // vm.ToggleSelection();
                //e.Handled = true;
            }   
        }
    }
}
