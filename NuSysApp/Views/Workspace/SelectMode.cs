using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class SelectMode : AbstractWorkspaceViewMode
    {
        public SelectMode(WorkspaceView view) : base(view)
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            vm.MultiSelectEnabled = false;
        }

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
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dc is WorkspaceViewModel) { 
                var vm = (WorkspaceViewModel)_view.DataContext;
                vm.ClearSelection();
            }
            e.Handled = true;
        }

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dc is NodeViewModel)
            {
                var vm = (NodeViewModel)dc;
                vm.ToggleSelection();
            }

            e.Handled = true;
        }
    }
}
