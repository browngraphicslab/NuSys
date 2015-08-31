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
            var vm = (WorkspaceViewModel)_view.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(e.GetPosition(_view));
            await NetworkConnector.Instance.RequestMakePin(p.X.ToString(), p.Y.ToString());
            e.Handled = true;
        }
    }
}
