using System;
using System.Collections;
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
using Windows.UI.Xaml.Media.Animation;

namespace NuSysApp
{
    public class NodeManipulationMode : AbstractWorkspaceViewMode
    {
        public static int _zIndexCounter = 10000;

        private bool _isPinAnimating;
        private bool _isFreeForm;
        private FrameworkElement _bounds;
        private Point _originalPosition;
        private Point _newPosition;
        private UndoButton _moveNodeUndoButton;



        public List<UserControl> ActiveNodes { get; private set; }
        public bool Limited { get; set; }
        private FreeFormViewer _viewer;
        public NodeManipulationMode(FrameworkElement view) : base(view) { }

    
        public NodeManipulationMode(FrameworkElement view, bool isFreeFormCollection) : base(view)
        {
            _isFreeForm = isFreeFormCollection;

            _originalPosition = new Point(0,0);
            _newPosition = new Point(0,0);
            _moveNodeUndoButton = new UndoButton();

        }

        public void SetViewer(FreeFormViewer viewer)
        {
            _viewer = viewer;
        }
        public override async Task Activate()
        {   
            ActiveNodes = new List<UserControl>();
            var vm = (FreeFormViewerViewModel)_view.DataContext;
            foreach (var userControl in vm.AtomViewList)
            {

                if (!(userControl is UndoButton))
                {
                    userControl.ManipulationMode = ManipulationModes.All;
                    userControl.ManipulationStarted += ManipulationStarting;
                    userControl.ManipulationDelta += OnManipulationDelta;
                    userControl.ManipulationCompleted += OnManipulationCompleted;
                    userControl.ManipulationInertiaStarting += OnManipulationIntertiaStarting;
                }
            }

            vm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
        }

        /// <summary>
        /// When finished moving node, calculat original and new position in global space, then
        /// create an Undo action based on those positions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="manipulationCompletedRoutedEventArgs"></param>
        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs manipulationCompletedRoutedEventArgs)
        {
            //Updates new position coordinates
            _newPosition.X = manipulationCompletedRoutedEventArgs.Position.X;
            _newPosition.Y = manipulationCompletedRoutedEventArgs.Position.Y;

            // Transforms original and new positions from screen to global space
            _newPosition = (SessionController.Instance.ActiveFreeFormViewer.CompositeTransform).Inverse.TransformPoint(
                _newPosition);
            _originalPosition = (SessionController.Instance.ActiveFreeFormViewer.CompositeTransform).Inverse.TransformPoint(
                _originalPosition);
            
             ActiveNodes.Remove((UserControl)sender);
            
            //Disposes of pointer released event needed for move undo button
            var userControl = (UserControl)sender;
            //userControl.PointerReleased -= UserControl_PointerReleased;
           
            manipulationCompletedRoutedEventArgs.Handled = true;
        
        }

        private void ManipulationStarting(object sender, ManipulationStartedRoutedEventArgs manipulationStartingRoutedEventArgs)
        {
            if(sender is UndoButton)
            {
                return;
            }
            var userControl = (UserControl)sender;
            if (userControl.DataContext is ElementViewModel && !(userControl.DataContext is LinkViewModel))
            {
                Canvas.SetZIndex(userControl, _zIndexCounter++);
            }

            _originalPosition.X = manipulationStartingRoutedEventArgs.Position.X;
            _originalPosition.Y = manipulationStartingRoutedEventArgs.Position.Y;

            ActiveNodes.Add((UserControl)sender);

            //If an action had been done and a new manipulation has started then we want to then make sure that it doesn't interfere with our current manipulation
            if (_moveNodeUndoButton != null)
            {
                if (_moveNodeUndoButton.ActionExecuted == true)
                {
                    _moveNodeUndoButton.ActionExecuted = false; // This prevents the node from being immovable right agter being undo'd
                }

                var ffvm = (FreeFormViewerViewModel)_view.DataContext;
                _moveNodeUndoButton.Deactivate();
                if (ffvm.AtomViewList.Contains(_moveNodeUndoButton))
                {
                    ffvm.AtomViewList.Remove(_moveNodeUndoButton);

                }
            }
        }

        private void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            var newItems = notifyCollectionChangedEventArgs.NewItems;
            if (newItems == null)
                return;

