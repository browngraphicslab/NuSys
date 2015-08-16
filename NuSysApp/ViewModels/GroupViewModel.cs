using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class GroupViewModel: NodeViewModel
    {
        private double _margin;
        private CompositeTransform _localTransform;
        public GroupViewModel(WorkspaceViewModel vm, string id): base(vm, id)
        {
            AtomViewList = new ObservableCollection<UserControl>();
            NodeViewModelList = new ObservableCollection<NodeViewModel>();
            LinkViewModelList = new ObservableCollection<LinkViewModel>();
            this.AtomType = Constants.Node;
            this.Model = new Node(id);
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
            this.View = new GroupView(this);
            this.NodeType = Constants.NodeType.group;
            _margin = 75;
            this.LocalTransform = new CompositeTransform();
        }

        public void AddNode(NodeViewModel toAdd)
        {
            toAdd.Transform = new MatrixTransform();
            AtomViewList.Add(toAdd.View);
            NodeViewModelList.Add(toAdd);
          //  ArrangeNodesInGrid();
            foreach (var link in toAdd.LinkList)
            {
                link.SetVisibility(false);
            }
            //TODO Handle links
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
            this.AtomViewList.Remove(toRemove.View);
            NodeViewModelList.Remove(toRemove);
         //   ArrangeNodesInGrid();
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
                        link.SetVisibility(true);
                        link.UpdateAnchor();
                        link.Atom1.UpdateAnchor();
                        link.Atom2.UpdateAnchor();
                    }
                    lastNode.UpdateAnchor();
                    break;
            }
            //TODO Handle links
        }

        public bool CheckNodeIntersection(NodeViewModel node) { 
            for (var i = 0; i < NodeViewModelList.Count; i++)
            {
                var node2 = NodeViewModelList[i];
                var rect2 = Geometry.NodeToBoudingRect(node2);
                var rect1 = Geometry.NodeToBoudingRect(node);
                rect1.Intersect(rect2);//stores intersection rectangle in rect1
                if (node != node2 && !rect1.IsEmpty)
                {
                    NodeViewModelList.Remove(node);
                    AtomViewList.Remove(node.View);
                    if (node.X + node.Transform.Matrix.OffsetX > node2.X + node2.Transform.Matrix.OffsetX)
                    {
                        if (NodeViewModelList.Count <= i+1)
                        {
                            NodeViewModelList.Add(node);
                            AtomViewList.Add(node.View);
                            return true;
                        }
                        NodeViewModelList.Insert(i+1, node);
                        AtomViewList.Insert(i+1,node.View);
                    }
                    else
                    {
                        NodeViewModelList.Insert(i, node);
                        AtomViewList.Insert(i,node.View);
                    }
                    return true;
                }
            }
            return false;
        }

        public override XmlElement WriteXML(XmlDocument doc)
        {
            Atom currModel = this.Model;

            //Main XmlElement 
            XmlElement groupNode = doc.CreateElement(string.Empty, "Node", string.Empty); //TODO: Change how we determine node type for name
            

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach (XmlAttribute attr in basicXml)
            {
                groupNode.SetAttributeNode(attr);
            }

            //get nodes within groups
            foreach(NodeViewModel nodevm in NodeViewModelList)
            {
                groupNode.AppendChild(nodevm.WriteXML(doc));
            }
            return groupNode;
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