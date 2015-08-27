using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Input;

namespace NuSysApp{
    public class PinMode : AbstractWorkspaceViewMode
    {
        public PinMode(WorkspaceView view) : base(view)
        {

        }
        public override async Task Activate()
        {
            _view.IsRightTapEnabled = true;
            _view.RightTapped += OnRightTapped;
        }

        public override async Task Deactivate()
        {
            _view.IsRightTapEnabled = false;
            _view.RightTapped -= OnRightTapped;
        }
        private async void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            await AddPin(_view, e.GetPosition(_view) );
            e.Handled = true;
        }

        private async Task AddPin(WorkspaceView _view, Point point)
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(point);
           await ((WorkspaceViewModel)_view.DataContext).AddNewPin(p.X, p.Y);
        }
    }
}
