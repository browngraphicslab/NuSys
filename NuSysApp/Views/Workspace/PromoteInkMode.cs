using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp.Views.Workspace
{
    internal class PromoteInkMode : AbstractWorkspaceViewMode
    {

        private Dictionary<Polyline, InkStroke> _strokes;

        public PromoteInkMode(WorkspaceView view) : base(view)
        {
            
        }

        public override async Task Activate()
        {
            _strokes = _view.InqCanvas.Strokes;
            for (int i = 0; i < _strokes.Count; i++)
            {
                _strokes.ElementAt(i).Key.RightTapped += OnRightTapped;
            }
        }

        public override async Task Deactivate()
        {
            for (int i = 0; i < _strokes.Count; i++)
            {
                _strokes.ElementAt(i).Key.RightTapped -= OnRightTapped;
            }
        }

        private async void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Polyline line = sender as Polyline;
            var inkStroke = _view.InqCanvas.Strokes[line];
            _view.InqCanvas.Manager.SelectWithLine(inkStroke.GetInkPoints().First().Position, inkStroke.GetInkPoints().Last().Position);
            if (inkStroke.Selected)
            {
                _view.InqCanvas.Manager.CopySelectedToClipboard();
                _view.InqCanvas.Children.Remove(line);
                var vm = (WorkspaceViewModel)_view.DataContext;
                var p = vm.CompositeTransform.Inverse.TransformPoint(e.GetPosition(_view));
                Debug.WriteLine("click at " + p.X + ", " + p.Y);
                await vm.CreateNewNode(NodeType.Ink, p.X, p.Y, inkStroke);
            }
        }

    }
}
