using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class ElementInstanceViewModel : BaseINPC
    {
        #region Private Members      

        private double _x, _y, _height, _width, _alpha;
        private int _anchorX, _anchorY;
        private Point2d _anchor;
        private string _id;
        private SolidColorBrush _color;
        private bool _isEditing, _isEditingInk;
        private CompositeTransform _inkScale;
        private SolidColorBrush _userColor;
        private CompositeTransform _transform = new CompositeTransform();
        private ElementInstanceController _controller;
        protected bool _isSelected, _isMultiSelected, _isVisible;

        #endregion Private Members

        protected ElementInstanceViewModel(ElementInstanceController controller)
        {
            _controller = controller;
            LinkList = new ObservableCollection<LinkViewModel>();
            controller.CanEditChange += OnCanEditChange;
            controller.MetadataChange += OnMetadataChange;
            controller.PositionChanged += OnPositionChanged;
            controller.Translated += OnTranslated;
            controller.SizeChanged += OnSizeChanged;
            controller.Resized += OnResized;
            controller.ScaleChanged += OnScaleChanged;
            controller.AlphaChanged += OnAlphaChanged;
            controller.MetadataChange += OnMetadataChange;
            controller.TitleChanged += OnTitleChanged;
            controller.Deleted += OnDeleted;

            Tags = new ObservableCollection<Button>();

            controller.UserChanged +=
                delegate(NetworkUser user)
                {
                    _userColor = user != null ? new SolidColorBrush(user.Color) : new SolidColorBrush(Colors.Transparent);
                };

            ReadFromModel();
        }

        private void OnTranslated(object source, double tx, double ty)
        {
            Transform.TranslateX += tx;
            Transform.TranslateY += ty;
            UpdateAnchor();
            RaisePropertyChanged("Transform");
        }

        private void OnDeleted(object source)
        {
            // TODO: Dispose everything in here.
        }


        public virtual async Task Init()
        {
        }

        private void OnTitleChanged(object source, string title)
        {
            Title = title;
            RaisePropertyChanged("Title");
        }

        protected virtual void OnPositionChanged(object source, double x, double y)
        {
            Transform.TranslateX = x;
            Transform.TranslateY = y;
            UpdateAnchor();
            RaisePropertyChanged("Transform");
        }

        protected virtual void OnSizeChanged(object source, double width, double height)
        {
            _width = width;
            _height = height;
            UpdateAnchor();
            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
        }

        protected virtual void OnResized(object source, double deltaWidth, double deltaHeight)
        {
            if (Width > Constants.MinNodeSizeX || deltaWidth > 0)
            {
                _width = Width + deltaWidth;
            }
            if (Height > Constants.MinNodeSizeY || deltaHeight > 0)
            {
                _height = Height + deltaHeight;
            }

            UpdateAnchor();
            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
        }

        protected virtual void OnScaleChanged(object source, double sx, double sy)
        {
            Transform.ScaleX = sx;
            Transform.ScaleY = sy;
            RaisePropertyChanged("Transform");
        }

        protected virtual void OnCanEditChange(object source, EditStatus status)
        {
            CanEdit = status;
        }

        protected virtual void OnAlphaChanged(object source, double alpha)
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

            //TODO: refactor

            /*

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
            */
            RaisePropertyChanged("Tags");
        }

        #region Atom Manipulations

        public virtual void ReadFromModel()
        {
            var model = Controller.Model;
            Transform.TranslateX = model.X;
            Transform.TranslateY = model.Y;
            Transform.ScaleX = model.ScaleX;
            Transform.ScaleY = model.ScaleY;

            Id = model.Id;
            Width = model.Width;
            Height = model.Height;
            Alpha = model.Alpha;
            Title = model.Title;
            IsVisible = true;
            Transform.TranslateX = model.X;
            Transform.TranslateY = model.Y;

            CreateTags();
        }

        public virtual void WriteToModel()
        {
            var model = Controller.Model;
            model.Id = Id;
            model.X = Transform.TranslateX;
            model.Y = Transform.TranslateY;
            model.ScaleX = Transform.ScaleX;
            model.ScaleY = Transform.ScaleY;
            model.Width = Width;
            model.Height = Height;
            model.Alpha = Alpha;
            model.Title = Title;
            model.X = Transform.TranslateX;
            model.Y = Transform.TranslateY;
        }

        public virtual void Dispose()
        {
            _controller.CanEditChange -= OnCanEditChange;
            _controller.MetadataChange -= OnMetadataChange;
            _controller.PositionChanged -= OnPositionChanged;
            _controller.Translated -= OnTranslated;
            _controller.SizeChanged -= OnSizeChanged;
            _controller.ScaleChanged -= OnScaleChanged;
            _controller.AlphaChanged -= OnAlphaChanged;
            _controller.MetadataChange -= OnMetadataChange;

            Tags = null;
            _transform = null;
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
                SessionController.Instance.ActiveFreeFormViewer.SetSelection(this);
        }

        public void AddLink(LinkViewModel linkVm)
        {
            LinkList.Add(linkVm);
            UpdateAnchor();
        }

        public virtual void Remove()
        {
        }

        #endregion

        #region Other Methods

        public virtual void UpdateAnchor()
        {
            Anchor = new Point2d(Transform.TranslateX + Width/2, Transform.TranslateY + Height/2);
            foreach (var link in LinkList)
            {
                link.UpdateAnchor();
            }
        }

        #endregion Other Methods

        #region Public Properties

        public ObservableCollection<LinkViewModel> LinkList { get; set; }

        private EditStatus _canEdit;

        public EditStatus CanEdit
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
            get { return _color; }
            set
            {
                _color = value;
                RaisePropertyChanged("Color");
            }
        }

        public string Id
        {
            get { return _id; }
            set { _id = value; }
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

        public ElementInstanceModel Model
        {
            get { return _controller.Model; }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                if (value < Constants.MinNodeSize) //prevent atom from getting too small
                    return;
                _width = value;
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

        public string Title { get; set; }

        //public string Tags { get; set; }

        public ObservableCollection<Button> Tags { get; set; }

        public ElementInstanceController Controller
        {
            get { return _controller; }
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

        public ElementType ElementType
        {
            get { return ((ElementInstanceModel) Model).ElementType; }
        }

        public string ContentId
        {
            get { return ((ElementInstanceModel) Model).ContentId; }
        }

        public CompositeTransform InkScale
        {
            get { return _inkScale; }
            set
            {
                _inkScale = value;
                RaisePropertyChanged("InkScale");
            }
        }

        public SolidColorBrush UserColor
        {
            get { return _userColor; }
            set
            {
                if (_userColor == value)
                {
                    return;
                }

                RaisePropertyChanged("UserColor");
            }
        }

        #endregion Public Properties
    }
}