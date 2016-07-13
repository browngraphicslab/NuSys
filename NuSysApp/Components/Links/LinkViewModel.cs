using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Controller;

namespace NuSysApp
{
    public class LinkViewModel : BaseINPC, IEditable
    {
        public ObservableCollection<LinkController> LinkList{ get; set; }


        
        private string _title;
        private string _annotation;
        private readonly LinkController _controller;
        private bool _selected;
        private SolidColorBrush _color;

        private SolidColorBrush _selectedColor = new SolidColorBrush( ColorHelper.FromArgb(0xFF, 0x98, 0x1A, 0x4D));
        private SolidColorBrush _notSelectedColor = new SolidColorBrush(ColorHelper.FromArgb(0xFF,0x11,0x3D,0x40));

        public LinkModel LinkModel
        {
            get
            {
                Debug.Assert(Controller != null && Controller.Model != null);
                return Controller.Model;
            }
        }


        public LinkController Controller {
            get
            {
                Debug.Assert(_controller != null);
                return _controller;
            } 
        }
        public Point2d Anchor
        {
            get { return Controller.Anchor; }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        public LinkViewModel(LinkController controller)
        {
            _controller = controller;

            Debug.Assert(controller.LibraryElementController != null);

            controller.TitleChanged += TitleChanged;
            Title = controller.Title;
            IsSelected = false;
            controller.AnchorChanged += ChangeAnchor;

            RaisePropertyChanged("Anchor");
        }

        private void ChangeAnchor(object sender, Point2d e)
        {
            UpdateAnchor();
        }

        private void TitleChanged(object sender, string title)
        {
            Title = title;
        }


        public void Dispose()
        {
            Controller.TitleChanged -= TitleChanged;
        }
      

        public void UpdateAnchor()
        {
            RaisePropertyChanged("Anchor");
        }

        public void UpdateTitle(string title)
        {
            Controller.TitleChanged -= TitleChanged;
            Controller.LibraryElementController.SetTitle(title);
            Controller.TitleChanged += TitleChanged;
        }

        public bool ContainsSelectedLink { get; }

        public SolidColorBrush Color
        {
            get { return _color; }
            set
            {
                _color = value;
                RaisePropertyChanged("Color");
            }
        }

        /// <summary>
        /// From the ISelectable Interface, used to cull the screen, basically if something is null it is always visible
        ///  but calculating all the points for a link would require a bunch of math.
        /// </summary>
        public PointCollection ReferencePoints
        {

            get { return null; }
        }

        /// <summary>
        /// From the ISelectable Interface, used to implement selection in the free form viewer
        /// </summary>
        public bool IsSelected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                if (_selected == true)
                {
                    Color = _selectedColor;
                }
                else
                {
                    Color = _notSelectedColor;
                }
                RaisePropertyChanged("IsSelected");
            }
        }

        /// <summary>
        /// From the IEditable interface
        /// </summary>
        public bool IsEditing { get; set; }
    }
}