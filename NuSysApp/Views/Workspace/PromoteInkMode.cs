using System;
using System.Collections.Generic;
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

        private HashSet<Polyline> _strokes;

        public PromoteInkMode(WorkspaceView view) : base(view)
        {
            
        }

        public override async Task Activate()
        {
            _strokes = _view.InqCanvas.Strokes;
            for (int i = 0; i < _strokes.Count; i++)
            {
                _strokes.ElementAt(i).RightTapped += OnRightTapped;
            }
        }

        public override async Task Deactivate()
        {
            for (int i = 0; i < _strokes.Count; i++)
            {
                _strokes.ElementAt(i).RightTapped -= OnRightTapped;
            }
        }

        private async void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            _view.InqCanvas.Children.Remove(sender as Polyline);
            _strokes.Remove(sender as Polyline);
            var vm = (WorkspaceViewModel)_view.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(e.GetPosition(_view));
            Debug.WriteLine("click at " + p.X + ", " + p.Y);
            Polyline[] lines = {sender as Polyline};
            string plines = "";
            foreach (Polyline pl in lines)
            {
                plines += "<polyline points='";
                foreach (Point point in pl.Points)
                {
                    plines += point.X.ToString() + "," + point.Y.ToString() + ";";
                }
                plines += "' thickness='"+pl.StrokeThickness+"' stroke='"+pl.Stroke.ToString()+"'/>";
            }
            await NetworkConnector.Instance.RequestMakeNode(p.X.ToString(), p.Y.ToString(), NodeType.Ink.ToString(),plines);
        }
    }
}
