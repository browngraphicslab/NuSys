﻿using System;
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
            if (userControl.DataContext is ElementViewModel)
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
                var userControl = (UserControl) n;
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
            if (SessionController.Instance.SessionView.IsPenMode)
                return;
            
            var s = (UserControl) sender;
            var vm = (ElementViewModel)s.DataContext;



            var dx = e.Delta.Translation.X / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var dy = e.Delta.Translation.Y / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleY;

            if (SessionController.Instance.ActiveFreeFormViewer.Selections.Contains(vm))
            {
               Canvas.SetLeft(_view.MultiMenu, Canvas.GetLeft(_view.MultiMenu) + e.Delta.Translation.X);
               Canvas.SetTop(_view.MultiMenu, Canvas.GetTop(_view.MultiMenu) + e.Delta.Translation.Y);

                //move all selected content if a selected node is moved
                foreach (var vmodel in SessionController.Instance.ActiveFreeFormViewer.Selections)
                {
                    if (!vmodel.IsEditing &&!vmodel.ContainsSelectedLink)
                        vmodel.Controller.SetPosition(vmodel.Transform.TranslateX + dx, vmodel.Transform.TranslateY + dy);
                }
            }
            else {
                if (!vm.IsEditing)
                    vm.Controller.SetPosition(vm.Transform.TranslateX + dx, vm.Transform.TranslateY + dy);
            }
        }
    }
}
