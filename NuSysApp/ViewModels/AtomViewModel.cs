using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class AtomViewModel : BaseINPC
    {
        #region Private Members      

        private int _x, _y;
        private double _width, _height;

        //anchor points are centers of nodes
        private int _anchorX, _anchorY;
        private Point _anchor;

        private bool _isSelected, _isEditing;
        private UserControl _view;
        private MatrixTransform _transform;
        #endregion Private Members

        protected AtomViewModel(WorkspaceViewModel vm)
        {
            WorkSpaceViewModel = vm;
            LinkList = new ObservableCollection<LinkViewModel>();
        }

        protected AtomViewModel(WorkspaceViewModel vm, double x, double y, double width, double height):this(vm)
        {
            this.X = (int)x;
            this.Y = (int)y;
            this.Width = width;
            this.Height = height;
        }

        #region Atom Manipulations

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

        /// <summary>
        /// Adds a link to this atom.
        /// </summary>
        public void AddLink(LinkViewModel linkVm)
        {
            this.LinkList.Add(linkVm);
        }

        /// <summary>
        /// Removes this atom and all references to it.
        /// </summary>
        public abstract void Remove();

        #endregion

        #region Other Methods
     
        /// <summary>
        /// Updates the anchor point of an atom. 
        /// </summary>
        public abstract void UpdateAnchor();

        #endregion Other Methods

        #region Public Properties

        /// <summary>
        /// Holds a reference to all LinkViewModels that the atom is linked to.
        /// </summary>
        public ObservableCollection<LinkViewModel> LinkList { get; set; }

        

        /// <summary>
        /// Accessor only reference to the workspace in which the atom is contained
        /// </summary>
        public WorkspaceViewModel WorkSpaceViewModel { get; }

        private AtomViewModel _clippedParent;
        public AtomViewModel ClippedParent
        {
            get { return _clippedParent; }
            set
            {
                if (_clippedParent == null)
                {
                    _clippedParent = value;
                    _clippedParent.PropertyChanged += parent_PropertyChanged;
                    parent_PropertyChanged(null, null);
                    this.Width = Constants.DefaultAnnotationSize*2;
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
            transMat.OffsetX = ClippedParent.AnchorX - this.Width/2 ;
            transMat.OffsetY = ClippedParent.AnchorY - this.Height/2;
            Transform = new MatrixTransform();
            this.Transform.Matrix = transMat;
        }

        public bool IsAnnotation { get; set; }

        /// <summary>
        /// indicates whether node is selected.
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

        /// <summary>
        /// sets and gets view, to be applied specifically in the child classes of nodeviewmodel.
        /// </summary>
        public UserControl View
        {
            get { return _view; }
            set
            {
                if (_view == value)
                {
                    return;
                }

                _view = value;

                RaisePropertyChanged("View");
            }
        }

        /// <summary>
            /// X-coordinate of this atom
            /// </summary>
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

        /// <summary>
        /// Y-coordinate of this atom
        /// </summary>
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
        /// Width of this atom
        /// </summary>
        public double Width
        {
            get { return _width; }
            set
            {
                if (_width == value || value < Constants.MinNodeSize) //prevent atom from getting to small
                {
                    return;
                }

                _width = value;

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

                RaisePropertyChanged("Height");
            }
        }


        /// <summary>
        /// X-coordinate of this atom's anchor
        /// </summary>
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

        /// <summary>
        /// Y-coordinate of this atom's anchor
        /// </summary>
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
        /// This atom's anchor point
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