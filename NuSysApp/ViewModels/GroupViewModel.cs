using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace NuSysApp
{
    public class GroupViewModel: NodeViewModel
    {
        private double _margin;
        private CompositeTransform _localTransform;
        private ObservableCollection<UserControl> _atomViewList;
        public ObservableCollection<NodeViewModel> _nodeViewModelList;
        public ObservableCollection<LinkViewModel> _linkViewModelList;


        public GroupViewModel(UserControl view, GroupNodeModel model, WorkspaceViewModel vm): base(model,vm)
        {
            this.AtomType = Constants.Node;
            _nodeViewModelList = new ObservableCollection<NodeViewModel>();
            _linkViewModelList = new ObservableCollection<LinkViewModel>();
            _atomViewList = new ObservableCollection<UserControl>();
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize;   //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
            this.View = view;
            this.NodeType = NodeType.Group;
            _margin = 75;
            this.LocalTransform = new CompositeTransform();
        }

        public void AddNode(NodeViewModel toAdd)
        {
            if (toAdd as GroupViewModel == null) {
                toAdd.Width = Constants.DefaultNodeSize + 20;//TODO CHANGE THIS
                toAdd.Height = Constants.DefaultNodeSize + 20;//TODO CHANGE THIS
            }

            if (toAdd.ParentGroup == null) //node is currently in workspace
            {
                WorkSpaceViewModel.AtomViewList.Remove(toAdd.View);
                WorkSpaceViewModel.NodeViewModelList.Remove(toAdd);
            }
            else
            {
                toAdd.ParentGroup.AtomViewList.Remove(toAdd.View);
                WorkSpaceViewModel.NodeViewModelList.Remove(toAdd);
            }
            toAdd.Transform = new MatrixTransform();
            _atomViewList.Add(toAdd.View);
            _nodeViewModelList.Add(toAdd);
            
            foreach (var link in toAdd.LinkList)
            {
                link.SetVisibility(false);
                if (link.Annotation != null)
                {
                    link.Annotation.IsVisible = false;
                }
            }
            (View as GroupView).ArrangeNodesInGrid();
            //TODO Handle links
        }

        public ObservableCollection<UserControl> AtomViewList
        {
            get { return _atomViewList; }
            set
            {
                _atomViewList = value;
            }
        }

        public ObservableCollection<NodeViewModel> NodeViewModelList
        {
            get { return _nodeViewModelList; }
            set
            {
                _nodeViewModelList = value;
            }
        }
         
        public override void Resize(double dx, double dy)
        {
            var trans = LocalTransform;
            var newDx = 0.0;
            var newDy = 0.0;
            if (dx > dy)
            {
                newDx = dy * Width / Height;
                newDy = dy;
            }
            else
            {
                newDx = dx;
                newDy = dx * Height / Width;
            }
            if (newDx / WorkSpaceViewModel.CompositeTransform.ScaleX + Width <= Constants.MinNodeSizeX || newDy / WorkSpaceViewModel.CompositeTransform.ScaleY + Height <= Constants.MinNodeSizeY)
            {
                return;
            }
            var scale = newDx < newDy ? (Width + newDx/WorkSpaceViewModel.CompositeTransform.ScaleX) / Width : (Height + newDy/ WorkSpaceViewModel.CompositeTransform.ScaleY) / Height;
            trans.ScaleX *= scale;
            trans.ScaleY *= scale;
            LocalTransform = trans;
            
            _margin += newDx;
            (View as GroupView).ArrangeNodesInGrid();
            base.Resize(newDx, newDy);
        }

        public void RemoveNode(NodeViewModel toRemove)
        {
            foreach (var link in toRemove.LinkList)
            {
                link.IsVisible = true;
                link.UpdateAnchor();
            }
            toRemove.UpdateAnchor();
            _atomViewList.Remove(toRemove.View);
            _nodeViewModelList.Remove(toRemove);
            ((GroupNodeModel)Model).NodeModelList.Remove((NodeModel)toRemove.Model);

            if (NodeViewModelList.Count == 1)
            {
                var lastNode = _nodeViewModelList[0];
                var nodeModel = (NodeModel)lastNode.Model;
                nodeModel.MoveToGroup(null);
                var x = lastNode.Transform.Matrix.OffsetX * lastNode.ParentGroup.LocalTransform.ScaleX;
                var y = lastNode.Transform.Matrix.OffsetY * lastNode.ParentGroup.LocalTransform.ScaleY;
                WorkSpaceViewModel.PositionNode(lastNode, this.Transform.Matrix.OffsetX + x, this.Transform.Matrix.OffsetY + y);
                WorkSpaceViewModel.DeleteNode(this);
                //NetworkConnector.Instance.RequestDeleteSendable(this.Model.ID);//TODO use an actual network delete
                foreach (var link in lastNode.LinkList)
                {
                    link.SetVisibility(true);
                    link.UpdateAnchor();
                    link.Atom1.UpdateAnchor();
                    link.Atom2.UpdateAnchor();
                }
                lastNode.UpdateAnchor();
            }               
        }

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