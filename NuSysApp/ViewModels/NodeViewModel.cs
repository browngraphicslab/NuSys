﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using Windows.Foundation;
using Windows.UI;
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

        private bool _isEditing,_isEditingInk;
        private AtomViewModel _clippedParent;
        #endregion Private Members

        protected NodeViewModel(WorkspaceViewModel vm): base(vm)
        {
            this.AtomType = Constants.Node;
            this.Model = new Node(0);
        }

        #region Node Manipulations

        public override void Remove()
        {
            WorkSpaceViewModel.DeleteNode(this);
            if (this.IsSelected)
            {
                WorkSpaceViewModel.ClearSelection();
            }
        }

        public virtual void Translate(double dx, double dy)
        {
            if (IsAnnotation){return;}
            if (!this.IsEditing)
            {
                var transMat = ((MatrixTransform) this.View.RenderTransform).Matrix;
                if (ParentGroup == null)
                {
                transMat.OffsetX += dx / WorkSpaceViewModel.CompositeTransform.ScaleX;
                transMat.OffsetY += dy / WorkSpaceViewModel.CompositeTransform.ScaleY;
                } else
                {
                    transMat.OffsetX += dx / WorkSpaceViewModel.CompositeTransform.ScaleX / ParentGroup.LocalTransform.ScaleX ;
                transMat.OffsetY += dy / WorkSpaceViewModel.CompositeTransform.ScaleY/ ParentGroup.LocalTransform.ScaleX;
                }
                Transform = new MatrixTransform();
                this.Transform.Matrix = transMat;
                this.UpdateAnchor();
                foreach (var link in LinkList)
                {
                    link.UpdateAnchor();
                }
            }
            
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
            this.AnchorX = (int) (this.X + this.Transform.Matrix.OffsetX + this.Width/2); //this is the midpoint
            this.AnchorY = (int) (this.Y + this.Transform.Matrix.OffsetY + this.Height/2);
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
                this.IsAnnotation = true;
            }
        }
        #endregion Node Manipulations

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

        public bool IsAnnotation { get; set; }

        /// <summary>
        /// X-coordinate of this atom
        /// </summary>
        public int X
        {
            get { return ((Node)Model).X; }
            set
            {
                if (((Node)Model).X == value)
                {
                    return;
                }
                ((Node)Model).X = value;
                RaisePropertyChanged("X");
            }
        }

        /// <summary>
        /// Y-coordinate of this atom
        /// </summary>
        public int Y
        {
            get { return ((Node)Model).Y; }
            set
            {
                if (((Node)Model).Y == value)
                {
                    return;
                }

                ((Node)Model).Y = value;
                RaisePropertyChanged("Y");
            }
        }

        public MatrixTransform Transform
        {
            get { return ((Node)Model).Transform; }
            set
            {
                if (((Node)Model).Transform == value)
                {
                    return;
                }
                ((Node)Model).Transform = value;

                RaisePropertyChanged("Transform");
            }
        }

        /// <summary>
        /// Width of this atom
        /// </summary>
        public double Width
        {
            get { return ((Node)Model).Width; }
            set
            {
                if (((Node)Model).Width == value || value < Constants.MinNodeSize) //prevent atom from getting too small
                {
                    return;
                }
                ((Node)Model).Width = value;

                RaisePropertyChanged("Width");
            }
        }

        /// <summary>
        /// Height of this atom
        /// </summary>
        public double Height
        {
            get { return ((Node)Model).Height; }
            set
            {
                if (((Node)Model).Height == value || value < Constants.MinNodeSize) //prevent atom from getting to small
                {
                    return;
                }

                ((Node)Model).Height = value;

                RaisePropertyChanged("Height");
            }
        }

        public abstract XmlElement WriteXML(XmlDocument doc);

        public List<XmlAttribute> getBasicXML(XmlDocument doc)
        {
            List<XmlAttribute> basicXml = new List<XmlAttribute>();

            //create xml attribute nodes
            XmlAttribute type = doc.CreateAttribute("nodeType");
            type.Value = ((Node)this.Model).NodeType.ToString();

            XmlAttribute id = doc.CreateAttribute("id");
            id.Value = this.Model.ID.ToString();

            XmlAttribute x = doc.CreateAttribute("x");
            x.Value = ((int) ((Node)this.Model).Transform.Matrix.OffsetX).ToString();

            XmlAttribute y = doc.CreateAttribute("y");
            y.Value = ((int) ((Node)this.Model).Transform.Matrix.OffsetY).ToString();

            XmlAttribute height = doc.CreateAttribute("height");
            height.Value = ((int)((Node)this.Model).Height).ToString();

            XmlAttribute width = doc.CreateAttribute("width");
            width.Value = ((int)((Node)this.Model).Width).ToString();

            //append to list and return
            basicXml.Add(type);
            basicXml.Add(id);
            basicXml.Add(x);
            basicXml.Add(y);
            basicXml.Add(height);
            basicXml.Add(width);

            return basicXml;
        }

        

        /// <summary>
        /// indicates whether node is editable.
        /// </summary>
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

        public Constants.NodeType NodeType {
            get { return ((Node)this.Model).NodeType; }
            set
            {
                ((Node)this.Model).NodeType = value;
            }

        }

        public GroupViewModel ParentGroup { get; set; }

        #endregion Public Properties
    }
}