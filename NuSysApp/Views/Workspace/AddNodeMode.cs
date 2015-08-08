using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Input;

namespace NuSysApp.Views.Workspace
{
    public class AddNodeMode : AbstractWorkspaceViewMode
    {
        NodeType _nodeType;

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



            /*
            if (vm.CurrentMode == WorkspaceViewModel.Mode.InkSelect)
            {
                int d = 20;

                var point1 = new Point(p.X - d, p.Y - d);
                var point2 = new Point(p.X + d, p.Y - d);
                var point3 = new Point(p.X + d, p.Y + d);
                var point4 = new Point(p.X - d, p.Y + d);


                var result = _view.InqCanvas.Manager.SelectWithLine(point1, point3);
                if (result.IsEmpty)
                {
                    result = _view.InqCanvas.Manager.SelectWithLine(point2, point4);
                }

                if (result.IsEmpty) { return; }

                foreach (var inkStroke in _view.InqCanvas.Manager.GetStrokes())
                {
                    if (inkStroke.Selected)
                    {
                        _view.InqCanvas.RemoveByInkStroke(inkStroke);
                    }
                }

                _view.InqCanvas.Manager.CopySelectedToClipboard();
                _view.InqCanvas.Manager.DeleteSelected();
                p.X = result.X;
                p.Y = result.Y;
                if (result.Width == 0 && result.Height == 0)
                {
                    return;
                }
            }

            if (vm.CurrentMode == WorkspaceViewModel.Mode.Pdf) return;
            */
   
        }
    }
}
