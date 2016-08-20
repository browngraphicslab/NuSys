using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MyToolkit.Utilities;
using NusysIntermediate;

namespace NuSysApp
{
    public class SelectMode : AbstractWorkspaceViewMode
    {

        private bool _released;
        private bool _doubleTapped;
        private PointerEventHandler _pointerPressedHandler;
        private PointerEventHandler _pointerReleasedHandler;
        private DoubleTappedEventHandler _doubleTappedHandler;

        /// <summary>
        /// mapping of pointerIds to SessionView Positions for relatedDocumentsGesture
        /// </summary>
        private Dictionary<uint, Point> _pointerIdToStartLocation;

        /// <summary>
        /// These are the lines that will be removed once the jesus hold has been released
        /// </summary>
        private List<RelevanceLineView> _relevanceLineViewsToRemove; 


        /// <summary>
        /// A List of possible element types that relatedDocumentsGesture can find
        /// </summary>
        private readonly List<Type> _possibleElements = new List<Type>
        {
            typeof(ImageNodeView),
            typeof(PdfNodeView),
            typeof(GroupNodeView),
            typeof(AudioNodeView),
            typeof(VideoNodeView),
        };

        public SelectMode(FreeFormViewer view):base(view)
        {
            // instantiated the _pointerIdToStartLocation dictionary and the _relevanceLineViewsToRemove
            _pointerIdToStartLocation = new Dictionary<uint, Point>();
            _relevanceLineViewsToRemove = new List<RelevanceLineView>();


            // add the mode specific event handlers
            _pointerPressedHandler = OnPointerPressed;
            _pointerReleasedHandler = OnPointerReleased;
            _doubleTappedHandler = OnDoubleTapped;
        }

