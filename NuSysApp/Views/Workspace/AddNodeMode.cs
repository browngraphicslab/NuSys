using System.Threading.Tasks;
using Windows.Foundation;
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
            await AddNode(e.GetPosition(_view), _nodeType);
            e.Handled = true;
        }

        /// <summary>
        /// Further abstraction to be used by Cortana to manually add nodes
        /// </summary>
        /// <param name="pos">The position to place the node</param>
        private async Task AddNode(Point pos, NodeType nodeType) 
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(pos);
            await vm.CreateNewNode(nodeType, p.X, p.Y, "");
            vm.ClearSelection();
        }
    }
}
