using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;


namespace NuSysApp
{
    public class CreateGroupMode : AbstractWorkspaceViewMode
    {
        private DispatcherTimer _timer;
        private bool _isHovering;
        private ElementViewModel _hoveredNode;
        private string _createdGroupId;

        public CreateGroupMode(FrameworkElement view) : base(view)
        {
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

            wvm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
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

            wvm.AtomViewList.CollectionChanged -= AtomViewListOnCollectionChanged;
        }



        private void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            var newItems = notifyCollectionChangedEventArgs.NewItems;
            if (newItems == null)
                return;

            var newNodes = newItems;
            foreach (var n in newNodes)
            {
                var userControl = (UserControl)n;
                if (userControl.DataContext is ElementViewModel)
                {
                    userControl.ManipulationMode = ManipulationModes.All;
                    userControl.ManipulationDelta += UserControlOnManipulationDelta;
                    userControl.ManipulationStarting += UserControlOnManipulationStarting;
                    userControl.ManipulationCompleted += UserControlOnManipulationCompleted;
                }
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

            var draggedItem = (((FrameworkElement) sender).DataContext as ElementViewModel);

            if (draggedItem is LinkViewModel)
                return;

            var id1 = draggedItem.Id;
            var id2 = _hoveredNode.Id;
            if (_hoveredNode.IsEditing)//TODO FIX?
            {
                return; //makes sure you don't add a node to a group that it is already in when in simple edit mode
            }

            if (id1 == id2)
                return;

            var p = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(
                            e.Position);
            p.X -= 150;
            p.Y -= 150;

            var controller1 = SessionController.Instance.IdToControllers[id1];
            var controller2 = SessionController.Instance.IdToControllers[id2];

            var c1IsCollection = controller1 is ElementCollectionController;
            var c2IsCollection = controller2 is ElementCollectionController;
           

            var metadata = new Dictionary<string, object>();
            metadata["node_creation_date"] = DateTime.Now;
            // TODO: remove temp
            Random rnd = new Random();
            metadata["random_id"] = rnd.Next(1, 1000);
            metadata["random_id2"] = rnd.Next(1, 100);

            if (!(c1IsCollection || c2IsCollection)) { 
                var contentId = SessionController.Instance.GenerateId();
                var newCollectionId = SessionController.Instance.GenerateId();

                var elementMsg = new Message();
                elementMsg["metadata"] = metadata;
                elementMsg["width"] = 300;
                elementMsg["height"] = 300;
                elementMsg["x"] = p.X;
                elementMsg["y"] = p.Y;
                elementMsg["contentId"] = contentId;
                elementMsg["nodeType"] = ElementType.Collection;
                elementMsg["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
                elementMsg["id"] = newCollectionId;

                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, "", ElementType.Collection, "New Collection"));

                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new SubscribeToCollectionRequest(contentId));

                //await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(elementMsg)); 

                var controller = await StaticServerCalls.PutCollectionInstanceOnMainCollection(p.X, p.Y, contentId, 300, 300, newCollectionId);

                await controller2.RequestMoveToCollection(contentId);
                await controller1.RequestMoveToCollection(contentId);


                _isHovering = false;
                return;
            }

            if (c2IsCollection)
            {
                var x = draggedItem.Transform.TranslateX;
                var y = draggedItem.Transform.TranslateY;
                var point = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.TransformPoint(new Point(x, y));
                var model = (ElementModel)_hoveredNode.Model;
                var modelX = model.X;
                var modelY = model.Y;
                var gX = modelX - point.X;
                var gY = modelY - point.Y;

                var point2 = _hoveredNode.Transform.TransformPoint(point);

                await controller1.RequestMoveToCollection(controller2.Model.LibraryId, point2.X, point2.Y);
                return;
            }

            if (c1IsCollection)
            {
                await controller2.RequestMoveToCollection(controller1.Model.LibraryId);
            }
 
         

            _isHovering = false;

        }

        private FrameworkElement _hoveredView;
        private void UserControlOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var draggedItem = (AnimatableUserControl)e.OriginalSource;
            var hits = VisualTreeHelper.FindElementsInHostCoordinates(e.Position, SessionController.Instance.SessionView);
            var result = hits.Where((uiElem) =>
            {
                var fe = (FrameworkElement) uiElem;
                var r = fe.DataContext is ElementViewModel &&
                        fe.DataContext != SessionController.Instance.ActiveFreeFormViewer &&
                        !(fe.DataContext is LinkViewModel) &&
                        draggedItem.DataContext != fe.DataContext && 
                        (draggedItem.DataContext as ElementViewModel).Model != (fe.DataContext as ElementViewModel).Model;
                return r;
            });

            if (result.Any())
            {
               
                _isHovering = true;
                _hoveredView  = (FrameworkElement)result.First();
                _hoveredNode = (ElementViewModel)_hoveredView.DataContext;
                draggedItem.Opacity = 0.5;
            }
            else
            {
                draggedItem.Opacity = 1;
                _timer?.Stop();
                _timer = null;
                _isHovering = false;
                _hoveredNode = null;
                _hoveredView = null;
            }


            /*

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
                */
        }
    }
}
