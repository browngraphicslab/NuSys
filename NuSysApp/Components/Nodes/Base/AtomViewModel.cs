using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class AtomViewModel : BaseINPC
    {
        #region Private Members      

        private int _anchorX, _anchorY;
        private Point2d _anchor;

        protected bool _isSelected, _isMultiSelected, _isVisible;
        private UserControl _view;
        private CompositeTransform _transform = new CompositeTransform();

        #endregion Private Members

        protected AtomViewModel(AtomModel model)
        {
            LinkList = new ObservableCollection<LinkViewModel>();
            IsVisible = true;
            Model = model;
            Model.CanEditChange += OnCanEditChange;
            Model.MetadataChange += OnMetadataChange;
        }

        private void OnMetadataChange(object source, string key)
        {
            if (key == "linksTo")
            {
                Debug.WriteLine("linkTo");
            }
        }

        private void OnCanEditChange(object source, CanEditChangedEventArg e)
        {
            CanEdit = Model.CanEdit;
        }

        #region Atom Manipulations

        public virtual void Dispose()
        {
            Model.CanEditChange -= OnCanEditChange;
            Model.MetadataChange -= OnMetadataChange;
        }
        
        public void ToggleSelection()
        {
            SetSelected(!IsSelected);

            if (IsSelected)
                SessionController.Instance.ActiveWorkspace.SetSelection(this);
        }
  
        public void AddLink(LinkViewModel linkVm)
        {
            LinkList.Add(linkVm);
            UpdateAnchor();
        }

        public abstract void Remove();

        #endregion

        #region Other Methods
     
        public abstract void UpdateAnchor();

        #endregion Other Methods
        
        #region Public Properties

        public ObservableCollection<LinkViewModel> LinkList { get; set; }
        
        private AtomModel.EditStatus _canEdit;
        public AtomModel.EditStatus CanEdit
        {
            get { return _canEdit; }
            set
            {
                _canEdit = value;
                RaisePropertyChanged("CanEdit");
                
            }
        }
       
        public bool IsSelected
        {
            get { return _isSelected; }

        }

        public virtual void SetSelected(bool val)
        {
            if (_isSelected == val)
            {
                return;
            }

            _isSelected = val;
            RaisePropertyChanged("IsSelected");
        }

        public bool IsMultiSelected
        {
            get { return _isMultiSelected; }
            set
            {
                if (_isMultiSelected == value)
                {
                    return;
                }

                _isMultiSelected = value;
                RaisePropertyChanged("IsMultiSelected");
            }
        }

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

        public string Id
        {
            get { return Model.Id; }
            set { Model.Id = value; }
        }

        public CompositeTransform Transform
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
       
        
        public Point2d Anchor
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

        public AtomModel Model { get;}

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