using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Credentials.UI;
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
        private ElementViewModel _hoveredNode;
        private IThumbnailable _hoveredNodeView;
        private string _createdGroupId;

        public CreateGroupMode(FreeFormViewer view, NodeManipulationMode nodeManipulationMode) : base(view)
        {
            _nodeManipulationMode = nodeManipulationMode;
        }

        public async override Task Activate()
        {
            FreeFormViewerViewModel wvm = (FreeFormViewerViewModel)_view.DataContext;

            foreach (var userControl in wvm.AtomViewList.Where(s => s.DataContext is ElementViewModel))
            {
                userControl.ManipulationStarting += UserControlOnManipulationStarting;
                userControl.ManipulationDelta += UserControlOnManipulationDelta;
                userControl.ManipulationCompleted += UserControlOnManipulationCompleted;
            }
        }

        public async override Task Deactivate()
        {
            FreeFormViewerViewModel wvm = (FreeFormViewerViewModel)_view.DataContext;

            foreach (var userControl in wvm.AtomViewList.Where(s => s.DataContext is ElementViewModel))
            {
                userControl.ManipulationDelta -= UserControlOnManipulationDelta;
                userControl.ManipulationCompleted -= UserControlOnManipulationCompleted;
                userControl.ManipulationStarting -= UserControlOnManipulationStarting;
            }
        }
        private void UserControlOnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs manipulationStartingRoutedEventArgs)
        {
            manipulationStartingRoutedEventArgs.Container = _view;
        }

        private async void UserControlOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (!_isHovering)
                return;
            
            if (((FrameworkElement)sender).DataContext is AreaNodeViewModel)
                return;

            var id1 = (((FrameworkElement)sender).DataContext as ElementViewModel).Id;
            var id2 = _hoveredNode.Id;

   

            if (id1 == id2)
                return;

            var p = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(
                            e.Position);
            p.X -= 150;
            p.Y -= 150;

            
            await SessionController.Instance.SaveThumb(id1, await ((IThumbnailable) sender).ToThumbnail(210, 100));
            await SessionController.Instance.SaveThumb(id2, await ((IThumbnailable)sender).ToThumbnail(210, 100));

            var metadata = new Dictionary<string, object>();
            metadata["node_creation_date"] = DateTime.Now;

            var contentId = SessionController.Instance.GenerateId();
            var newElementId = SessionController.Instance.GenerateId();

            var elementMsg = new Message();
            elementMsg["metadata"] = metadata;
            elementMsg["width"] = 300;
            elementMsg["height"] = 300;
            elementMsg["x"] = p.X;
            elementMsg["y"] = p.Y;
            elementMsg["contentId"] = contentId;
            elementMsg["nodeType"] = ElementType.Collection;
            elementMsg["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
            elementMsg["id"] = newElementId;

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(elementMsg)); 

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewLibraryElementCollectionRequest(contentId,"",id1,id2,"NEW GROUP"));

           // await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(id1));
           // await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(id2));

            SessionController.Instance.IdToControllers[id1].Delete();
            SessionController.Instance.IdToControllers[id2].Delete();

            var m1 = new Message();
            m1["metadata"] = metadata;
            m1["contentId"] = SessionController.Instance.IdToControllers[id1].Model.ContentId;
            m1["nodeType"] = SessionController.Instance.IdToControllers[id1].Model.ElementType;
            m1["x"] = 0;
            m1["y"] = 0;
            m1["width"] = 200;
            m1["height"] = 200;
            m1["autoCreate"] = true;
            m1["creator"] = newElementId;
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m1));

            var m2 = new Message();
            m2["metadata"] = metadata;
            m2["contentId"] = SessionController.Instance.IdToControllers[id2].Model.ContentId;
            m2["nodeType"] = SessionController.Instance.IdToControllers[id2].Model.ElementType;
            m2["x"] = 0;
            m2["y"] = 0;
            m2["width"] = 200;
            m2["height"] = 200;
            m2["autoCreate"] = true;
            m2["creator"] = newElementId;
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m2));

        }

        private void UserControlOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var hits = VisualTreeHelper.FindElementsInHostCoordinates(e.Position, SessionController.Instance.SessionView);
            var result = hits.Where(uiElem => (uiElem as FrameworkElement).DataContext is ElementViewModel && !((uiElem as FrameworkElement).DataContext == (sender as FrameworkElement).DataContext) && !((uiElem as FrameworkElement).DataContext is FreeFormViewerViewModel) && !((uiElem as FrameworkElement).DataContext is AreaNodeViewModel));
            var draggedItem = (AnimatableUserControl) sender;   
            FreeFormViewerViewModel wvm = (FreeFormViewerViewModel)_view.DataContext;
            

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
                        _hoveredNode = (result.First() as FrameworkElement).DataContext as ElementViewModel;
                        Debug.WriteLine("atomviewlist count: " + wvm.AtomViewList.Count);
                        var atoms = wvm.AtomViewList.Where(v => v.DataContext == _hoveredNode);

                        if (atoms.Any())
                            _hoveredNodeView = atoms.First() as IThumbnailable;

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
    }
}
