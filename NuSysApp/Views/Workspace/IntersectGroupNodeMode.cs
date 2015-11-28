using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class IntersectGroupNodeMode : AbstractWorkspaceViewMode
    {
     
        private bool _isIntersecting;
        private GroupTagNodeView _intersectedGroupNode;

        public IntersectGroupNodeMode(WorkspaceView view) : base(view)
        {
        }

        public override async Task Activate()
        {
            var wvm = (WorkspaceViewModel) _view.DataContext;
            wvm.AtomViewList.CollectionChanged += OnWorkspaceChildrenChanged;
            foreach (var userControl in wvm.AtomViewList.Where(s => s is GroupTagNodeView))
            {
                userControl.PointerEntered += OnAtomPressed;
                userControl.PointerMoved += OnPointerMoved;
                userControl.PointerExited += OnAtomReleased;
            }
        }

        public override async Task Deactivate()
        {
            var wvm = (WorkspaceViewModel) _view.DataContext;
            wvm.AtomViewList.CollectionChanged -= OnWorkspaceChildrenChanged;
            foreach (var userControl in wvm.AtomViewList.Where(s => s is GroupTagNodeView))
            {
                userControl.PointerEntered -= OnAtomPressed;
                userControl.PointerMoved -= OnPointerMoved;
                userControl.PointerExited -= OnAtomReleased;
            }
        }

        private void OnWorkspaceChildrenChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.NewItems == null)
                return;

            foreach (var newItem in notifyCollectionChangedEventArgs.NewItems)
            {
                if (!(((FrameworkElement) newItem) is GroupTagNodeView)) continue;
                var item = (UserControl) newItem;
                item.PointerPressed += OnAtomPressed;
                item.PointerMoved += OnPointerMoved;
                item.PointerReleased += OnAtomReleased;
            }
        }


        private void OnPointerMoved(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
        //    Debug.WriteLine(sender);
            var obj = (GroupTagNodeView) sender;
            var vm = (GroupTagNodeViewModel) obj.DataContext;
            var wvm = (WorkspaceViewModel)_view.DataContext;
            var x = ((CompositeTransform)obj.RenderTransform).TranslateX;
            var y = ((CompositeTransform)obj.RenderTransform).TranslateY;

            const double threshold = 100;

            if (vm.IsTemporary)
                return;

            foreach (var userControl in wvm.AtomViewList.Where(s => s is GroupTagNodeView))
            {
                if (userControl == obj)
                    continue;

                var otherVm = (GroupTagNodeViewModel)userControl.DataContext;
                if (otherVm.IsTemporary)
                    continue;

                var transform = (CompositeTransform)userControl.RenderTransform;
                var distX = Math.Abs(x - transform.TranslateX);
                var distY = Math.Abs(y - transform.TranslateY);
                if (_intersectedGroupNode == null && Math.Sqrt(distX*distX + distY*distY) < threshold)
                {
                    Debug.WriteLine("IIINNNTTTERRRESSSECCCTTTT");
                    _intersectedGroupNode = (GroupTagNodeView)userControl;
                    var props = new Dictionary<string, string>();
                    props["isTemporary"] = "True";
                    NetworkConnector.Instance.RequestNewGroupTag(transform.TranslateX.ToString(), (transform.TranslateY + 120).ToString(),"Interection", props);
                } else if (_intersectedGroupNode != null && Math.Sqrt(distX * distX + distY * distY) > threshold)
                {
                    Debug.WriteLine("Done intersecting");
                    _intersectedGroupNode = null;
                }
            }
        }

        private async void OnAtomReleased(object sender, PointerRoutedEventArgs e)
        {
            var uc = (UserControl)sender;

        }

        private void OnAtomPressed(object sender, PointerRoutedEventArgs e)
        {
            var uc = (UserControl) sender;
   
        }
    }
}