﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
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
            model.Deleted += OnDeleted;

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
            Tags = new ObservableCollection<Button>();
            _debouncingDictionary = new DebouncingDictionary(model.Id);

            CreateTags();
        }

        private void OnDeleted(object source)
        {
            // TODO: Dispose everything in here.
        }


        public virtual async Task Init(){} 

        private void OnTitleChanged(object source, string title)
        {
            _title = title;
            RaisePropertyChanged("Title");
        }

        protected virtual void OnPositionChanged(object source, double x, double y)
        {
            SetPosition(x, y);
            UpdateAnchor();
        }

        protected virtual void OnSizeChanged(object source, double width, double height)
        {

            _width = width;
            _height = height;
            UpdateAnchor();
            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
        }

        protected virtual void OnScaleChanged(object source)
        {
            Transform.ScaleX = Model.ScaleX;
            Transform.ScaleY = Model.ScaleY;
            RaisePropertyChanged("Transform");
        }

        protected virtual void OnCanEditChange(object source, AtomModel.EditStatus status)
        {
            CanEdit = Model.CanEdit;
        }
        protected virtual void OnAlphaChanged(object source)
        {
            Alpha = Model.Alpha;
        }
        protected virtual void OnMetadataChange(object source, string key)
        {
            if (key == "tags")
                CreateTags();

        }

        private void CreateTags()
        {
            Tags.Clear();
            
            List<string> tagList = (List<string>)Model.GetMetaData("tags");

            foreach (string tag in tagList)
            {
                //sorry about this - should also be in frontend and not in viewmodel
                Button tagBlock = new Button();
                tagBlock.Background = new SolidColorBrush(Colors.DarkSalmon);
                tagBlock.Content = tag;
                tagBlock.Height = 30;
                tagBlock.Padding = new Thickness(5);
                tagBlock.BorderThickness = new Thickness(0);
                tagBlock.Foreground = new SolidColorBrush(Colors.White);
                tagBlock.Margin = new Thickness(2, 2, 2, 2);///
                tagBlock.Opacity = 0.75;
                tagBlock.FontStyle = FontStyle.Italic;
                tagBlock.IsHitTestVisible = false;

                Tags.Add(tagBlock);
            }
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
            _debouncingDictionary?.Add("x", Model.X);
            _debouncingDictionary?.Add("y", Model.Y);
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
            _debouncingDictionary?.Add("width", Width);
            _debouncingDictionary?.Add("height", Height);
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
                if (value < Constants.MinNodeSize) //prevent atom from getting too small
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
                if (value < Constants.MinNodeSize)
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
            set
            {
                Model.Title = value;
                _debouncingDictionary?.Add("title", value);
            }
        }

        //public string Tags { get; set; }

        public ObservableCollection<Button> Tags { get; set; }
        #endregion Public Properties
    }
}