
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.System.Power.Diagnostics;
using Windows.UI.Xaml.Controls;
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
        public string Tags { get; set; }

        private bool _isEditing, _isEditingInk;
        private double _height, _width;
        private AtomViewModel _clippedParent;
        private GroupViewModel _group;
        
        protected NodeViewModel(NodeModel model) : base(model)
        {
            AtomType = Constants.Node;
            ((NodeModel) Model).PositionChanged += LocationUpdateHandler;
            ((NodeModel) Model).SizeChanged += WidthHeightChangedHandler;

            Tags = model.GetMetaData("tags");
            model.MetadataChanged += OnMetadataChanged;
       
        }

        private void OnMetadataChanged(object source, string key)
        {
            Tags = Model.GetMetaData("tags");
            RaisePropertyChanged("Tags");
        }
        
        public virtual async Task Init(UserControl view)
        {
            View = view;
            var nodeModel = (NodeModel)Model;
            SetPosition(nodeModel.X, nodeModel.Y);
            Width = nodeModel.Width;
            Height = nodeModel.Height;
            Tags = Model.GetMetaData("tags");
            RaisePropertyChanged("tags");
        }
        
               
        #region Node Manipulations

        public override void Remove()
        {
            //WorkSpaceViewModel.DeleteNode(this);
            if (this.IsSelected)
            {
                //TODO: re-add
                SessionController.Instance.ActiveWorkspace.ClearSelection();
            } 
        }

        public virtual void Translate(double dx, double dy)
        {
            if (IsAnnotation) { return; }
            if (!this.IsEditing)
            {
             //   var transMat = ((CompositeTransform)this.View.RenderTransform);
             //   transMat.TranslateX += dx / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleX;
             //   transMat.TranslateY += dy / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleY;
                        
             //   Transform = transMat;
                ((NodeModel)Model).X +=  dx / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleX;
                ((NodeModel)Model).Y +=  dy / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleY;
                //Debug.WriteLine("translating...");
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
        public virtual void SetPosition(double x, double y)
        {
            var transMat = ((CompositeTransform)View.RenderTransform);
            transMat.TranslateX = x;
            transMat.TranslateY = y;
            this.Transform = transMat;
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
            AnchorX = (int)(Transform.TranslateX + Width / 2); //this is the midpoint
            AnchorY = (int)(Transform.TranslateY + Height / 2);
            Anchor = new Point(AnchorX, AnchorY);
        }

        /// <summary>
        /// Resizes the node. Eventually this should use a scale transformation instead.
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public virtual void Resize(double dx, double dy)
        {   
            double changeX = dx / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleX;
            double changeY = dy / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleY;
            if (Width > Constants.MinNodeSizeX || changeX > 0)
            {
                Width += changeX;
            }
            if (Height > Constants.MinNodeSizeY || changeY > 0)
            {
                Height += changeY;
            }
            UpdateAnchor();
        }

        public void CreateAnnotation()
        {
            if (LinkList.Count > 0) return;

            //TODO: re-add
            /*
            if (this.WorkSpaceViewModel != null && this.WorkSpaceViewModel.CheckForNodeLinkIntersections(this))
            {
                ((NodeModel)this.Model).IsAnnotation = true;
            }*/
        }
        #endregion Node Manipulations

        #region Event Handlers

        private void LocationUpdateHandler(object source, LocationUpdateEventArgs e)
        {
            SetPosition(((NodeModel)Model).X, ((NodeModel)Model).Y);
            UpdateAnchor();
        }

        private void WidthHeightChangedHandler(object source, WidthHeightUpdateEventArgs e)
        {
            Width = ((NodeModel)Model).Width;
            Height = ((NodeModel)Model).Height;
            UpdateAnchor();
        }
        #endregion Event Handlers

        #region XML methods
        public XmlElement WriteXML(XmlDocument doc)
        {
            return ((NodeModel)Model).WriteXML(doc);
        }
        #endregion XML methods

        #region Public Properties

        public bool IsAnnotation
        {
            get { return ((NodeModel)Model).IsAnnotation; }
            set { ((NodeModel)Model).IsAnnotation = value; }
        }

        public string id
        {
            get { return Model.ID; }
            set { Model.ID = value; }
        }
        
        public virtual double Width
        {
            get { return _width; }
            set
            {
                if (_width == value || value < Constants.MinNodeSize) //prevent atom from getting too small
                {
                    return;
                }
                _width = value;
                ((NodeModel)Model).Width = value;
         
                RaisePropertyChanged("Width");
            }
        }

        /// <summary>
        /// Height of this atom
        /// </summary>
        public virtual double Height
        {
            get { return _height; }
            set
            {
                if (_height == value || value < Constants.MinNodeSize) //prevent atom from getting to small
                {
                    return;
                }
                _height = value;
                ((NodeModel)Model).Height = value;
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
            get { return ((NodeModel)this.Model).NodeType; }
            set
            {
                ((NodeModel)this.Model).NodeType = value;
            }
        }
        
        #endregion Public Properties
    }
}