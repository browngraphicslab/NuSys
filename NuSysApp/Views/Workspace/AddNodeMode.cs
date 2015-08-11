using Windows.UI.Xaml.Input;

namespace NuSysApp.Views.Workspace
{
    public class AddNodeMode : AbstractWorkspaceViewMode
    {
        readonly NodeType _nodeType;

        public AddNodeMode(WorkspaceView view, NodeType nodeType) : base(view) {
            _nodeType = nodeType;
        }
        
        public override void Activate()
        {
            _view.IsRightTapEnabled = true;
            _view.RightTapped += OnRightTapped;
        }

        public override void Deactivate()
        {
            _view.IsRightTapEnabled = false;
            _view.RightTapped -= OnRightTapped;
        }

        private async void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(e.GetPosition(_view));

            await vm.CreateNewNode(_nodeType, p.X, p.Y, "");
            vm.ClearSelection();
            e.Handled = true;
        }
    }
}
