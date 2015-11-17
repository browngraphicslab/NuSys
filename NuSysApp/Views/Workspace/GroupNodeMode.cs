﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class GroupNodeMode : AbstractWorkspaceViewMode
    {
        private UserControl _selectedView;
        private GroupTagNodeView _hoveredGroup;
        private List<NodeViewModel> _pressedItems = new List<NodeViewModel>(); 
    

        public GroupNodeMode(WorkspaceView view) : base(view) { }

        public override async Task Activate()
        {
            _view.IsDoubleTapEnabled = true;
            WorkspaceViewModel wvm = (WorkspaceViewModel) _view.DataContext;

            wvm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
            foreach (var userControl in wvm.AtomViewList.Where(s => s.DataContext is NodeViewModel))
            {
                userControl.PointerPressed += OnAtomPressed;
                userControl.PointerReleased += OnAtomReleased;
            }

            foreach (var userControl in wvm.AtomViewList.Where(s => s.DataContext is GroupTagNodeViewModel))
            {
                userControl.DoubleTapped += OnGroupTagDoubleTapped;
            }
        }


        public override async Task Deactivate()
        {
            WorkspaceViewModel wvm = (WorkspaceViewModel)_view.DataContext;

            foreach (var userControl in wvm.AtomViewList.Where(s => s.DataContext is NodeViewModel))
            {
                userControl.PointerPressed -= OnAtomPressed;
                userControl.PointerReleased -= OnAtomReleased;
            }
            wvm.AtomViewList.CollectionChanged -= AtomViewListOnCollectionChanged;

            foreach (var userControl in wvm.AtomViewList.Where(s => s.DataContext is GroupTagNodeViewModel))
            {
                userControl.DoubleTapped -= OnGroupTagDoubleTapped;
            }
        }

        private void OnGroupTagDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var groupTagNode = (GroupTagNodeView) sender;
            groupTagNode.ToggleExpand();
        }

        private void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.NewItems == null)
                return;

            foreach (var newItem in notifyCollectionChangedEventArgs.NewItems)
            {
               if (((FrameworkElement)newItem).DataContext is GroupTagNodeViewModel)
                    continue;

               var item = (UserControl) newItem;
               item.PointerPressed += OnAtomPressed;
               item.PointerReleased += OnAtomReleased;
            }
        }


        private async void OnAtomReleased(object sender, PointerRoutedEventArgs e)
        {
            var releasedNode = ((FrameworkElement)e.OriginalSource).DataContext as NodeViewModel;

            if (releasedNode == null)
                return;

            if (_pressedItems.Count == 2)
                BuildGroup(_pressedItems[0], _pressedItems[1], true);
            else if(_pressedItems.Count == 1)
            {
                var hits = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(_view).Position, _view);
                var result = hits.Where(uiElem => uiElem is GroupTagNodeView);

                if (result?.Count() > 0)
                {
                    BuildGroup(_pressedItems[0], ((FrameworkElement)result.First()).DataContext as NodeViewModel);
                    _hoveredGroup = null;
                }
            }

            _pressedItems.Remove(releasedNode);
        }

        private async void BuildGroup(NodeViewModel node0, NodeViewModel node1, bool keepOriginal = false)
        {
 
            if (node1 == null)
                return;
            
            if (node0 == node1)
            {
                return;
            }

            var groupTagNode = node0 is GroupTagNodeViewModel
                ? node0 as GroupTagNodeViewModel
                : node1 as GroupTagNodeViewModel;
            var nodeToTag = groupTagNode == node0 ? node1 : node0;
            if (groupTagNode == null || nodeToTag == null || nodeToTag is GroupTagNodeViewModel)
            {
                return;
            }

            var inkCaption = groupTagNode.Title;
            var tags = groupTagNode.Model.GetMetaData("tags");
            nodeToTag.Model.SetMetaData("tags", tags + " " + inkCaption);

            if (!keepOriginal) { 
                var nodeModel = (NodeModel)nodeToTag.Model;
                nodeModel.MoveToGroup((GroupModel)groupTagNode.Model);
            } else { 
                var callback = new Action<string>((s) =>
                {
                    UITask.Run(() =>
                    {
                        var newNodeModel = (NodeModel)SessionController.Instance.IdToSendables[s];
                        newNodeModel.MoveToGroup((GroupModel)groupTagNode.Model);
                        nodeToTag.Model.SetMetaData("tags", groupTagNode.Model.GetMetaData("tags") + " " + inkCaption);
                        Debug.WriteLine("node created");
                    });
                });

                var dict = await nodeToTag.Model.Pack();
                var props = dict;
                props.Remove("id");
                props.Remove("type");
                props.Remove("nodeType");
                props.Remove("x");
                props.Remove("y");
                props.Remove("metadata");
                NetworkConnector.Instance.RequestMakeNode("0", "0", nodeToTag.NodeType.ToString(), null, null, props, callback);
            }
            //e.Handled = true;
        }

        private void OnAtomPressed(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnAtomPressed");
      
           
            var pressedNode = ((FrameworkElement)e.OriginalSource).DataContext as NodeViewModel;
            Debug.WriteLine(pressedNode);
            if (pressedNode == null)
                return;
            
            _pressedItems.Add(pressedNode);
            //Debug.WriteLine(_selectedNode);
            //e.Handled = true;
        }
    }
}
