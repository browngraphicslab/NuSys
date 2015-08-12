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
        private CompositeTransform _localTransform;
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
            this.LocalTransform = new CompositeTransform();
        }

        public void AddNode(NodeViewModel toAdd)
        {
            AtomViewList.Add(toAdd.View);
            NodeViewModelList.Add(toAdd);
            toAdd.Transform = new MatrixTransform();
            ArrangeNodesInGrid();

            //TODO Handle links
        }
         
        public override void Resize(double dx, double dy)
        {
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
            //            foreach (var node in this.NodeViewModelList)
            //            {
            //                node.Resize(dx, dx);
            //                
            //            }

            var trans = LocalTransform;
            var scale = dx < dy ? (Width + dx) / Width : (Height + dy) / Height;
            trans.ScaleX *= scale;
            trans.ScaleY *= scale;
            LocalTransform = trans;
            //var transmat = Transform.Matrix;
            //transmat.M11 *= scale;
            //transmat.M22 *= scale;
            //Transform.Matrix = transmat;
            base.Resize(dx, dy);
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
            columnCount = 2 > columnCount ? 2 : columnCount;
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
                    break;
            }
            //TODO Handle links
        }

        public ObservableCollection<UserControl> AtomViewList { get; private set;}
        public ObservableCollection<LinkViewModel> LinkViewModelList { get; private set; }
        public ObservableCollection<NodeViewModel> NodeViewModelList { get; private set; }

        public CompositeTransform LocalTransform
        {
            get { return _localTransform; }
            set
            {
                if (_localTransform == value)
                {
                    return;
                }
                _localTransform = value;
                RaisePropertyChanged("LocalTransform");
            }
        }
    }
}