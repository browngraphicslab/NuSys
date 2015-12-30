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
        private int _zIndexCounter = 10000;
        private bool _isPinAnimating;

        public NodeManipulationMode(WorkspaceView view) : base(view) { }

        public override async Task Activate()
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            foreach (var userControl in vm.AtomViewList)
            {
                userControl.ManipulationMode = ManipulationModes.All;
                userControl.ManipulationStarting += ManipulationStarting;
                userControl.ManipulationDelta += OnManipulationDelta;
            }

            vm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
        }

        private void ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs manipulationStartingRoutedEventArgs)
        {
            var userControl = (UserControl)sender;
            if (userControl.DataContext is NodeViewModel)
                Canvas.SetZIndex(userControl, _zIndexCounter++);
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
                if (userControl.DataContext is NodeViewModel) { 
                    userControl.ManipulationMode = ManipulationModes.All;
                    userControl.ManipulationDelta += OnManipulationDelta;
                }
            }
        }

        public override async Task Deactivate()
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            foreach (var userControl in vm.AtomViewList)
            {
                userControl.ManipulationMode = ManipulationModes.All;
                userControl.ManipulationDelta -= OnManipulationDelta;
            }

            vm.AtomViewList.CollectionChanged -= AtomViewListOnCollectionChanged;
        }
        
        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.IsPenMode)
                return;

            var s = (UserControl) sender;

            var vm = s.DataContext as NodeViewModel;
            vm?.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
        }
    }
}
