using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class GroupViewModel: NodeViewModel
    {
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
        }

        public void AddNode(NodeViewModel toAdd)
        {
            AtomViewList.Add(toAdd.View);
            NodeViewModelList.Add(toAdd);
            toAdd.Transform = new MatrixTransform();
            ArrangeNodes();
            Canvas.SetZIndex(toAdd.View, 10);

            //TODO Handle links
        }
        
        public void ArrangeNodes()
        {
            this.Width = Constants.MinNodeSizeX;
            this.Height = Constants.MinNodeSizeY;

            var currentX = 75.0;
            var currentY = 75.0;
            for (var i = 0; i < AtomViewList.Count;i++) {
                var toArr = NodeViewModelList[i];
                var mat = toArr.Transform.Matrix;
                mat.OffsetX = currentX;
                mat.OffsetY = currentY;
                toArr.Transform.Matrix = mat;

                if (Height < currentY + toArr.Height + 75)
                {
                    Height = currentY + toArr.Height + 75;
                }
                if (Width < currentX + toArr.Width + 75)
                {
                    Width = currentX + toArr.Width + 75;
                }
                if (i % 3 == 2)
                {
                    currentX = 75;
                    currentY += toArr.Height + 75;
                }
                else
                {
                    currentX += toArr.Width + 75;
                }
            }
        }

        public void RemoveNode(NodeViewModel toRemove)
        {
            this.AtomViewList.Remove(toRemove.View);
            NodeViewModelList.Remove(toRemove);
            ArrangeNodes();
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
    }
}