using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class AtomViewModel : BaseINPC
    {
        #region Private Members      

        //anchor points are centers of nodes
        private int _anchorX, _anchorY;
        private Point _anchor;

        private bool _isSelected;
        private UserControl _view;
        private MatrixTransform _transform;
        public bool _isVisible;

        #endregion Private Members

        protected AtomViewModel(Atom model, WorkspaceViewModel vm, string id)
        {
            WorkSpaceViewModel = vm;
            LinkList = new ObservableCollection<LinkViewModel>();
            this.IsVisible = true;
            this.Model = model;
            this.Model.OnCanEditChanged += CanEditChangedHandler;
            ((Atom)this.Model).OnLinked += LinkedHappend;
        }

        private void LinkedHappend(object source, LinkedEventArgs e)
        {
            WorkSpaceViewModel.PrepareLink(e.ID, this, e.Link);
        }

        private void CanEditChangedHandler(object source, CanEditChangedEventArg e)
        {
            CanEdit = Model.CanEdit;
        }

        #region Atom Manipulations

        public void SetVisibility(bool visible)
        {
            this.IsVisible = visible;
            foreach (var link in LinkList)
            {
                link.SetVisibility(visible);
            }
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

        private Atom.EditStatus _canEdit;
        public Atom.EditStatus CanEdit
        {
            get { return _canEdit; }
            set
            {
                _canEdit = value;
                RaisePropertyChanged("CanEdit");
                if (_canEdit == Atom.EditStatus.No)
                {
                    this.Color = new SolidColorBrush(Colors.Chartreuse);
                }
                else if (_canEdit == Atom.EditStatus.Yes)
                {
                    this.Color = new SolidColorBrush(Colors.DarkOrange);
                }
                else
                {
                    this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 255, 235, 205));
                    //if(_canEdit == Atom.EditStatus.Yes)
                    //{
                    //    this.Color = new SolidColorBrush(Constants.DefaultColor);
                    //}
                }
            }
        }
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
        /// color of the atom
        /// </summary>
        public SolidColorBrush Color
        {
            get { return Model.Color; }
            set
            {
                if (Model.Color == value)
                {
                    return;
                }

                Model.Color = value;
                
                RaisePropertyChanged("Color");
            }
        }

        public string ID
        {
            get { return Model.ID; }
            set { Model.ID = value; }
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

        public Atom Model { get;}

        public String AtomType { get; set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value)
                {
                    return;
                }
                _isVisible = value;
                RaisePropertyChanged("IsVisible");
            }
        }
        #endregion Public Properties
    }
}