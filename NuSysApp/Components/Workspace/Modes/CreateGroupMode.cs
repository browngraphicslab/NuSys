using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class CreateGroupMode : AbstractWorkspaceViewMode
    {
        private NodeManipulationMode _nodeManipulationMode;
        private DispatcherTimer _timer;
        private bool _isHovering;
        private NodeViewModel _hoveredNode;


        public CreateGroupMode(WorkspaceView view, NodeManipulationMode nodeManipulationMode) : base(view)
        {

            _nodeManipulationMode = nodeManipulationMode;
        }



        public async override Task Activate()
        {
            WorkspaceViewModel wvm = (WorkspaceViewModel)_view.DataContext;

            foreach (var userControl in wvm.Children.Values.Where(s => s.DataContext is NodeViewModel))
            {
                userControl.ManipulationDelta += UserControlOnManipulationDelta;
                userControl.ManipulationCompleted += UserControlOnManipulationCompleted;
            }
        }

        private void UserControlOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs manipulationCompletedRoutedEventArgs)
        {
            if (!_isHovering)
                return;
            Debug.WriteLine("Creating Group!");

            var p = SessionController.Instance.ActiveWorkspace.CompositeTransform.Inverse.TransformPoint(
                manipulationCompletedRoutedEventArgs.Position);

            var props = new Dictionary<string,object>();
            props["width"] = 200;
            props["height"] = 200;
            var id1 = (((UserControl) sender).DataContext as NodeViewModel).Id;
            var id2 = _hoveredNode.Id;



            NetworkConnector.Instance.RequestMakeGroup(id1, id2, p.X.ToString(), p.Y.ToString(), null, props);



           // NetworkConnector.Instance.RequestNewGroupTag(p.X.ToString(), p.Y.ToString(), "TItles", null);

        }

        private void UserControlOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            var hits = VisualTreeHelper.FindElementsInHostCoordinates(args.Position,_view);
            var result = hits.Where(uiElem => (uiElem as FrameworkElement).DataContext is NodeViewModel && !((uiElem as FrameworkElement).DataContext == (sender as FrameworkElement).DataContext) && !((uiElem as FrameworkElement).DataContext is WorkspaceViewModel));


            if (result.Any())
            {
                if (_timer == null)
                {
                    Debug.WriteLine("Creating timer");
                    _timer = new DispatcherTimer();
                    _timer.Tick += delegate(object o, object o1)
                    {
                        Debug.WriteLine("Give feedback!");
                        _timer.Stop();
                        _isHovering = true;
                        _hoveredNode = (result.First() as FrameworkElement).DataContext as NodeViewModel;
                    };
                    _timer.Interval = TimeSpan.FromSeconds(1);
                    _timer.Start();
                }
            }
            else
            {
                _timer?.Stop();
                _timer = null;
                _isHovering = false;
                _hoveredNode = null;
            }
        }


        public async override Task Deactivate()
        {
            WorkspaceViewModel wvm = (WorkspaceViewModel)_view.DataContext;

            foreach (var userControl in wvm.Children.Values.Where(s => s.DataContext is NodeViewModel))
            {
                userControl.ManipulationDelta -= UserControlOnManipulationDelta;
                userControl.ManipulationCompleted -= UserControlOnManipulationCompleted;
            }
        }
    }
}
