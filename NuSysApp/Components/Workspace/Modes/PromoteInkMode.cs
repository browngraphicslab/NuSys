using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    internal class PromoteInkMode : AbstractWorkspaceViewMode
    {

        public PromoteInkMode(WorkspaceView view) : base(view)
        {
            
        }

        public override async Task Activate()
        {
            var strokes = _view.InqCanvas.Children;
            foreach (var stroke in strokes)
            {
                if (stroke is InqLineView)
                {
                    stroke.RightTapped += OnRightTapped;
                }
            }
        }

        public override async Task Deactivate()
        {
            var strokes = _view.InqCanvas.Children;
            foreach(var stroke in strokes)
            {
                if (stroke is InqLineView)
                {
                    stroke.RightTapped -= OnRightTapped;
                }
            }
        }

        private async void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            _view.InqCanvas.Children.Remove(sender as InqLineView);
            var vm = (WorkspaceViewModel)_view.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(e.GetPosition(_view));
            InqLineView[] linesView = {sender as InqLineView};
            string plines = "";
            foreach (InqLineView pl in linesView)
            {
                if (pl.Points.Count > 0)
                {
                    plines += "<polyline points='";
                    foreach (Point point in pl.Points)
                    {
                        plines += Math.Floor(point.X) + "," + Math.Floor(point.Y) + ";";
                    }
                    plines += "' thickness='" + pl.StrokeThickness + "'/>";
                }
            }
            // TODO: Add to text node inqcanvas
            //await NetworkConnector.Instance.RequestMakeNode(p.X.ToString(), p.Y.ToString(), NodeType.Ink.ToString(),plines);
        }
    }
}
