﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using NusysIntermediate;

namespace NuSysApp
{
    public class DuplicateNodeMode : AbstractWorkspaceViewMode
    {
        private ElementViewModel _selectedNode;

        public DuplicateNodeMode(FrameworkElement view) : base(view) { }

        public override async Task Activate()
        {
            _view.IsRightTapEnabled = true;
            ElementCollectionViewModel wvm = (ElementCollectionViewModel) _view.DataContext;

            wvm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
            foreach (var userControl in wvm.AtomViewList.Where(s => s.DataContext is ElementViewModel))
            {
                userControl.PointerPressed += OnAtomPressed;
                userControl.PointerReleased += OnAtomReleased;
            }
            _view.RightTapped += OnWorkspacePressed;
        }

        private void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.NewItems == null)
                return;

            foreach (var newItem in notifyCollectionChangedEventArgs.NewItems)
            {
                var item = (FrameworkElement) newItem;
                if (item.DataContext is ElementViewModel) { 
                   item.PointerPressed += OnAtomPressed;
                   item.PointerReleased += OnAtomReleased;
                }
            }
        }
        

        public override async Task Deactivate()
        {
            ElementCollectionViewModel wvm = (ElementCollectionViewModel)_view.DataContext;
            foreach (var userControl in wvm.AtomViewList.Where( s => s.DataContext is ElementViewModel))
            {
                userControl.PointerPressed -= OnAtomPressed;
                userControl.PointerReleased -= OnAtomReleased;
            }
            _view.RightTapped -= OnWorkspacePressed;
            wvm.AtomViewList.CollectionChanged -= AtomViewListOnCollectionChanged;
        }

        private async void OnWorkspacePressed(object sender, RightTappedRoutedEventArgs e)
        {
            var doubleTappedNode = ((FrameworkElement)e.OriginalSource).DataContext;
            if (_selectedNode != null && _selectedNode != doubleTappedNode)
            {
                var vm = (FreeFormViewerViewModel)_view.DataContext;

                var tappedPoint = e.GetPosition(null);
                var p = vm.CompositeTransform.Inverse.TransformPoint(tappedPoint);
                p.X -= _selectedNode.Width / 2;
                p.Y -= _selectedNode.Height / 2;

                var msg = new Message();
                msg["id"] = _selectedNode.Id;
                msg["targetX"] = p.X;
                msg["targetY"] = p.Y;

                // TODO: factor this out to the DuplicateNodeRequest
                if (_selectedNode is ElementCollectionViewModel)
                {
                    var children = new List<string>(); ;
                    foreach (var child in (_selectedNode as ElementCollectionViewModel).AtomViewList)
                    {
                        children.Add((child.DataContext as GroupItemViewModel).Id);
                    }
                    msg["groupChildren"] = children;
                }
                
                var request = new DuplicateNodeRequest(msg);
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            }
        }

        private void OnAtomReleased(object sender, PointerRoutedEventArgs e)
        {
            _selectedNode = null;
        }

        private void OnAtomPressed(object sender, PointerRoutedEventArgs e)
        {
            _selectedNode = (ElementViewModel)((FrameworkElement)sender).DataContext;
        }
    }
}
