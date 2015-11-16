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
    public class GroupNodeMode : AbstractWorkspaceViewMode
    {
        private GroupTagNodeViewModel _selectedNode;
        private UserControl _selectedView;
        private int _pointerCount = 0;

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
        }

        private void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.NewItems == null)
                return;

            foreach (var newItem in notifyCollectionChangedEventArgs.NewItems)
            {
               var item = (UserControl) newItem;
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
            wvm.AtomViewList.CollectionChanged -= AtomViewListOnCollectionChanged;
        }

        private async void OnAtomReleased(object sender, PointerRoutedEventArgs e)
        {        
            Debug.WriteLine("OnAtomReleased 1");
            _pointerCount--;

            var nodeToTag = ((FrameworkElement)e.OriginalSource).DataContext as NodeViewModel;

            if ( nodeToTag != null &&_selectedNode != null && _pointerCount == 1 && nodeToTag is NodeViewModel && !( nodeToTag is GroupTagNodeViewModel) && _selectedNode != nodeToTag)
            {
                var vm = (WorkspaceViewModel) _view.DataContext;
                var dict = await nodeToTag.Model.Pack();
                var tappedPoint = e.GetCurrentPoint(_view).Position;
                var inkCaption = _selectedNode.Title;
                var tags = nodeToTag.Model.GetMetaData("tags");
                nodeToTag.Model.SetMetaData("tags", tags + " " + inkCaption);

                var nodeModel = (NodeModel) nodeToTag.Model;
                nodeModel.MoveToGroup((GroupModel)_selectedNode.Model);
                 
          
                /*
                var props = dict;
                props.Remove("id");
                props.Remove("type");
                props.Remove("nodeType");
                props.Remove("x");
                props.Remove("y");
                props.Remove("metadata");

               
                tappedPoint.X -= nodeToTag.Width/2;
                tappedPoint.Y -= nodeToTag.Height/2;
                var p = vm.CompositeTransform.Inverse.TransformPoint(tappedPoint);
                NetworkConnector.Instance.RequestMakeNode(p.X.ToString(), p.Y.ToString(),
                    nodeToTag.NodeType.ToString(), null, null, props);
                    */

            }
            else if (_pointerCount <= 0)
            {
                _pointerCount = 0;
                _selectedNode = null;
            }
            
            //e.Handled = true;
        }

        private void OnAtomPressed(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnAtomPressed");
            _pointerCount++;
            if (_selectedNode == null && ((UserControl)sender).DataContext is GroupTagNodeViewModel) { 
                _selectedNode = (GroupTagNodeViewModel)((UserControl)sender).DataContext;
                _selectedView = (UserControl) sender;
                
            }
            //Debug.WriteLine(_selectedNode);
            //e.Handled = true;
        }
    }
}
