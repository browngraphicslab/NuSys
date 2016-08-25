using System;
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
using Windows.UI.Xaml.Shapes;
using NusysIntermediate;
using NuSysApp.Controller;
using NuSysApp.Util;

namespace NuSysApp
{
    public class ElementViewModel : BaseINPC, IEditable
    {
        #region Private Members      

        protected double _x, _y, _height, _width, _alpha;
        private string _id;
        private SolidColorBrush _color;
        private bool _isEditing, _isEditingInk;
        private CompositeTransform _inkScale;
        private CompositeTransform _transform = new CompositeTransform();
        private ElementController _controller;
        protected bool _isSelected, _isVisible, _isPresenting;
        private bool Favorited;

        #endregion Private Members
       
        public ElementViewModel(ElementController controller)
        {
            _controller = controller;
            LinkList = new ObservableCollection<LinkController>();
            controller.MetadataChange += OnMetadataChange;
            controller.PositionChanged += OnPositionChanged;
            controller.SizeChanged += OnSizeChanged;
            controller.ScaleChanged += OnScaleChanged;
            controller.AlphaChanged += OnAlphaChanged;
            controller.MetadataChange += OnMetadataChange;
            controller.AnchorChanged += ControllerOnAnchorChanged;
            if (controller.LibraryElementController != null)
            {
                controller.LibraryElementController.TitleChanged += OnTitleChanged;
                controller.LibraryElementController.KeywordsChanged += KeywordsChanged;
            }
            controller.Disposed += OnDisposed;
            controller.Deleted += ControllerOnDeleted;
            controller.LinksUpdated += ControllerLinksUpdated;

            Tags = new ObservableCollection<Button>();
            CircleLinks = new ObservableCollection<LinkCircle>();
            ReadFromModel();
        }

        private void ControllerLinksUpdated(object source)
        {
            UITask.Run(async delegate { CreateCircleLinks(); });
        }

        private void ControllerOnAnchorChanged(object sender, Point2d point2D)
        {
            UpdateAnchor();
        }

        private void KeywordsChanged(object sender, HashSet<Keyword> keywords)
        {
            CreateTags();
        }

        public void UpdateLinks()
        {
            UITask.Run(async delegate { CreateCircleLinks(); });    
        }
        private void ControllerOnDeleted(object source)
        {

        }

        private void OnDisposed(object source, object args)
        {
            Dispose();
        }

        public virtual async Task Init()
        {
        }

        private void OnTitleChanged(object source, string title)
        {
            Title = title;
            RaisePropertyChanged("Title");
        }

        protected virtual void OnPositionChanged(object source, double x, double y, double dx, double dy)
        {
            Transform.TranslateX = x;
            Transform.TranslateY = y;
            RaisePropertyChanged("Transform");
        }

        protected virtual void OnSizeChanged(object source, double width, double height)
        {
            _width = width;
            _height = height;
            
            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
        }

