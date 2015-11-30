using System;
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
            _view.IsDoubleTapEnabled = true;
            WorkspaceViewModel wvm = (WorkspaceViewModel) _view.DataContext;

            wvm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
            foreach (var userControl in wvm.AtomViewList.Where(s => s.DataContext is NodeViewModel))
            {
                userControl.PointerPressed += OnAtomPressed;
                userControl.PointerReleased += OnAtomReleased;
            }

            _view.DoubleTapped += OnWorkspacePressed;
        }

        private void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.NewItems == null)
                return;

            foreach (var newItem in notifyCollectionChangedEventArgs.NewItems)
            {
                var item = (UserControl) newItem;
            //     item.PointerPressed -= OnAtomPressed;
            //     item.PointerReleased -= OnAtomReleased;
               item.PointerPressed += OnAtomPressed;
               item.PointerReleased += OnAtomReleased;
            }
        }
        

        public override async Task Deactivate()
        {
            WorkspaceViewModel wvm = (WorkspaceViewModel)_view.DataContext;
            foreach (var userControl in wvm.AtomViewList.Where( s => s.DataContext is NodeViewModel))
            {
                userControl.PointerPressed -= OnAtomPressed;
                userControl.PointerReleased -= OnAtomReleased;
            }
            _view.DoubleTapped -= OnWorkspacePressed;
            wvm.AtomViewList.CollectionChanged -= AtomViewListOnCollectionChanged;
        }

        private async void OnWorkspacePressed(object sender, DoubleTappedRoutedEventArgs e)
        {
            var doubleTappedNode = ((FrameworkElement)e.OriginalSource).DataContext;
            if (_selectedNode != null && _selectedNode != doubleTappedNode)
            {
                Debug.WriteLine("OnWorkspacePressed");
                var vm = (WorkspaceViewModel)_view.DataContext;
                var dict = await _selectedNode.Model.Pack();

                var props = dict;
                props.Remove("id");
                props.Remove("type");
                props.Remove("nodeType");
                props.Remove("x");
                props.Remove("y");
               // props.Remove("metadata");
               // props.Add("meta", _selectedNode.Model.GetMetaData("tags"));

                var tappedPoint = e.GetPosition(null);
                tappedPoint.X -= _selectedNode.Width/2;
                tappedPoint.Y -= _selectedNode.Height/2;
                var p = vm.CompositeTransform.Inverse.TransformPoint(tappedPoint);
                NetworkConnector.Instance.RequestMakeNode(p.X.ToString(),p.Y.ToString(), _selectedNode.NodeType.ToString(), null, null, props);
            }

            //_selectedNode = null;
        }

        private void OnAtomReleased(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnAtomReleased");
            _selectedNode = null;
           // e.Handled = true;
        }

        private void OnAtomPressed(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnAtomPressed");
            _selectedNode = (NodeViewModel)((UserControl)sender).DataContext;
            //Debug.WriteLine(_selectedNode);
            //e.Handled = true;
        }
    }
}
