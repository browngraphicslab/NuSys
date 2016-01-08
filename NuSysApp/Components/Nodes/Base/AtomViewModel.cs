using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class AtomViewModel : BaseINPC
    {
        #region Private Members      
        private double _x, _y, _height, _width, _alpha;
        private int _anchorX, _anchorY;
        private Point2d _anchor;
        private string _title;

        protected bool _isSelected, _isMultiSelected, _isVisible;
        private UserControl _view;
        private CompositeTransform _transform = new CompositeTransform();
        private DebouncingDictionary _debouncingDictionary;
        
        #endregion Private Members

        protected AtomViewModel(AtomModel model)
        {
            LinkList = new ObservableCollection<LinkViewModel>();
            IsVisible = true;
            Model = model;
            Model.CanEditChange += OnCanEditChange;
            Model.MetadataChange += OnMetadataChange;
            model.PositionChanged += OnPositionChanged;
            model.SizeChanged += OnSizeChanged;
            model.ScaleChanged += OnScaleChanged;
            model.AlphaChanged += OnAlphaChanged;
            model.MetadataChange += OnMetadataChange;
            model.TitleChanged += OnTitleChanged;

            Transform.TranslateX = model.X;
            Transform.TranslateY = model.Y;
            Transform.ScaleX = model.ScaleX;
            Transform.ScaleY = model.ScaleY;

            Width = model.Width;
            Height = model.Height;
            Alpha = model.Alpha;
            Title = model.Title;
            X = model.X;
            Y = model.Y;
            _debouncingDictionary = new DebouncingDictionary(model.Id);
        }

        private void OnTitleChanged(object source, string title)
        {
            _title = title;
            RaisePropertyChanged("Title");
        }

        protected virtual void OnPositionChanged(object source, PositionChangeEventArgs e)
        {
            SetPosition(Model.X, Model.Y);
            UpdateAnchor();
            _debouncingDictionary?.Add("x",Model.X);
            _debouncingDictionary?.Add("y", Model.Y);
        }

        protected virtual void OnSizeChanged(object source, WidthHeightUpdateEventArgs e)
        {
            Width = Model.Width;
            Height = Model.Height;
            UpdateAnchor();
            _debouncingDictionary?.Add("width", Width);
            _debouncingDictionary?.Add("height", Height);
        }

        protected virtual void OnScaleChanged(object source)
        {
            Transform.ScaleX = Model.ScaleX;
            Transform.ScaleY = Model.ScaleY;
            RaisePropertyChanged("Transform");
        }

        protected virtual void OnCanEditChange(object source, CanEditChangedEventArg e)
        {
            CanEdit = Model.CanEdit;
        }
        protected virtual void OnAlphaChanged(object source)
        {
            Alpha = Model.Alpha;
        }
        protected virtual void OnMetadataChange(object source, string key)
        {
            Tags = string.Join(",", Model.GetMetaData("tags") as List<string>);
            RaisePropertyChanged("Tags");
        }

        #region Atom Manipulations

        public virtual void Dispose()
        {
            Model.CanEditChange -= OnCanEditChange;
            Model.MetadataChange -= OnMetadataChange;
            Model.PositionChanged -= OnPositionChanged;
            Model.SizeChanged -= OnSizeChanged;
            Model.ScaleChanged -= OnScaleChanged;
            Model.AlphaChanged -= OnAlphaChanged;
            Model.MetadataChange -= OnMetadataChange;
        }

        public virtual void Translate(double dx, double dy)
        {
            Model.X += dx / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleX;
            Model.Y += dy / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleY;
            UpdateAnchor();
        }

        public virtual void SetPosition(double x, double y)
        {
            Transform.TranslateX = x;
            Transform.TranslateY = y;

            UpdateAnchor();
            RaisePropertyChanged("Transform");
        }

        public virtual void Resize(double dx, double dy)
        {
            var changeX = dx / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleX;
            var changeY = dy / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleY;
            if (Width > Constants.MinNodeSizeX || changeX > 0)
            {
                SetSize(Width + changeX, Height);
            }
            if (Height > Constants.MinNodeSizeY || changeY > 0)
            {
                SetSize(Width, Height + changeY);
            }

        }

        public virtual void SetSize(double width, double height)
        {
            Width = width;
            Height = height;
            UpdateAnchor();
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

        public virtual void UpdateAnchor()
        {
            Anchor = new Point2d(Transform.TranslateX + Width / 2, Transform.TranslateY + Height / 2);
            foreach (var link in LinkList)
            {
                link.UpdateAnchor();
            }
        }

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

        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
                Model.X = value;
                RaisePropertyChanged("X");
            }
        }

        public double Y
        {
            get { return _y; }
            set
            {
                _y = value;
                Model.Y = value;
                RaisePropertyChanged("Y");
            }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                if (_width == value || value < Constants.MinNodeSize) //prevent atom from getting too small
                    return;
                _width = value;
                Model.Width = value;
                RaisePropertyChanged("Width");
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                if (_height == value || value < Constants.MinNodeSize)
                    return;

                _height = value;
                Model.Height = value;
                RaisePropertyChanged("Height");
            }
        }

        public double Alpha
        {
            get { return _alpha; }
            set
            {
                _alpha = value;
                //Model.Alpha = value;
                RaisePropertyChanged("Alpha");
            }
        }

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

        public string Title
        {
            get { return _title; }
            set { Model.Title = value; }
        }

        public string Tags { get; set; }
        #endregion Public Properties
    }
}