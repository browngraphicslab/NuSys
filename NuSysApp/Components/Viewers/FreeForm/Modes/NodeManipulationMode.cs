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
        public List<UserControl> ActiveNodes { get; private set; }
        public bool Limited { get; set; }
        private FreeFormViewer _viewer;
        public NodeManipulationMode(FrameworkElement view) : base(view) { }

    
        public NodeManipulationMode(FrameworkElement view, bool isFreeFormCollection) : base(view)
        {
            _isFreeForm = isFreeFormCollection;

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
                userControl.ManipulationMode = ManipulationModes.All;
                userControl.ManipulationStarted += ManipulationStarting;
                userControl.ManipulationDelta += OnManipulationDelta;
                userControl.ManipulationCompleted += OnManipulationCompleted;
            }

            vm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs manipulationCompletedRoutedEventArgs)
        {
            ActiveNodes.Remove((UserControl) sender);
        }

        private void ManipulationStarting(object sender, ManipulationStartedRoutedEventArgs manipulationStartingRoutedEventArgs)
        {
            var userControl = (UserControl)sender;
            if (userControl.DataContext is ElementViewModel && !(userControl.DataContext is LinkViewModel))
            {
                Canvas.SetZIndex(userControl, _zIndexCounter++);
            }

            ActiveNodes.Add((UserControl)sender);
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
                    Debug.WriteLine(p);
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
