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
using Windows.UI.Xaml.Media.Animation;

namespace NuSysApp
{
    public class NodeManipulationMode : AbstractWorkspaceViewMode
    {
        private static int _zIndexCounter = 10000;
        private bool _isPinAnimating;

        public List<UserControl> ActiveNodes { get; private set; }

        public NodeManipulationMode(FreeFormViewer view) : base(view) { }

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
            if (userControl.DataContext is ElementInstanceViewModel)
                Canvas.SetZIndex(userControl, _zIndexCounter++);
            
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
                var userControl = (UserControl) n;
                if (userControl.DataContext is ElementInstanceViewModel) { 
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
            if (SessionController.Instance.SessionView.IsPenMode)
                return;

            var s = (UserControl) sender;
            var vm = (ElementInstanceViewModel)s.DataContext;
            vm.Controller.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);                  
        }
    }
}