        public SelectMode(AreaNodeView view) : base(view)
        {
            _pointerPressedHandler = OnPointerPressed;
            _pointerReleasedHandler = OnPointerReleased;
            _doubleTappedHandler = OnDoubleTapped;
        }
        public override async Task Activate()
        {
            _view.IsDoubleTapEnabled = true;

            _view.ManipulationMode = ManipulationModes.All;

            _view.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, false );
            _view.AddHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler, false );
            _view.AddHandler(UIElement.DoubleTappedEvent, _doubleTappedHandler, false );
        }

        public override async Task Deactivate()
        {
            _view.IsDoubleTapEnabled = false;

            _view.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);
            _view.RemoveHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler);
            _view.RemoveHandler(UIElement.DoubleTappedEvent, _doubleTappedHandler);

            _view.ManipulationMode = ManipulationModes.None;
  
        //    var vm = _view.DataContext as FreeFormViewerViewModel;
        //    vm.ClearSelection();
        }

        /// <summary>
        /// Called every time the user presses anywhere on the workspace while in SelectMode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        { 
            // Add the pointer to the mapping of pointerIds to SessionView Positions
            _pointerIdToStartLocation.Add(e.Pointer.PointerId,
                e.GetCurrentPoint(SessionController.Instance.SessionView).Position);

            // if there are five pointers in contact with the screen
            if (_pointerIdToStartLocation.Count >=5)
            {
                // get the list of point locations
                var points = _pointerIdToStartLocation.Values;
                // 
                // calculate the minimum bounding rect
                var minBoundingRect = new Rect(new Point(points.Min(point => point.X), points.Min(point => point.Y)), new Point(points.Max(point => point.X), points.Max(point => point.Y)));
                if (minBoundingRect.Width < 400 && minBoundingRect.Height < 400) // 400 px is slightly smaller than the avg American hand size according to Sahil
                {
                    GetRelatedElements(minBoundingRect);
                }
            }

            if (SessionController.Instance.SessionView.FreeFormViewer.MultiMenu.Visibility == Visibility.Visible)
            {
                return;
            }

            _released = false;
            await Task.Delay(200);
            if (!_released)
                return;

            await Task.Delay(50);
            if (_doubleTapped)
            {
                _doubleTapped = false;
                return;
            }

            var dc = ((FrameworkElement)e.OriginalSource).DataContext as ISelectable;
            if (dc == null)
            {
                return;
            }

            var viwerVm = _view.DataContext as FreeFormViewerViewModel;
            var isCtrlDown = (CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control) &
                                CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

            if (!isCtrlDown)
            {


                if (dc == viwerVm)
                {
                    viwerVm?.ClearSelection();
                    return;
                }

                viwerVm?.ClearSelection();
                viwerVm?.AddSelection(dc);
            }
            else
            {
                if (dc is FreeFormViewerViewModel)
                {
                    return;
                }

                if (dc.IsSelected)
                {
                    viwerVm?.RemoveSelection(dc);
                }
                else
                {
                    viwerVm?.AddSelection(dc);
                }
            }
        }

        /// <summary>
        /// Takes in a rectangle and performs Junsu's Algorithm if there is only one element view model contained within in
        /// </summary>
        /// <param name="rect"></param>
        private void GetRelatedElements(Rect rect)
        {
            // returns a list of all the xaml stuff that is contained in the rectangle
            var xamlElements = VisualTreeHelper.FindElementsInHostCoordinates(rect, null);

            // get a list of views which the relatedDocumentsGesture can comprehend
            var possibleElements = xamlElements.Where(uiElem =>
                                _possibleElements.Contains(uiElem.GetType())).ToList();
            if (possibleElements.Count == 1)
            {
                var elementViewModel = (possibleElements[0] as FrameworkElement).DataContext as ElementViewModel;
                if (elementViewModel != null)
                {
                    UITask.Run(async delegate {
                        MakeRelevanceLines(elementViewModel.Controller);
                    });
                }
            }
        }

        private IEnumerable<string> GetStrings(IEnumerable<Keyword> words)
        {
            return words?.Select(w => w?.Text?.ToLower() ?? "") ?? new List<string>();
        }

        /// <summary>
        /// make the lines of relevance from one node to its five most relevant nodes
        /// </summary>
        /// <param name="vm"></param>
        private async Task MakeRelevanceLines(ElementController controller)
        {
            //get list of node's relevant documents
            //foreach document in the list as long as the relevance is above .25, make a relevance line for it

            var keywordsToCompare = GetStrings(controller.LibraryElementModel.Keywords ?? new HashSet<Keyword>());

            var count = (controller.LibraryElementModel.Keywords ?? new HashSet<Keyword>()).Count();

            var viewModels = (SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(item => item?.DataContext is ElementViewModel)).Select(fe => fe.DataContext as ElementViewModel).ToImmutableHashSet();

            var dict = viewModels.ToDictionary(vm => vm, 
                item => ((double)GetStrings(item.Controller?.LibraryElementModel?.Keywords ??
                   new HashSet<Keyword>()).Intersect(keywordsToCompare).Count())/(double)count);

            foreach (var kvp in dict)
            {
                var line = new RelevanceLineView(controller.Model, kvp.Key.Controller.Model, kvp.Value);
                SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Add(line);
            }

            Task.Run(async delegate
            {
                if (controller.LibraryElementController.LibraryElementModel.Type == NusysConstants.ElementType.PDF)
                {
                    var request = new GetAnalysisModelRequest(controller.LibraryElementController.LibraryElementModel.ContentDataModelId);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                    var analysisModel = request.GetReturnedAnalysisModel() as NusysPdfAnalysisModel;
                }
            });

        }


        /// <summary>
        /// Called when the user releases their pointer from the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            // set released to true, used for code which ignores accidental pointer pressed events
            _released = true;
            // remove the Pointer from the mapping of pointerIds to start locations
            _pointerIdToStartLocation.Remove(e.Pointer.PointerId);
            if (_pointerIdToStartLocation.Count < 5)
            {
                foreach (var relevanceLine in new List<RelevanceLineView>(_relevanceLineViewsToRemove))
                {
                    SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Remove(relevanceLine);
                    _relevanceLineViewsToRemove.Remove(relevanceLine);
                }
            }
        }

        /// <summary>
        /// Pass in the relevance lines that need to be removed when you release your jesus hold
        /// </summary>
        /// <param name="relevanceLines"></param>
        public void PrepareToRemoveRelevanceLines(IEnumerable<RelevanceLineView> relevanceLines)
        {
            _relevanceLineViewsToRemove.AddRange(relevanceLines);
        }

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _doubleTapped = true;
            var dc = (e.OriginalSource as FrameworkElement)?.DataContext;
            if ((dc is ElementViewModel || dc is LinkViewModel) && !(dc is FreeFormViewerViewModel) )
            {
                if (dc is ElementViewModel)
                {
                    var vm = dc as ElementViewModel;              

                    if (vm.ElementType == NusysConstants.ElementType.Powerpoint)
                    {
                        SessionController.Instance.SessionView.OpenFile(vm);
                    }
                    else if (vm.ElementType != NusysConstants.ElementType.Link)
                    {

                        if (vm.ElementType == NusysConstants.ElementType.PDF || vm.ElementType == NusysConstants.ElementType.PdfRegion)
                        {
                            var pdfVm = (PdfNodeViewModel)vm;
                            PdfDetailHomeTabViewModel.InitialPageNumber = pdfVm.CurrentPageNumber;
                        }

                        SessionController.Instance.SessionView.ShowDetailView(vm.Controller.LibraryElementController);
                    }

                }
            }   
        }
    }
}
