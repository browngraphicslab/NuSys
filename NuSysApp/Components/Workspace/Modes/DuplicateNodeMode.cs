﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class DuplicateNodeMode : AbstractWorkspaceViewMode
    {
        private NodeViewModel _selectedNode;

        public DuplicateNodeMode(WorkspaceView view) : base(view) { }

        public override async Task Activate()
        {
            _view.IsRightTapEnabled = true;
            WorkspaceViewModel wvm = (WorkspaceViewModel) _view.DataContext;

            wvm.Children.CollectionChanged += AtomViewListOnCollectionChanged;
            foreach (var userControl in wvm.Children.Values.Where(s => s.DataContext is NodeViewModel))
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
                var kv = (KeyValuePair<string, FrameworkElement>) newItem;
                var item = (FrameworkElement)kv.Value;
                if (item.DataContext is NodeViewModel) { 
                   item.PointerPressed += OnAtomPressed;
                   item.PointerReleased += OnAtomReleased;
                }
            }
        }
        

        public override async Task Deactivate()
        {
            WorkspaceViewModel wvm = (WorkspaceViewModel)_view.DataContext;
            foreach (var userControl in wvm.Children.Values.Where( s => s.DataContext is NodeViewModel))
            {
                userControl.PointerPressed -= OnAtomPressed;
                userControl.PointerReleased -= OnAtomReleased;
            }
            _view.RightTapped -= OnWorkspacePressed;
            wvm.Children.CollectionChanged -= AtomViewListOnCollectionChanged;
        }

        private async void OnWorkspacePressed(object sender, RightTappedRoutedEventArgs e)
        {
            var doubleTappedNode = ((FrameworkElement)e.OriginalSource).DataContext;
            if (_selectedNode != null && _selectedNode != doubleTappedNode)
            {
                var vm = (WorkspaceViewModel)_view.DataContext;

                var tappedPoint = e.GetPosition(null);
                var p = vm.CompositeTransform.Inverse.TransformPoint(tappedPoint);
                p.X -= _selectedNode.Width / 2;
                p.Y -= _selectedNode.Height / 2;
                var dict = await _selectedNode.Model.Pack();

                var props = dict;
                props.Remove("id");
                props.Remove("type");
                props["x"] = p.X.ToString();
                props["y"] = p.Y.ToString();

                if (_selectedNode is NodeContainerViewModel)
                {
                    var children = new List<string>();;
                    foreach (var child in (_selectedNode as NodeContainerViewModel).Children.Values)
                    {
                        children.Add((child.DataContext as AtomViewModel).Id);
                    }
                    props["groupChildren"] = children;

                }
               
      
                //  NetworkConnector.Instance.RequestMakeNode(, _selectedNode.NodeType.ToString(), props["contentId"]?.ToString(), null, props);
                NetworkConnector.Instance.RequestDuplicateNode(props);

            }

        }

        private void OnAtomReleased(object sender, PointerRoutedEventArgs e)
        {
            _selectedNode = null;
        }

        private void OnAtomPressed(object sender, PointerRoutedEventArgs e)
        {
            _selectedNode = (NodeViewModel)((FrameworkElement)sender).DataContext;
        }
    }
}
