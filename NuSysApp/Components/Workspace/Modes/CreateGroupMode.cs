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

            var p = SessionController.Instance.ActiveWorkspace.CompositeTransform.Inverse.TransformPoint(
                            manipulationCompletedRoutedEventArgs.Position);
            p.X -= 150;
            p.Y -= 150;

            var props = new Dictionary<string, object>();
            props["width"] = 300;
            props["height"] = 300;



            var id1 = (((FrameworkElement)sender).DataContext as NodeViewModel).Id;
            var id2 = _hoveredNode.Id;



            SessionController.Instance.SaveThumb(id1, await ((IThumbnailable) sender).ToThumbnail(210, 100));
            SessionController.Instance.SaveThumb(id2, await _hoveredNodeView.ToThumbnail(210, 100));

            var callback = new Action<string>(async (s) =>
            {
                _createdGroupId = s;

                var wvm = _view.DataContext as WorkspaceViewModel;
                var found = wvm.AtomViewList.Where(a => (a.DataContext as AtomViewModel).Id == s);

                var node1 = SessionController.Instance.IdToSendables[id1];
                var node2 = SessionController.Instance.IdToSendables[id2];

                NodeContainerModel groupModel;
                if (!found.Any())
                {
                    groupModel = (NodeContainerModel) node2;
                    await groupModel.AddChild(node1);
                    wvm.RemoveChild(node1.Id);
                }
                else
                {
                    groupModel = (NodeContainerModel) SessionController.Instance.IdToSendables[s];

                    await groupModel.AddChild(node1);
                    wvm.RemoveChild(node1.Id);
                    await groupModel.AddChild(node2);
                    wvm.RemoveChild(node2.Id);
                }
                
                if (!found.Any())
                    return;

                var groupView = found.First() as AnimatableUserControl;
                groupView.RenderTransformOrigin = new Point(0.5, 0.5);       

                Anim.FromTo(groupView, "Alpha", 0, 1, 600, new BackEase());
                Anim.FromTo(groupView, "ScaleY", 0, 1, 600, new BackEase());
                Anim.FromTo(groupView, "ScaleX", 0, 1, 600, new BackEase());

            });
            //NetworkConnector.Instance.RequestMakeGroup(id1, id2, p.X.ToString(), p.Y.ToString(), null, props, callback);

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
                    Debug.WriteLine("Creating timer");
                    _timer = new DispatcherTimer();
                    _timer.Tick += delegate(object o, object o1)
                    {
                        Debug.WriteLine("Give feedback!");
                        _timer.Stop();
                        _isHovering = true;
                        _hoveredNode = (result.First() as FrameworkElement).DataContext as NodeViewModel;
                        _hoveredNodeView = wvm.AtomViewList.Where(v => v.DataContext == _hoveredNode).First() as IThumbnailable;

                    };
                    _timer.Interval = TimeSpan.FromMilliseconds(400);
                    _timer.Start();
                }
            }
            else
            {
                if (_createdGroupId != null)
                {
                   // SessionController.Instance.IdToSendables[_createdGroupId]
                }
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
