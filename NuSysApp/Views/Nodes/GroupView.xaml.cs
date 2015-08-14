using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            (this.DataContext as GroupViewModel).NodeViewModelList.CollectionChanged += AtomViewList_CollectionChanged;
        }

        private void AtomViewList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
            var _margin = 75.0 * scale;
            var currentX = _margin;
            var currentY = _margin;
            scale = 1;
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
                if (vm.Height < currentY + toArr.Height * scale + _margin)
                {
                    vm.Height = currentY + toArr.Height * scale+ _margin;
                    Height = currentY + toArr.Height * scale+ _margin;
                }
                if (vm.Width < currentX + toArr.Width * scale+ _margin)
                {
                    vm.Width = currentX + toArr.Width * scale+ _margin;
                    Width = currentX + toArr.Width * scale+ _margin;
                }
                if ((i + 1) % columnCount == 0)
                {
                    currentX = _margin;
                    currentY += heightToAdd * scale+ _margin;
                    heightToAdd = 0;
                }
                else
                {
                    currentX += toArr.Width * scale+ _margin;
                }
            }
            this.DataContext = vm;
        }
        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var vm = (GroupViewModel)this.DataContext;
            vm.WorkSpaceViewModel.CheckForNodeNodeIntersection(vm); //TODO Eventually need to remove 
            e.Handled = true;
        }
    }
}
