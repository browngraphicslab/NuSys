using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class GroupViewModel: NodeViewModel
    {
        private double _margin;
        public GroupViewModel(WorkspaceViewModel vm): base(vm)
        {
            AtomViewList = new ObservableCollection<UserControl>();
            NodeViewModelList = new ObservableCollection<NodeViewModel>();
            LinkViewModelList = new ObservableCollection<LinkViewModel>();
            this.AtomType = Constants.Node;
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.View = new GroupView(this);
            _margin = 75;
        }

        public void AddNode(NodeViewModel toAdd)
        {
            AtomViewList.Add(toAdd.View);
            NodeViewModelList.Add(toAdd);
            toAdd.Transform = new MatrixTransform();
            ArrangeNodesInGrid();
            foreach (var link in toAdd.LinkList)
            {
                link.IsVisible = false;
            }
            //TODO Handle links
        }
         
        public override void Resize(double dx, double dy)
        {
            base.Resize(dx, dy);
            //var minWidth = NodeViewModelList.Count*(Constants.MinNodeSizeX+75);
            //var minHeight = NodeViewModelList.Count * (Constants.MinNodeSizeY + 75);
            //var resizeX = minWidth - this.Width;
            //var resizeY = minWidth - this.Height;
            //if (resizeX > resizeY)
            //{
            //    if (resizeX > 0)
            //    {
            //        ResizeGroupMembers(-resizeX, -resizeX);
            //    }
            //}
            //else 
            //{
            //    if (resizeY > 0)
            //    {
            //        ResizeGroupMembers(-resizeY, -resizeY);
            //    }
            //}
            foreach (var node in this.NodeViewModelList)
            {
                node.Resize(dx, dx);              
            }
            _margin += dx;
            this.ArrangeNodesInGrid();
        }

        private void ArrangeNodesInGrid()
        {
            this.Width = Constants.MinNodeSizeX;
            this.Height = Constants.MinNodeSizeY;

            _margin = 75;
            var currentX = _margin;
            var currentY = _margin;
            var columnCount = Math.Round(Math.Sqrt(AtomViewList.Count));
            for (var i = 0; i < AtomViewList.Count;i++) {
                var toArr = NodeViewModelList[i];

                var mat = toArr.Transform.Matrix;
                mat.OffsetX = currentX;
                mat.OffsetY = currentY;
                toArr.Transform.Matrix = mat;
                
                if (Height < currentY + toArr.Height + _margin)
                {
                    Height = currentY + toArr.Height + _margin;
                }
                if (Width < currentX + toArr.Width + _margin)
                {
                    Width = currentX + toArr.Width + _margin;
                }
                if ((i + 1 )% columnCount == 0)
                {
                    currentX = _margin;
                    currentY += toArr.Height + _margin;
                }
                else
                {
                    currentX += toArr.Width + _margin;
                }
            }
        }

        public void RemoveNode(NodeViewModel toRemove)
        {
            foreach (var link in toRemove.LinkList)
            {
                link.IsVisible = true;
                link.UpdateAnchor();
            }
            toRemove.UpdateAnchor();
            this.AtomViewList.Remove(toRemove.View);
            NodeViewModelList.Remove(toRemove);
            ArrangeNodesInGrid();
            switch (NodeViewModelList.Count)
            {
                case 0:
                    WorkSpaceViewModel.DeleteNode(this);
                    break;
                case 1:
                    var lastNode = NodeViewModelList[0];
                    this.AtomViewList.Remove(lastNode.View);
                    NodeViewModelList.Remove(lastNode);
                    WorkSpaceViewModel.NodeViewModelList.Add(lastNode);
                    WorkSpaceViewModel.AtomViewList.Add(lastNode.View);
                    WorkSpaceViewModel.PositionNode(lastNode, this.Transform.Matrix.OffsetX, this.Transform.Matrix.OffsetY);
                    lastNode.ParentGroup = null;
                    WorkSpaceViewModel.DeleteNode(this);
                    foreach (var link in lastNode.LinkList)
                    {
                        link.IsVisible = true;
                        link.UpdateAnchor();
                    }
                    lastNode.UpdateAnchor();
                    break;
            }
            //TODO Handle links
        }

        public ObservableCollection<UserControl> AtomViewList { get; private set;}
        public ObservableCollection<LinkViewModel> LinkViewModelList { get; private set; }
        public ObservableCollection<NodeViewModel> NodeViewModelList { get; private set; }
    }
}