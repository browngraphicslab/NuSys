using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace NuSysApp
{
    public class CreateGroupMode : AbstractWorkspaceViewMode
    {
        private NodeManipulationMode _nodeManipulationMode;
        private DispatcherTimer _timer;
        private bool _isHovering;
        private NodeViewModel _hoveredNode;
        private IThumbnailable _hoveredNodeView;
        private string _createdGroupId;

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

        private async void UserControlOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs manipulationCompletedRoutedEventArgs)
        {

            if (!_isHovering)
                return;

            var id1 = (((FrameworkElement)sender).DataContext as NodeViewModel).Id;
            var id2 = _hoveredNode.Id;

            if (id1 == id2)
                return;

            var p = SessionController.Instance.ActiveWorkspace.CompositeTransform.Inverse.TransformPoint(
                            manipulationCompletedRoutedEventArgs.Position);
            p.X -= 150;
            p.Y -= 150;

          

            
            await SessionController.Instance.SaveThumb(id1, await ((IThumbnailable) sender).ToThumbnail(210, 100));
             await SessionController.Instance.SaveThumb(id2, await _hoveredNodeView.ToThumbnail(210, 100));

            var msg = new Message();
            msg["id1"] = id1;
            msg["id2"] = id2;
            msg["groupNodeId"] = SessionController.Instance.GenerateId();
            msg["width"] = 300;
            msg["height"] = 300;
            msg["x"] = p.X;
            msg["y"] = p.Y;

            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewGroupRequest(msg));
        }

        private void UserControlOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            var hits = VisualTreeHelper.FindElementsInHostCoordinates(args.Position,_view);
            var result = hits.Where(uiElem => (uiElem as FrameworkElement).DataContext is NodeViewModel && !((uiElem as FrameworkElement).DataContext == (sender as FrameworkElement).DataContext) && !((uiElem as FrameworkElement).DataContext is WorkspaceViewModel));
            var draggedItem = (AnimatableUserControl) sender;

            WorkspaceViewModel wvm = (WorkspaceViewModel)_view.DataContext;
            

            if (result.Any())
            {
                draggedItem.Opacity = 0.5;

                if (_timer == null)
                {
                    _timer = new DispatcherTimer();
                    _timer.Tick += delegate(object o, object o1)
                    {
                        _timer.Stop();
                        _isHovering = true;
                        _hoveredNode = (result.First() as FrameworkElement).DataContext as NodeViewModel;
                        _hoveredNodeView = wvm.AtomViewList.Where(v => v.DataContext == _hoveredNode).First() as IThumbnailable;

                    };
                    _timer.Interval = TimeSpan.FromMilliseconds(200);
                    _timer.Start();
                }
            }
            else
            {
                draggedItem.Opacity = 1;
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
