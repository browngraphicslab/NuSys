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
            Model.OnCanEditChanged += CanEditChangedHandler;
        }

        private void CanEditChangedHandler(object source, CanEditChangedEventArg e)
        {
            CanEdit = Model.CanEdit;
        }

        #region Atom Manipulations

        public void SetVisibility(bool visible)
        {
            IsVisible = visible;
            foreach (var link in LinkList)
            {
                link.SetVisibility(visible);
            }
        }
        
        public void ToggleSelection()
        {
            IsSelected = !IsSelected;

            if (IsSelected)
                SessionController.Instance.ActiveWorkspace.SetSelection(this);
        }
  
        public void AddLink(LinkViewModel linkVm)
        {
            LinkList.Add(linkVm);
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
                Color color = this.Color.Color;
                if (_canEdit == AtomModel.EditStatus.No)
                {
                    color.A = 50;
                    Color = new SolidColorBrush(color);
                }
                else if (_canEdit == AtomModel.EditStatus.Yes)
                {
                    color.A = 255;
                    Color = new SolidColorBrush(color);
                }
                else
                {
                    color.A = 175;
                    Color = new SolidColorBrush(color);
                }
            }
        }
       
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