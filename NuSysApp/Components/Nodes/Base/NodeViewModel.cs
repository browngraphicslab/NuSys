using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class NodeViewModel : AtomViewModel
    {
        private double _x, _y, _height, _width, _alpha;
        private bool _isEditing, _isEditingInk;

        public string Tags { get; set; }

        protected NodeViewModel(NodeModel model) : base(model)
        {
            model.PositionChanged += OnPositionChanged;
            model.SizeChanged += OnSizeChanged;
            model.ScaleChanged += OnScaleChanged;
            model.AlphaChanged += OnAlphaChanged;
            model.MetadataChange += OnMetadataChange;

            Transform.TranslateX = model.X;
            Transform.TranslateY = model.Y;
            Transform.ScaleX = model.ScaleX;
            Transform.ScaleY = model.ScaleY;
            Width = model.Width;
            Height = model.Height;
            Alpha = model.Alpha;
            X = model.X;
            Y = model.Y;

            Title = model.Title;
            Tags = string.Join(",", model.GetMetaData("tags") as List<string>);
        }

        private void OnAlphaChanged(object source)
        {
            Alpha = ((NodeModel) Model).Alpha;
        }

        private void OnMetadataChange(object source, string key)
        {
            Tags = string.Join(",", Model.GetMetaData("tags") as List<string>);
            RaisePropertyChanged("Tags");
        }

        public override void Dispose()
        {
            var model = (NodeModel) Model;
            model.PositionChanged -= OnPositionChanged;
            model.SizeChanged -= OnSizeChanged;
            model.ScaleChanged -= OnScaleChanged;
            model.AlphaChanged -= OnAlphaChanged;
            model.MetadataChange -= OnMetadataChange;
            base.Dispose();
        }

        #region Node Manipulations

        public override void Remove()
        {
            if (IsSelected)
            {
                //TODO: re-add
                SessionController.Instance.ActiveWorkspace.ClearSelection();
            }
        }

        public virtual void Translate(double dx, double dy)
        {
            if (IsEditing)
                return;

            var model = ((NodeModel) Model);
            model.X += dx/SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleX;
            model.Y += dy/SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleY;

            UpdateAnchor();

            foreach (var link in LinkList)
            {
                link.UpdateAnchor();
            }
        }

        public virtual void SetPosition(double x, double y)
        {
            Transform.TranslateX = x;
            Transform.TranslateY = y;
            foreach (var link in LinkList)
            {
                link.UpdateAnchor();
            }
            UpdateAnchor();
            RaisePropertyChanged("Transform");
        }

        public void ToggleEditing()
        {
            IsEditing = !IsEditing;
        }

        public void ToggleEditingInk()
        {
            IsEditingInk = !IsEditingInk;
        }

        public override void UpdateAnchor()
        {
            Anchor = new Point2d(Transform.TranslateX + Width / 2, Transform.TranslateY + Height / 2);
        }

        public virtual void Resize(double dx, double dy)
        {
            var changeX = dx/SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleX;
            var changeY = dy/SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleY;
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

        #endregion Node Manipulations

        #region Event Handlers

        private void OnPositionChanged(object source, PositionChangeEventArgs e)
        {
            SetPosition(((NodeModel) Model).X, ((NodeModel) Model).Y);
            UpdateAnchor();
        }

        private void OnSizeChanged(object source, WidthHeightUpdateEventArgs e)
        {
            Width = ((NodeModel) Model).Width;
            Height = ((NodeModel) Model).Height;
            UpdateAnchor();
        }

        private void OnScaleChanged(object source)
        {
            var model = (NodeModel) Model;
            Transform.ScaleX = model.ScaleX;
            Transform.ScaleY = model.ScaleY;
            RaisePropertyChanged("Transform");
        }

        #endregion Event Handlers

        #region Public Properties

        public string Title
        {
            get { return ((NodeModel)Model).Title; }
            set
            {
                ((NodeModel)Model).Title = value;
                RaisePropertyChanged("Title");
            }
        }

        public virtual double X
        {
            get { return _x; }
            set
            {
                _x = value;
                ((NodeModel)Model).X= value;
                RaisePropertyChanged("X");
            }
        }

        public virtual double Y
        {
            get { return _y; }
            set
            {
                _y = value;
                ((NodeModel)Model).Y = value;
                RaisePropertyChanged("Y");
            }
        }

        public virtual double Width
        {
            get { return _width; }
            set
            {
                if (_width == value || value < Constants.MinNodeSize) //prevent atom from getting too small
                    return;
                _width = value;
                ((NodeModel) Model).Width = value;
                RaisePropertyChanged("Width");
            }
        }
        
        public virtual double Height
        {
            get { return _height; }
            set
            {
                if (_height == value || value < Constants.MinNodeSize)
                    return;
                
                _height = value;
                ((NodeModel) Model).Height = value;
                RaisePropertyChanged("Height");
            }
        }

        public double Alpha
        {
            get { return _alpha; }
            set
            {
                _alpha = value;
                RaisePropertyChanged("Alpha");
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
            get { return ((NodeModel) Model).NodeType; }
        }

        #endregion Public Properties
    }
}