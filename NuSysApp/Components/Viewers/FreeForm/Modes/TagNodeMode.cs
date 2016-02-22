using System;
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
    public class TagNodeMode : AbstractWorkspaceViewMode
    {
        private UserControl _selectedView;
        private LabelNodeView _hoveredGroup;
        private List<ElementInstanceViewModel> _pressedItems = new List<ElementInstanceViewModel>(); 

        public TagNodeMode(FreeFormViewer view) : base(view) { }

        public override async Task Activate()
        {
            _view.IsDoubleTapEnabled = true;
            FreeFormViewerViewModel wvm = (FreeFormViewerViewModel) _view.DataContext;

            wvm.Children.CollectionChanged += AtomViewListOnCollectionChanged;
            foreach (var userControl in wvm.Children.Values.Where(s => s.DataContext is ElementInstanceViewModel))
            {
                userControl.PointerPressed += OnAtomPressed;
                userControl.PointerReleased += OnAtomReleased;
            }

            foreach (var userControl in wvm.Children.Values.Where(s => s.DataContext is LabelNodeViewModel))
            {
                userControl.DoubleTapped += OnGroupTagDoubleTapped;
            }
        }


        public override async Task Deactivate()
        {
            FreeFormViewerViewModel wvm = (FreeFormViewerViewModel)_view.DataContext;

            foreach (var userControl in wvm.Children.Values.Where(s => s.DataContext is ElementInstanceViewModel))
            {
                userControl.PointerPressed -= OnAtomPressed;
                userControl.PointerReleased -= OnAtomReleased;
            }
            wvm.Children.CollectionChanged -= AtomViewListOnCollectionChanged;

            foreach (var userControl in wvm.Children.Values.Where(s => s.DataContext is LabelNodeViewModel))
            {
                userControl.DoubleTapped -= OnGroupTagDoubleTapped;
                Canvas.SetZIndex(userControl, 100);
            }
        }

        private void OnGroupTagDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var groupTagNode = (LabelNodeView) sender;
        //    groupTagNode.ToggleExpand();
        }

        private void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.NewItems == null)
                return;

            foreach (var newItem in notifyCollectionChangedEventArgs.NewItems)
            {
                var kv = (KeyValuePair<string, FrameworkElement>)newItem;
                
                if (((FrameworkElement)kv.Value).DataContext is LabelNodeViewModel) {
                    Canvas.SetZIndex((UserControl)kv.Value, 100);
                    continue;
                }

                var item = (UserControl)kv.Value;
               item.PointerPressed += OnAtomPressed;
               item.PointerReleased += OnAtomReleased;
            }
        }


        private async void OnAtomReleased(object sender, PointerRoutedEventArgs e)
        {
            var releasedNode = ((FrameworkElement)e.OriginalSource).DataContext as ElementInstanceViewModel;

            if (releasedNode == null)
                return;

            //_view.ReleasePointerCaptures();

            if (_pressedItems.Count == 2)
            {
                BuildGroup(_pressedItems[0], _pressedItems[1], true);
                _pressedItems.Remove(_pressedItems[1]);
            }
            else if (_pressedItems.Count == 1)
            {
                var hits = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(_view).Position, _view);
                var result = hits.Where(uiElem => uiElem is LabelNodeView);

                if (result?.Count() > 0)
                {
                    BuildGroup(_pressedItems[0], ((FrameworkElement) result.First()).DataContext as ElementInstanceViewModel);

                    _hoveredGroup = null;
                }
                _pressedItems.Remove(_pressedItems[0]);
            } else { 

                Debug.WriteLine("didn't removed anything!!!");
            }
        }

        private async void BuildGroup(ElementInstanceViewModel node0, ElementInstanceViewModel node1, bool keepOriginal = false)
        {
            if (node1 == null)
                return;
            
            if (node0 == node1)
            {
                return;
            }

            var groupTagNode = node0 is LabelNodeViewModel
                ? node0 as LabelNodeViewModel
                : node1 as LabelNodeViewModel;
            var nodeToTag = groupTagNode == node0 ? node1 : node0;
            if (groupTagNode == null || nodeToTag == null || nodeToTag is LabelNodeViewModel)
            {
                return;
            }

            var inkCaption = groupTagNode.Title;
            var tags = (List<string>) nodeToTag.Model.GetMetaData("tags");

            if (tags.Contains(inkCaption))
                return;

            tags.Add(inkCaption);

            nodeToTag.Model.SetMetaData("tags", tags);
            
            // tag all visual copies

            foreach (var userControl in SessionController.Instance.ActiveFreeFormViewer.Children.Values)
            {
                var vm = (ElementInstanceViewModel) userControl.DataContext;
                var model = vm.Model;
                if (model.GetMetaData("visualCopyOf") == nodeToTag.Id)
                {
                    var t = (List<string>)nodeToTag.Model.GetMetaData("tags");
                    t.Add(inkCaption);
                    model.SetMetaData("tags", tags);
                }
            }

            var nodeToTagModel = (ElementInstanceModel)nodeToTag.Model;
            if (!keepOriginal) {

              //  nodeToTagModel.MoveToGroup((NodeContainerModel)groupTagNode.Model, true);
            } else { 
                var callback = new Action<string>((s) =>
                {
                    UITask.Run(() =>
                    {
                        var newNodeModel = SessionController.Instance.IdToSendables[s].Model;
                        newNodeModel.SetMetaData("visualCopyOf", nodeToTag.Id);
                        //newNodeModel.MoveToGroup((NodeContainerModel)groupTagNode.Model, true);
                    });
                });

                var dict = await nodeToTag.Model.Pack();
                var props = dict;
                props["creator"] = groupTagNode.Id;
                props.Remove("id");
                props.Remove("type");
                props.Remove("nodeType");
                props.Remove("x");
                props.Remove("y");
              //  props.Remove("metadata");
                //NetworkConnector.Instance.RequestMakeNode(nodeToTagModel.X.ToString(), nodeToTagModel.Y.ToString(), nodeToTag.NodeType.ToString(), null, null, props, callback);
            }
            //e.Handled = true;
        }

        private void OnAtomPressed(object sender, PointerRoutedEventArgs e)
        {   
            var pressedNode = ((FrameworkElement)e.OriginalSource).DataContext as ElementInstanceViewModel;
           
            if (pressedNode == null)
                return;

            //_view.CapturePointer(e.Pointer);

            
            if (!(pressedNode is LabelNodeViewModel))
                Canvas.SetZIndex((FrameworkElement)sender, 0);

            _pressedItems.Add(pressedNode);
        }
    }
}
