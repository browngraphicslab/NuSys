using System;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.UI;
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
        #region Private Members      

       
        private int _x, _y;
        private double _width, _height;
        private Color _color; //currently unused

        //anchor points are centers of nodes
        private int _anchorX, _anchorY;
        private Point _anchor;
         
        private bool _isSelected, _isEditing;
        private MatrixTransform _transform;
        
        #endregion Private Members

        protected NodeViewModel(WorkspaceViewModel vm)
        {
            WorkSpaceViewModel = vm;
            Linklist = new ObservableCollection<BezierLink>();
            Linelist = new ObservableCollection<LinkView>();
        }

        #region Node Manipulations

        public ObservableCollection<BezierLink> GetLinkList()
        {
            return Linklist;
        }

        public ObservableCollection<LinkView> GetLineList()
        {
            return Linelist;
        }

        public void AddLink(object link)
        {
            if (WorkSpaceViewModel.CurrentLinkMode == WorkspaceViewModel.LinkMode.BEZIERLINK)
            {
                Linklist.Add(link as BezierLink);
            }
            else
            {
                Linelist.Add(link as LinkView);
            }
        }

        public void DeleteNode()
        {
            WorkSpaceViewModel.DeleteNode(this);
        }

        public void Translate(double dx, double dy)
        {
            var transMat = ((MatrixTransform) this.View.RenderTransform).Matrix;
            transMat.OffsetX += dx;
            transMat.OffsetY += dy;
            Transform = new MatrixTransform();
            this.Transform.Matrix = transMat;
            this.UpdateAnchor();
        }

        /// <summary>
        /// updates the anchor points (central points) of the node when it is transformed. Also updates the attached links.
        /// </summary>
        public void UpdateAnchor()
        {
            this.AnchorX = (int) (this.X + this.Transform.Matrix.OffsetX + this.Width/2);
            this.AnchorY = (int) (this.Y + this.Transform.Matrix.OffsetY + this.Height/2);
            this.Anchor = new Point(this.AnchorX, this.AnchorY);
            if (Linklist == null) { return;}
            foreach (var link in Linklist)
            {
                link.UpdateControlPoints();
            }
        }

        public void Resize(double dx, double dy)
        {
            this.Width += dx;
            this.Height += dy;
            this.UpdateAnchor();
        }

        /// <summary>
        /// toggles selection of the node
        /// </summary>
        public void ToggleSelection()
        {
            this.IsSelected = !this.IsSelected;
            WorkSpaceViewModel.SetSelection(this);
        }

        /// <summary>
        /// toggles editing ability of nodes.
        /// </summary>
        public void ToggleEditing()
        {
            this.IsEditing = !this.IsEditing;
        }

        #endregion Node Manipulations

        public Rect GetBoundingBox()
        {
            return new Rect()
            {
                Height = this.Height,
                Width = this.Width,
                X = this.X,
                Y = this.Y
            };           
        }

        #region Public Properties

        //collection of links - linklist is for bezier curves, linelist is for lines
        public ObservableCollection<LinkView> Linelist { get; }
        public ObservableCollection<BezierLink> Linklist { get; }

        public WorkspaceViewModel WorkSpaceViewModel { get; }

        /// <summary>
        /// indicatew whether node is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
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

        /// <summary>
        /// sets and gets view, to be applied specifically in the child classes of nodeviewmodel.
        /// </summary>
        public abstract UserControl View { get; set; }

        public int X
        {
            get { return _x; }
            set
            {
                if (_x == value)
                {
                    return;
                }
                _x = value;
                RaisePropertyChanged("X");
            }
        }

        public int Y
        {
            get { return _y; }
            set
            {
                if (_y == value)
                {
                    return;
                }

                _y = value;
                RaisePropertyChanged("Y");
            }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                if (_width == value || value < Constants.MIN_NODE_SIZE) //prevent node from getting to small
                {
                    return;
                }

                _width = value;

                RaisePropertyChanged("Width");
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                if (_height == value || value < Constants.MIN_NODE_SIZE) //prevent node from getting to small
                {
                    return;
                }

                _height = value;

                RaisePropertyChanged("Height");
            }
        }

        /// <summary>
        /// color of node
        /// </summary>
        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color == value)
                {
                    return;
                }

                _color = value;

                RaisePropertyChanged("Color");
            }
        }

        public int AnchorX
        {
            get { return _anchorX; }
            set
            {
                if (_anchorX == value)
                {
                    return;
                }

                _anchorX = value;

                RaisePropertyChanged("AnchorX");
            }
        }

        public int AnchorY
        {
            get { return _anchorY; }
            set
            {
                if (_anchorY == value)
                {
                    return;
                }

                _anchorY = value;

                RaisePropertyChanged("AnchorY");
            }
        }

        /// <summary>
        /// central point of node.
        /// </summary>
        public Point Anchor
        {
            get { return _anchor; }
            set
            {
                if (_anchor == value)
                {
                    return;
                }
                _anchor = value;
                RaisePropertyChanged("Anchor");
            }
        }

       

        #endregion Public Properties
    }
}