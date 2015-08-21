using NuSysApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml;
using Windows.UI;
using Windows.UI.Xaml;
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


        public GroupViewModel(WorkspaceViewModel vm, int id): base(vm, id)
        {
            this.AtomType = Constants.Node;
            this.Model = new Group(id);
            this.Model.ID = id;
            _nodeViewModelList = new ObservableCollection<NodeViewModel>();
            _linkViewModelList = new ObservableCollection<LinkViewModel>();
            _atomViewList = new ObservableCollection<UserControl>();
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 199, 235, 189));
            this.View = new GroupView(this);
            this.NodeType = Constants.NodeType.Group;
            _margin = 75;
            this.LocalTransform = new CompositeTransform();
        }

        public void AddNode(NodeViewModel toAdd)
        {
            Color opaque = toAdd.Color.Color;
            opaque.A = 255;
            toAdd.Color = new SolidColorBrush(opaque);
            toAdd.Transform = new MatrixTransform();
            _atomViewList.Add(toAdd.View);
            _nodeViewModelList.Add(toAdd);
            ((Group)Model).NodeModelList.Add((Node)toAdd.Model);
          //  ArrangeNodesInGrid();
            foreach (var link in toAdd.LinkList)
            {
                link.SetVisibility(false);
                if (link.Annotation != null)
                {
                    link.Annotation.IsVisible = false;
                }
            }
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
            ((Group)Model).NodeModelList.Remove((Node)toRemove.Model);
            Color translucent = toRemove.Color.Color;
            translucent.A = 175;
            toRemove.Color = new SolidColorBrush(translucent);

            //   ArrangeNodesInGrid();
            switch (_nodeViewModelList.Count)
            {
                case 0:
                    WorkSpaceViewModel.DeleteNode(this);
                    break;
                case 1:
                    var lastNode = _nodeViewModelList[0];
                    _atomViewList.Remove(lastNode.View);
                    _nodeViewModelList.Remove(lastNode);
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
            for (var i = 0; i < _nodeViewModelList.Count; i++)
            {
                var node2 = _nodeViewModelList[i];
                var rect2 = Geometry.NodeToBoudingRect(node2);
                var rect1 = Geometry.NodeToBoudingRect(node);
                rect1.Intersect(rect2);//stores intersection rectangle in rect1
                if (node != node2 && !rect1.IsEmpty)
                {
                    _nodeViewModelList.Remove(node);
                    _atomViewList.Remove(node.View);
                    if (node.X + node.Transform.Matrix.OffsetX > node2.X + node2.Transform.Matrix.OffsetX)
                    {
                        if (_nodeViewModelList.Count <= i+1)
                        {
                            _nodeViewModelList.Add(node);
                            _atomViewList.Add(node.View);
                            return true;
                        }
                        _nodeViewModelList.Insert(i+1, node);
                        _atomViewList.Insert(i+1,node.View);
                    }
                    else
                    {
                        _nodeViewModelList.Insert(i, node);
                        _atomViewList.Insert(i,node.View);
                    }
                    return true;
                }
            }
            return false;
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