        protected virtual void OnScaleChanged(object source, double sx, double sy)
        {
            Transform.ScaleX = sx;
            Transform.ScaleY = sy;
            RaisePropertyChanged("Transform");
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

        private void CreateCircleLinks()
        {
            var libraryElementId = this.Controller.LibraryElementModel?.LibraryElementId;
            if (libraryElementId == null)
            {
                return;
            }

            CircleLinks.Clear();
            // all the link library element ids for links connected to this element view model
            var linkLibraryElementIds = SessionController.Instance.LinksController.GetLinkedIds(libraryElementId);
            if(linkLibraryElementIds == null)
            {
                return;
            }
            foreach (var linkLibraryElementId in linkLibraryElementIds)
            {
                //sorry about this - should also be in frontend and not in viewmodel
                var linkLibraryElementModel = SessionController.Instance.ContentController.GetLibraryElementModel(linkLibraryElementId) as LinkLibraryElementModel;
                Debug.Assert(linkLibraryElementModel != null);
                if (linkLibraryElementModel != null)
                {
                    string contentId = "";
                    if (this.LibraryElementId == linkLibraryElementModel.InAtomId)
                    {
                        contentId = linkLibraryElementModel.OutAtomId;
                    }
                    else if (this.LibraryElementId == linkLibraryElementModel.OutAtomId)
                    {
                        contentId = linkLibraryElementModel.InAtomId;
                    }
                    var circlelink = new LinkCircle(linkLibraryElementId, contentId);

                    circlelink.Disposed += Circlelink_Disposed;

                    CircleLinks.Add(circlelink);
                }
                
            }
        }

        private void Circlelink_Disposed(object sender, EventArgs e)
        {
            Debug.Assert(sender is LinkCircle);
            var circleLink = sender as LinkCircle;
            circleLink.Disposed -= Circlelink_Disposed;
            CircleLinks.Remove(sender as LinkCircle);
        }

        private void CreateTags()
        {
            Tags.Clear();

            var tagList = Controller?.LibraryElementController?.LibraryElementModel?.Keywords;
            if(tagList == null)
            {
                return;
            }
            foreach (var tag in tagList)
            {
                //sorry about this - should also be in frontend and not in viewmodel
                Button tagBlock = new Button();
                tagBlock.Background = new SolidColorBrush(Constants.color4);
                tagBlock.Content = tag.Text;
                tagBlock.Height = 30;
                tagBlock.Padding = new Thickness(5);
                tagBlock.BorderThickness = new Thickness(0);
                tagBlock.Foreground = new SolidColorBrush(Constants.foreground6);
                tagBlock.Margin = new Thickness(2, 2, 2, 2);///
                tagBlock.FontStyle = FontStyle.Italic;
                tagBlock.IsHitTestVisible = true;     
                tagBlock.Tapped += OnTagBlockTapped;
                Tags.Add(tagBlock);
            }
            
            RaisePropertyChanged("Tags");
        }

    
        /// <summary>
        /// When a tag block is tapped, it should signal the session view to show a box
        /// with related elements (elements with the same tag)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTagBlockTapped(object sender, TappedRoutedEventArgs e)
        {
            var button = sender as Button;
            var text = button.Content.ToString();
            SessionController.Instance.SessionView.ShowRelatedElements(text);
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
            CreateCircleLinks();
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
            _controller.MetadataChange -= OnMetadataChange;
            _controller.PositionChanged -= OnPositionChanged;
            _controller.SizeChanged -= OnSizeChanged;
            _controller.ScaleChanged -= OnScaleChanged;
            _controller.AlphaChanged -= OnAlphaChanged;
            _controller.AnchorChanged -= ControllerOnAnchorChanged;
            _controller.MetadataChange -= OnMetadataChange;
            if (_controller.LibraryElementController != null)
            {
                _controller.LibraryElementController.TitleChanged -= OnTitleChanged;
                _controller.LibraryElementController.KeywordsChanged -= KeywordsChanged;
            }
            _controller.Disposed -= OnDisposed;
            _controller.Deleted -= ControllerOnDeleted;
            _controller.LinksUpdated -= ControllerLinksUpdated;

            Tags = null;
            _transform = null;
        }

        public virtual void SetSize(double width, double height)
        {
            Width = width;
            Height = height;
            Controller.Model.Height = height;
            Controller.Model.Width = width;
        }

        #endregion
        public virtual void UpdateAnchor()
        {
            RaisePropertyChanged("Anchor");
        }

        public virtual double GetRatio()
        {
            return 1.01010101;
        }
 

        #region Public Properties

        public ObservableCollection<LinkController> LinkList { get; set; }
        
        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                _controller.Selected(value);
                RaisePropertyChanged("IsSelected");
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
            get { return Controller.Anchor; }
        }

        public ElementModel Model
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
        public ObservableCollection<Button> Tags { get; set; }
        public ObservableCollection<LinkCircle> CircleLinks { get; set; }

        public ElementController Controller
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

        public NusysConstants.ElementType ElementType
        {
            get { return ((ElementModel) Model).ElementType; }
        }

        public string LibraryElementId
        {
            get { return ((ElementModel) Model).LibraryId; }
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

        /// <summary>
        /// returns the reference points to be used in multiselect. the four corners of the node are used. 
        /// </summary>
        public virtual PointCollection ReferencePoints
        {
            get
            {
                // must use the transform's translate x/y values in case the user has moved the node
                double x = Transform.TranslateX;
                double y = Transform.TranslateY;

                // add each corner point to the list of reference points
                PointCollection pts = new PointCollection();
                pts.Add(new Point(x, y));           // top left
                pts.Add(new Point(x + Width, y));   // top right
                pts.Add(new Point(x, y + Height));  // bottom left
                pts.Add(new Point(x + Width/2, y + Height/2)); // bottom right
                pts.Add(new Point(x + Width/4, y + Height/4)); // bottom right
                pts.Add(new Point(x + Width/4*3, y + Height/4)); // bottom right
                pts.Add(new Point(x + Width / 4, y + Height / 4*3)); // bottom right
                pts.Add(new Point(x + Width / 4 * 3, y + Height / 4*3)); // bottom right
                pts.Add(new Point(x + Width, y + Height)); // bottom right
                pts.Add(new Point(x + Width, y + Height)); // bottom right
                return pts;
            }
        }

        public virtual Rect GetBoundingRect()
        {
            double x = Transform.TranslateX;
            double y = Transform.TranslateY;
            return new Rect(x, y, Width, Height);
        }

        /// <summary>
        /// Will return if the atom has a link associated with it. This prevents "double-dipping" 
        /// manipulation events. For example, recall that all selected atoms will move if one selected atoms 
        /// is moved. If the moved atom is a link, then the two atoms the link is "linking" will be translated from
        /// (1) the translate method being called in the NodeManipulationMode since it is a selected atom and (2) the 
        /// translate method being called by the link. This method prevents this "double-dipping" from occuring.
        /// </summary>
        public bool ContainsSelectedLink
        {
            get
            {
                if (LinkList.Count > 0)
                {
                    // return true if at least one link is selected
                    foreach (var link in LinkList)
                    {
                        // TODO: refactor
                        //if (link.Model.IsSelected)
                        //    return true;
                    }
                }
                return false;
            }
        }

        #endregion Public Properties
    }

}