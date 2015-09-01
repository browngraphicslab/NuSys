using System;
using System.Collections.Specialized;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class GroupView : UserControl
    {
        public GroupView(GroupViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            Canvas.SetZIndex(this, -1);
            var groupViewModel = this.DataContext as GroupViewModel;
            if (groupViewModel != null)
            {
                groupViewModel.NodeViewModelList.CollectionChanged += AtomViewList_CollectionChanged;
            }
        }

        private void AtomViewList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ArrangeNodesInGrid();
        }

        public void ArrangeNodesInGrid()
        {
            var vm = this.DataContext as GroupViewModel;
            var AtomViewList = vm.NodeViewModelList;
            var NodeViewModelList = vm.NodeViewModelList;
            vm.Width = Constants.MinNodeSizeX;
            vm.Height = Constants.MinNodeSizeY;

            var scale = vm.LocalTransform.ScaleX;
            var _margin = 75.0 ;
            var currentX = _margin;
            var currentY = _margin;
            var columnCount = Math.Round(Math.Sqrt(AtomViewList.Count));
            columnCount = 2 > columnCount ? 2 : columnCount;
            var heightToAdd = 0.0;
            for (var i = 0; i < AtomViewList.Count; i++)
            {
                var toArr = NodeViewModelList[i];
                var mat = toArr.Transform.Matrix;
                mat.OffsetX = currentX;
                mat.OffsetY = currentY;
                toArr.Transform.Matrix = mat;
                heightToAdd = heightToAdd < toArr.Height ? toArr.Height : heightToAdd;
                if (vm.Height < currentY + toArr.Height  + _margin)
                {
                    vm.Height = currentY + toArr.Height + _margin;
                    Height = currentY + toArr.Height + _margin;
                }
                if (vm.Width < currentX + toArr.Width + _margin)
                {
                    vm.Width = currentX + toArr.Width + _margin;
                    Width = currentX + toArr.Width + _margin;
                }
                if ((i + 1) % columnCount == 0)
                {
                    currentX = _margin;
                    currentY += heightToAdd + _margin;
                    heightToAdd = 0;
                }
                else
                {
                    currentX += toArr.Width + _margin;
                }
            }
            vm.Height *= scale;
            vm.Width *= scale;
            Height *= scale;
            Width *= scale;
            this.DataContext = vm;
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.Remove();
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var vm = (GroupViewModel)this.DataContext;
            vm.WorkSpaceViewModel.CheckForNodeNodeIntersection(vm); //TODO Eventually need to remove 
            this.ArrangeNodesInGrid();
            e.Handled = true;
        }

        private void OnClick_Grid(object sender, RoutedEventArgs e)
        {
            this.ArrangeNodesInGrid();
        }
    }
}
