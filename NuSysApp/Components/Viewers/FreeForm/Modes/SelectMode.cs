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
        public SelectMode(FreeFormViewer view) : base(view) { }

        private static ElementViewModel _selectedElementVm;
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
  
            var vm = _view.DataContext as FreeFormViewerViewModel;
            vm.ClearSelection();
        }

        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _released = false;
            await Task.Delay(100);
            if (!_released)
                return;

            _selectedElementVm?.SetSelected(false);

            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dc == _selectedElementVm)
            {
                _selectedElementVm = null;
                return;
            }


            if (dc is FreeFormViewerViewModel)
            {
                var vm = (FreeFormViewerViewModel) _view.DataContext;
                vm.ClearSelection();
                _selectedElementVm = null;
            }
            else if (dc is ElementViewModel)
            {
                var vm = (ElementViewModel) dc;
                _selectedElementVm = vm;
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
            if ((dc is ElementViewModel || dc is LinkViewModel) && !(dc is FreeFormViewerViewModel) )
            {
                if (dc is ElementViewModel)
                {
                    var vm = (ElementViewModel)dc;

                    if (vm.ElementType == ElementType.Word || vm.ElementType == ElementType.Powerpoint)
                    {
                        SessionController.Instance.SessionView.OpenFile(vm);
                    }
                    else
                    {
                        SessionController.Instance.SessionView.ShowFullScreen((ElementModel)vm.Model);
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
