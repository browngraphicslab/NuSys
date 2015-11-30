using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace NuSysApp
{
    public class GroupViewModel: NodeViewModel
    {
        public ObservableCollection<UserControl> AtomViewList { get; }

        protected INodeViewFactory _nodeViewFactory = new FreeFormNodeViewFactory();
        public delegate Task ChildAddedHandler(object source, AnimatableUserControl node);
        public event ChildAddedHandler ChildAdded;

        private double _margin;
       // private CompositeTransform _localTransform;

        public GroupViewModel(GroupModel model): base(model)
        {
            AtomViewList = new ObservableCollection<UserControl>();
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
           
            NodeType = NodeType.Group;
            _margin = 75;

            model.ChildAdded += OnChildAdded;
            model.ChildRemoved += OnChildRemoved;
        }

        public virtual async Task OnChildAdded(object source, Sendable nodeModel)
        {
            var view = await _nodeViewFactory.CreateFromSendable(nodeModel, AtomViewList.ToList());
            AtomViewList.Add(view);
            ChildAdded?.Invoke(this, (AnimatableUserControl)view);
        }

        private void OnChildRemoved(object source, Sendable sendable)
        {
            if (sendable is InqLineModel)
            {
                var inqLineModel = (InqLineModel) sendable;
                inqLineModel.Delete();
                return;
            }

            var view = AtomViewList.Where((a => { var vm = (AtomViewModel)a.DataContext; return vm.Model == sendable; }));
            if (view.Count() > 0)
                AtomViewList.Remove(view.First());
        }

        
        public override void Resize(double dx, double dy)
        {
            // TODO: re-add
            /*
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
            */
          //  base.Resize(newDx, newDy);
        }

        public void RemoveNode(NodeViewModel toRemove)
        {/*
            foreach (var link in toRemove.LinkList)
            {
                link.IsVisible = true;
                link.UpdateAnchor();
            }
            toRemove.UpdateAnchor();
            AtomViewList.Remove(toRemove.View);
            NodeViewModelList.Remove(toRemove);
            ((GroupNodeModel)Model).NodeModelList.Remove((NodeModel)toRemove.Model);

            if (NodeViewModelList.Count == 1)
            {
                var lastNode = NodeViewModelList[0];
                var nodeModel = (NodeModel)lastNode.Model;
                nodeModel.MoveToGroup(null);
               // var x = lastNode.Transform.Matrix.OffsetX * lastNode.ParentGroup.LocalTransform.ScaleX;
               // var y = lastNode.Transform.Matrix.OffsetY * lastNode.ParentGroup.LocalTransform.ScaleY;
               // WorkSpaceViewModel.PositionNode(lastNode, this.Transform.Matrix.OffsetX + x, this.Transform.Matrix.OffsetY + y);
                //WorkSpaceViewModel.DeleteNode(this);
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
            */          
        }

        
    }
}