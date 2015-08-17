using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class AddNodeMode : AbstractWorkspaceViewMode
    {
        readonly NodeType _nodeType;

        public AddNodeMode(WorkspaceView view, NodeType nodeType) : base(view) {
            _nodeType = nodeType;
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
            await AddNode(_view, e.GetPosition(_view), _nodeType);
            e.Handled = true;
        }

        // This method is public because it's also used in CortanaMode.cs
        public static async Task AddNode(WorkspaceView view, Point pos, NodeType nodeType, object data = null) 
        {
            var vm = (WorkspaceViewModel)view.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(pos);
            await NetworkConnector.Instance.RequestMakeNode(p.X.ToString(), p.Y.ToString(), nodeType.ToString(), data==null ? null : data.ToString());
            vm.ClearSelection();
        }
    }
}
