using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using NusysIntermediate;


namespace NuSysApp
{
    public class CreateGroupMode : AbstractWorkspaceViewMode
    {
        private DispatcherTimer _timer;
        private bool _isHovering;
        /// <summary>
        /// The element view model we are hovering over
        /// </summary>
        private ElementViewModel _hoveredNode;
        private string _createdGroupId;

        private UndoButton _moveToCollectionUndoButton;

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

            _hoveredNode = null;
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

        /// <summary>
        /// Fired when the user lets go of a UserControl i.e. Node/Element, if the node is hovering over another noe
        /// this creates a collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UserControlOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            // if we are not over a node or the node is null then return, we cannot create a collection
            if (_hoveredNode == null || !_isHovering)
            {
                return;
            }

            // set the opacity of the item we are dragging back to 1 
            var draggedView = sender as FrameworkElement;
            Debug.Assert(draggedView != null, "This should always be a framework element");
            draggedView.Opacity = 1;

            // get the draggedItem as an ElementViewModel
            var draggedItem = draggedView.DataContext as ElementViewModel;

            // get the ids of the dragged and hover item
            var draggedId = draggedItem.Id;
            var hoveredId = _hoveredNode.Id;
            if (_hoveredNode.IsEditing)//TODO FIX?. I think this is fine, would really only occur in concurrent workspaces, and in that case is desired
            {
                return; //makes sure you don't add a node to a group that it is already in when in simple edit mode
            }

            // dont add an item to itself
            if (draggedId == hoveredId) //todo this probably isn't possible because elementIds should be unique? i think
            {
                return;
            }

            // get the point on the workspace where we want to add the element
            var p = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(e.Position);
            p.X -= 150;
            p.Y -= 150;//TODO not have this heere, factor out to half on a constantly-defined default width and height

            if (!SessionController.Instance.IdToControllers.ContainsKey(draggedId))
            {
                return;
            }

            if (!SessionController.Instance.IdToControllers.ContainsKey(hoveredId))
            {
                return;
            }

            // get the element controllers from the ids
            var draggedController = SessionController.Instance.IdToControllers[draggedId];
            var hoveredController = SessionController.Instance.IdToControllers[hoveredId];

            var draggedIsCollection = draggedController is ElementCollectionController;
            var hoverIsCollection = hoveredController is ElementCollectionController;
           
            // if neither element is a collection
            if (!(draggedIsCollection || hoverIsCollection)) { 
                var contentId = SessionController.Instance.GenerateId();
                var newCollectionId = SessionController.Instance.GenerateId();

                draggedView.Visibility = Visibility.Collapsed;
                _hoveredView.Visibility  = Visibility.Collapsed;

                var newContentRequestArgs = new CreateNewContentRequestArgs()
                {
                    LibraryElementArgs = new CreateNewLibraryElementRequestArgs()
                    {
                        LibraryElementId = newCollectionId,
                        AccessType = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType,
                        Title = "New Collection",
                        LibraryElementType = NusysConstants.ElementType.Collection
                    },
                    ContentId =  contentId
                };
                foreach(var preslink in PresentationLinkViewModel.Models.ToList())
                {
                    if (draggedId == preslink.InElementId || draggedId == preslink.OutElementId ||
                        hoveredId == preslink.InElementId || hoveredId == preslink.OutElementId)
                    {
                        var request = new DeletePresentationLinkRequest(preslink.LinkId);
                    //    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                    //    request.DeletePresentationLinkFromLibrary();
                    }
                }
                var newCollectionRequest = new CreateNewContentRequest(newContentRequestArgs);//create and execute request fro new collection content and libraryElement
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(newCollectionRequest);
                newCollectionRequest.AddReturnedLibraryElementToLibrary();

                var controller = SessionController.Instance.ContentController.GetLibraryElementController(newCollectionId);
                controller.AddElementAtPosition(p.X, p.Y);

                await hoveredController.RequestMoveToCollection(newCollectionId);
                await draggedController.RequestMoveToCollection(newCollectionId);

                _isHovering = false;
                return;
            }
            // if either element is a collection
            if (hoverIsCollection || draggedIsCollection)
            {

                var collection = (hoverIsCollection ? hoveredController : draggedController) as ElementCollectionController;
                var elementToBeAdded = (hoverIsCollection ? draggedController : hoveredController);
                var parentVm = (FreeFormViewerViewModel) _view.DataContext;
                var found  = parentVm.AtomViewList.Where(a => (a.DataContext as ElementViewModel)?.Controller == collection);
                if (!found.Any() || !(found.First() is GroupNodeView))
                    return;
                var groupnode = (GroupNodeView)found.First();

                Point targetPoint;

                if (groupnode.FreeFormView != null)
                {
                    var np =new Point(e.Position.X - hoveredController.Model.Width/2, e.Position.Y - hoveredController.Model.Height/2);
                    var canvas = groupnode.FreeFormView.AtomContainer;
                    targetPoint = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(canvas).TransformPoint(e.Position);
                    targetPoint.X -= draggedController.Model.Width/2;
                    targetPoint.Y -= draggedController.Model.Height/2;

                }
                else
                {
                    targetPoint = new Point(50000,50000);
                }

                var oldCollection = elementToBeAdded.GetParentCollectionId();
                var newCollection = collection.Model.LibraryId;
                //Give it the location of the collection + some offset so that the undo button is not hidden by the collection
                var oldLocation = new Point2d(collection.Model.X - 50, collection.Model.Y);
                var newLocation = new Point2d(targetPoint.X, targetPoint.Y);

                await elementToBeAdded.RequestMoveToCollection(collection.Model.LibraryId, targetPoint.X, targetPoint.Y);


                //Instantiates a MoveToCollectionAction that describes the action that just occurred.
                var action = new MoveToCollectionAction(elementToBeAdded.Id, oldCollection,
                    newCollection, oldLocation, newLocation);
                //Create UndoButton that will reverse the movetocollection action just occured. 
                _moveToCollectionUndoButton = new UndoButton();
                //Moves the undobutton to the old position and activates it.
                parentVm.AtomViewList.Add(_moveToCollectionUndoButton);
                _moveToCollectionUndoButton.MoveTo(oldLocation);
                _moveToCollectionUndoButton.Activate(action);


                _isHovering = false;
            }

        }

        private FrameworkElement _hoveredView;
        private void UserControlOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var draggedItem = (FrameworkElement)e.OriginalSource;
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