            var newNodes = newItems;
            foreach (var n in newNodes)
            {
                var userControl = (FrameworkElement) n;
                if (userControl.DataContext is ElementViewModel) { 
                    userControl.ManipulationMode = ManipulationModes.All;
                    userControl.ManipulationDelta += OnManipulationDelta;
                    userControl.ManipulationStarted += ManipulationStarting;
                    userControl.ManipulationCompleted += OnManipulationCompleted;
                    if (!(userControl is UndoButton))
                    {
                        userControl.ManipulationInertiaStarting += OnManipulationIntertiaStarting;
                    }
                }
            }
        }


        public override async Task Deactivate()
        {
            var vm = (FreeFormViewerViewModel)_view.DataContext;
            foreach (var userControl in vm.AtomViewList)
            {
                userControl.ManipulationMode = ManipulationModes.All;
                userControl.ManipulationDelta -= OnManipulationDelta;
                userControl.ManipulationStarted -= ManipulationStarting;
                userControl.ManipulationCompleted -= OnManipulationCompleted;
                userControl.ManipulationInertiaStarting -= OnManipulationIntertiaStarting;
            }

            vm.AtomViewList.CollectionChanged -= AtomViewListOnCollectionChanged;
        }
        
        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
           // if (SessionController.Instance.SessionView.IsPenMode)
           //     return;
            
            var s = (UserControl) sender;
            var vm = s.DataContext as ElementViewModel;
            if (vm == null)
            {
                return;
            }


            var dx = e.Delta.Translation.X / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var dy = e.Delta.Translation.Y / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleY;

            //If the undo action for moving elements has been executed, stop inertia!
            if(_moveNodeUndoButton != null)
            {
                if(_moveNodeUndoButton.ActionExecuted == true)
                {
                    //Completes the manipulation without inertia
                    e.Complete();
                    _moveNodeUndoButton.ActionExecuted = false;
                }
            }
            if (_isFreeForm)
            {
                var areaView = (AreaNodeView) _view;
                var areaViewVM = (AreaNodeViewModel)areaView.DataContext;
                dx = dx/areaViewVM.CompositeTransform.ScaleX;
                dy = dy / areaViewVM.CompositeTransform.ScaleX;
            }
            if (SessionController.Instance.ActiveFreeFormViewer.Selections.Contains(vm))
            {
                if (_view is FreeFormViewer)
                {
                    var cview = (FreeFormViewer) _view;
                    Canvas.SetLeft(cview.MultiMenu, Canvas.GetLeft(cview.MultiMenu) + e.Delta.Translation.X);
                    Canvas.SetTop(cview.MultiMenu, Canvas.GetTop(cview.MultiMenu) + e.Delta.Translation.Y);
                }

                //move all selected content if a selected node is moved
                foreach (var vmodel in SessionController.Instance.ActiveFreeFormViewer.Selections)
                {
                    // we only want to be able to move elementviewmodels, because links are moved by their anchor points, but can be selected now
                    var elementViewModel = vmodel as ElementViewModel;

                    if (elementViewModel != null && !elementViewModel.IsEditing &&!elementViewModel.ContainsSelectedLink)
                        elementViewModel.Controller.SetPosition(elementViewModel.Transform.TranslateX + dx, elementViewModel.Transform.TranslateY + dy);
                }
            }
            else {
                if (!vm.IsEditing)
                {
                    Point p = new Point(vm.Transform.TranslateX + dx, vm.Transform.TranslateY + dy);
                    if (Limited)
                    {
                        if (CheckInBounds(p))
                        {
                            return;
                        }
                    }
                    vm.Controller.SetPosition(p.X, p.Y);
                }
            }
        }
        private void OnManipulationIntertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            var ffvm = (FreeFormViewerViewModel)_view.DataContext;

            //Updates new position coordinates
            _newPosition.X = e.Cumulative.Translation.X;
            _newPosition.Y = e.Cumulative.Translation.Y;

            // Transforms original and new positions from screen to global space
            _newPosition = (SessionController.Instance.ActiveFreeFormViewer.CompositeTransform).Inverse.TransformPoint(
                _newPosition);
            _originalPosition = (SessionController.Instance.ActiveFreeFormViewer.CompositeTransform).Inverse.TransformPoint(
                _originalPosition);

            //Get elements controller
            var vm = (sender as FrameworkElement).DataContext as ElementViewModel;
            if (vm != null)
            {
                var elementController = vm.Controller;

                if (!vm.IsEditing)
                {


                    var linearVelocity = e.Velocities.Linear;
                    var magnitude = Math.Sqrt(linearVelocity.X * linearVelocity.X + linearVelocity.Y * linearVelocity.Y);
                    var arbitraryThreshold = 2;
                    //If the speed of node is higher than arbitrary threshold, create undo button
                    if(magnitude > arbitraryThreshold)
                    {
                        //Instantiates MoveElementAction
                        var moveElementAction = new MoveElementAction(elementController, _originalPosition, _newPosition);

                        _moveNodeUndoButton = new UndoButton();
                        //Activates undo button makes it appear in the old position.
                        ffvm.AtomViewList.Add(_moveNodeUndoButton);
                        _moveNodeUndoButton.MoveTo(_originalPosition);
                        _moveNodeUndoButton.Activate(moveElementAction);
                        

                    }



                }
            }
        }

        public bool CheckInBounds(Point p)
        {
            var adjustedPt = new Point(p.X - 50000, p.Y - 50000);
            var bounds = _viewer.GetAdornment();
            IEnumerable<UIElement> elements = VisualTreeHelper.FindElementsInHostCoordinates(adjustedPt, bounds);
            
            var adornment = elements.Any(a => a == bounds);
            List<UIElement> elementlist = elements.ToList();
            return adornment;
        }
    }
}
