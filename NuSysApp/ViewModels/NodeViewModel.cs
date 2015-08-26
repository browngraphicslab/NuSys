
using System;
using System.ComponentModel;
using System.Xml;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    /// <summary>
    /// 
    /// NodeViewModel class
    /// 
    /// Parameters: takes in the workspace
    /// </summary>
    public abstract class NodeViewModel : AtomViewModel
    {
        #region Private Members      

        private bool _isEditing, _isEditingInk;
        private AtomViewModel _clippedParent;
        private MatrixTransform _transform;
        private GroupViewModel _group;

        #endregion Private Members
        protected NodeViewModel(Node model, WorkspaceViewModel vm, string id): base(model, vm, id)
        {
            this.AtomType = Constants.Node;       
            ((Node)this.Model).OnDeletion += DeletionHappend;         
            ((Node) this.Model).OnLocationUpdate += LocationUpdateHandler;
            ((Node) this.Model).OnWidthHeightUpdate += WidthHeightChangedHandler;
           
            ((Node) this.Model).OnAddToGroup += AddToGroupHandler;
            X = 0;
            Y = 0;
        }

        private void AddToGroupHandler(object source, AddToGroupEventArgs e)
        {
            var groupVm = WorkSpaceViewModel.GroupDict[e.Group.ID];
            groupVm.AddNode(this);
            this.ParentGroup = groupVm;
        }
       
        #region Node Manipulations

        public override void Remove()
        {
            NetworkConnector.Instance.RequestDeleteAtom(ID);
            //WorkSpaceViewModel.DeleteNode(this);
            if (this.IsSelected)
            {
                WorkSpaceViewModel.ClearSelection();
            } 
        }

        public virtual void Translate(double dx, double dy)
        {
            if (IsAnnotation) { return; }
            if (!this.IsEditing)
            {
                var transMat = ((MatrixTransform)this.View.RenderTransform).Matrix;
                if (ParentGroup == null)
                {
                    transMat.OffsetX += dx / WorkSpaceViewModel.CompositeTransform.ScaleX;
                    transMat.OffsetY += dy / WorkSpaceViewModel.CompositeTransform.ScaleY;
                }
                else
                {
                    transMat.OffsetX += dx / WorkSpaceViewModel.CompositeTransform.ScaleX / ParentGroup.LocalTransform.ScaleX;
                    transMat.OffsetY += dy / WorkSpaceViewModel.CompositeTransform.ScaleY / ParentGroup.LocalTransform.ScaleX;
                }
                Transform = new MatrixTransform();
                this.Transform.Matrix = transMat;
                ((Node)Model).X = transMat.OffsetX;
                ((Node)Model).Y = transMat.OffsetY;
                this.UpdateAnchor();
                foreach (var link in LinkList)
                {
                    link.UpdateAnchor();
                }
            }
        }

        /// <summary>
        /// Behaves like Translate(dx, dy) but sets the position to absolute coordinates instead.
        /// Currently used to visually update location coordinates from the network.
        /// //TODO does not yet take into account scale
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetPosition(double x, double y)
        {
            var transMat = ((MatrixTransform)View.RenderTransform).Matrix;
            transMat.OffsetX = x;
            transMat.OffsetY = y;
            this.Transform = new MatrixTransform
            {
                Matrix = transMat
            };
            this.X = 0;
            this.Y = 0;
            foreach (var link in LinkList)
            {
                link.UpdateAnchor();
            }
            this.UpdateAnchor();
            RaisePropertyChanged("Transform");
        }

        /// <summary>
        /// toggles editing ability of nodes.
        /// </summary>
        public void ToggleEditing()
        {
            this.IsEditing = !this.IsEditing;
        }
        public void ToggleEditingInk()
        {
            this.IsEditingInk = !this.IsEditingInk;
        }

        /// <summary>
        /// Updates the anchor points (central points) of the node when it is transformed. Also updates the attached links.
        /// </summary>
        public override void UpdateAnchor()
        {
            this.AnchorX = (int)(this.X + this.Transform.Matrix.OffsetX + this.Width / 2); //this is the midpoint
            this.AnchorY = (int)(this.Y + this.Transform.Matrix.OffsetY + this.Height / 2);
            this.Anchor = new Point(this.AnchorX, this.AnchorY);
        }

        /// <summary>
        /// Resizes the node. Eventually this should use a scale transformation instead.
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public virtual void Resize(double dx, double dy)
        {
            var changeX = dx / WorkSpaceViewModel.CompositeTransform.ScaleX;
            var changeY = dy / WorkSpaceViewModel.CompositeTransform.ScaleY;
            if (this.Width > Constants.MinNodeSizeX || changeX > 0)
            {
                this.Width += changeX;
            }
            if (this.Height > Constants.MinNodeSizeY || changeY > 0)
            {
                this.Height += changeY;
            }
            this.UpdateAnchor();
        }

        public void CreateAnnotation()
        {
            if (this.LinkList.Count > 0) return;

            if (this.WorkSpaceViewModel.CheckForNodeLinkIntersections(this))
            {
                ((Node)this.Model).IsAnnotation = true;
            }
        }
        #endregion Node Manipulations

        #region Event Handlers
        private void DeletionHappend(object source, DeleteEventArgs e)
        {
            this.WorkSpaceViewModel.DeleteNode(this);
        }

        private void LocationUpdateHandler(object source, LocationUpdateEventArgs e)
        {
            this.SetPosition(((Node)this.Model).X, ((Node)this.Model).Y);
            this.UpdateAnchor();
        }

        private void WidthHeightChangedHandler(object source, WidthHeightUpdateEventArgs e)
        {
            this.Width = ((Node)this.Model).Width;
            this.Height = ((Node)this.Model).Height;
            this.UpdateAnchor();
        }
        #endregion Event Handlers

        #region XML methods
        public XmlElement WriteXML(XmlDocument doc)
        {
            return ((Node)Model).WriteXML(doc);
        }
        #endregion XML methods

        #region Public Properties

        public AtomViewModel ClippedParent//TODO move to link
        {
            get { return _clippedParent; }
            set
            {
                if (_clippedParent == null)
                {
                    _clippedParent = value;
                    _clippedParent.PropertyChanged += parent_PropertyChanged;
                    parent_PropertyChanged(null, null);
                    this.Width = Constants.DefaultAnnotationSize * 2;
                    this.Height = Constants.DefaultAnnotationSize;
                }
                else
                {
                    _clippedParent = value;
                }
            }
        }

        private void parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var transMat = ((MatrixTransform)this.View.RenderTransform).Matrix;
            transMat.OffsetX = ClippedParent.AnchorX - this.Width / 2;
            transMat.OffsetY = ClippedParent.AnchorY - this.Height / 2;
            Transform = new MatrixTransform();
            this.Transform.Matrix = transMat;
        }

        public bool IsAnnotation
        {
            get { return ((Node)Model).IsAnnotation; }
            set { ((Node)Model).IsAnnotation = value; }
        }

        public string id
        {
            get { return Model.ID; }
            set { Model.ID = value; }
        }

        private double _x, _y;
       

        public MatrixTransform Transform
        {
            get { return _transform; }
            set
            {
                if (_transform == value)
                {
                    return;
                }
                _transform = value;
                RaisePropertyChanged("Transform");
            }
        }

        private double _height, _width;
        /// <summary>
        /// Width of this atom
        /// </summary>
        public double Width
        {
            get { return _width; }
            set
            {
                if (_width == value || value < Constants.MinNodeSize) //prevent atom from getting too small
                {
                    return;
                }
                _width = value;
                ((Node)Model).Width = value;
         
                RaisePropertyChanged("Width");
            }
        }

        /// <summary>
        /// Height of this atom
        /// </summary>
        public double Height
        {
            get { return _height; }
            set
            {
                if (_height == value || value < Constants.MinNodeSize) //prevent atom from getting to small
                {
                    return;
                }
                _height = value;
                ((Node)Model).Height = value;
                RaisePropertyChanged("Height");
            }
        }

        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                if (_isEditing == value)
                {
                    return;
                }
                _isEditing = value;
                RaisePropertyChanged("IsEditing");
            }
        }
        public bool IsEditingInk
        {
            get { return _isEditingInk; }
            set
            {
                if (_isEditingInk == value)
                {
                    return;
                }
                _isEditingInk = value;
                RaisePropertyChanged("IsEditingInk");
            }
        }

        public NodeType NodeType
        {
            get { return ((Node)this.Model).NodeType; }
            set
            {
                ((Node)this.Model).NodeType = value;
            }
        }

        public GroupViewModel ParentGroup
        {
            get
            {
                return _group;
            }
            set
            {
                _group = value;
                if (_group != null)
                {
                    ((Node)Model).ParentGroup = (Group)_group.Model;
                }          
            }
        }

         /// <summary>
        /// DEPRICATED X-coordinate of this atom
        /// </summary>
        [Obsolete("Use X and Y at model level only", false)]
        public double X
        {
            get { return _x; }
            private set
            {
                if (_x == value)
                {
                    return;
                }
                _x = value;
                RaisePropertyChanged("X");
            }
        }

        /// <summary>
        /// DEPRICATED Y-coordinate of this atom
        /// </summary>      
        [Obsolete("Use X and Y at model level only", false)]
        public double Y
        {
            get { return _y; }
            private set
            {
                if (_y == value)
                {
                    return;
                }
                _y = value;
                RaisePropertyChanged("Y");
            }
        }
        #endregion Public Properties
    }
}