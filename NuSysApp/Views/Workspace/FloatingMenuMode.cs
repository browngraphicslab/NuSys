using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class FloatingMenuMode : AbstractWorkspaceViewMode
    {
        public FloatingMenuMode(WorkspaceView view) : base(view) { }

        public override async Task Activate()
        {
            _view.IsDoubleTapEnabled = true;
            _view.DoubleTapped += OnDoubleTapped;

            _view.FloatingMenu.ManipulationMode = ManipulationModes.All;
            _view.FloatingMenu.ManipulationDelta += OnManipulationDelta;

        }

        public override async Task Deactivate()
        {
            _view.IsDoubleTapEnabled = false;
            _view.DoubleTapped -= OnDoubleTapped;

            _view.FloatingMenu.ManipulationMode = ManipulationModes.None;
            _view.FloatingMenu.ManipulationDelta -= OnManipulationDelta;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            var vm = (WorkspaceViewModel)dc;
            var compositeTransform = vm.FMTransform;

            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;

            e.Handled = true;
        }

        protected void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var dc = ((FrameworkElement)sender).DataContext;
            if (!(dc is WorkspaceViewModel))
            {
                e.Handled = true;
                return;
            }

            var vm = (WorkspaceViewModel)_view.DataContext;
            var floatingMenuTransform = new CompositeTransform();

            var p = e.GetPosition(_view);
            floatingMenuTransform.TranslateX = p.X;
            floatingMenuTransform.TranslateY = p.Y;
            vm.FMTransform = floatingMenuTransform;

            _view.FloatingMenu.Visibility = Visibility.Visible;

            e.Handled = true;
        }
    }
}
