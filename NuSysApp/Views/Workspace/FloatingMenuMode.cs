using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp.Views.Workspace
{
    public class FloatingMenuMode : AbstractWorkspaceViewMode
    {
        public FloatingMenuMode(WorkspaceView view) : base(view) { }

        public override async Task Activate()
        {
            _view.IsDoubleTapEnabled = true;
            _view.DoubleTapped += OnDoubleTapped;
        }

        public override async Task Deactivate()
        {
            _view.IsDoubleTapEnabled = false;
            _view.DoubleTapped -= OnDoubleTapped;
        }

        protected void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
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

            _view.FloatingMenu.Visibility = _view.FloatingMenu.Visibility == Visibility.Collapsed
                ? Visibility.Visible
                : Visibility.Collapsed;

            e.Handled = true;
        }
    }
}
