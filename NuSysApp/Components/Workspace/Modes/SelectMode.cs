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

        private static AtomViewModel _selectedAtomVm;
        private bool _released;

        public override async Task Activate()
        {
            _view.IsDoubleTapEnabled = true;
            _view.DoubleTapped += OnDoubleTapped;
            _view.PointerPressed += OnPointerPressed;
            _view.PointerReleased += OnPointerReleased;
            _view.ManipulationMode = ManipulationModes.All;
        }

        public override async Task Deactivate()
        {
            _view.IsDoubleTapEnabled = false;
            _view.PointerPressed -= OnPointerPressed;
            _view.DoubleTapped -= OnDoubleTapped;
            _view.PointerReleased -= OnPointerReleased;

            _view.ManipulationMode = ManipulationModes.None;
  
            var vm = _view.DataContext as WorkspaceViewModel;
            vm.ClearSelection();
        }

        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _released = false;
            await Task.Delay(100);
            if (!_released)
                return;

            _selectedAtomVm?.SetSelected(false);

            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dc == _selectedAtomVm)
            {
                _selectedAtomVm = null;
                return;
            }


            if (dc is WorkspaceViewModel)
            {
                var vm = (WorkspaceViewModel) _view.DataContext;
                vm.ClearSelection();
                _selectedAtomVm = null;
            }
            else if (dc is AtomViewModel)
            {
                var vm = (AtomViewModel) dc;
                _selectedAtomVm = vm;
                vm.SetSelected(true);
            }
        }

        private async void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _released = true;
        }

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            if ((dc is NodeViewModel || dc is LinkViewModel) && !(dc is WorkspaceViewModel) )
            {
                if (dc is NodeViewModel)
                {
                    var vm = (NodeViewModel)dc;

                    if (vm.NodeType == NodeType.Word || vm.NodeType == NodeType.Powerpoint)
                    {
                        SessionController.Instance.SessionView.OpenFile(vm);
                    }
                    else
                    {

                        SessionController.Instance.SessionView.ShowFullScreen((NodeModel)vm.Model);
                    }

                }
                if (dc is LinkViewModel)
                {
                    var vm = (LinkViewModel)dc;
                    SessionController.Instance.SessionView.ShowFullScreen((LinkModel)vm.Model);
                }
               // vm.ToggleSelection();
                //e.Handled = true;
            }   
        }
    }
}
