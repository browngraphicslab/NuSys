using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

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
                        children.Add((child.DataContext as GroupItemViewModel).Id);
                    }
                    props["groupChildren"] = children;

                }


                var callback = new Action<string>(async (newId) =>
                {

                    var wvm = _view.DataContext as WorkspaceViewModel;
                    var found = wvm.AtomViewList.Where(a => (a.DataContext as AtomViewModel).Id == newId);

                    var duplicateModel = (AtomModel)SessionController.Instance.IdToSendables[newId];

                    if (!(duplicateModel is NodeContainerModel))
                        return;
                    
                    
                    foreach (var child in SessionController.Instance.IdToSendables.Values.Where( s => (s as AtomModel).Creators.Contains(duplicateModel.Id)))
                    {
                        ((NodeContainerModel) duplicateModel).AddChild(child);
                    }

                });

//                NetworkConnector.Instance.RequestDuplicateNode(props, callback);
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
