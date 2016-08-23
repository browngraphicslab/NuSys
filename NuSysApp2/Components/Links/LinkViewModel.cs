using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Controller;

namespace NuSysApp2
{
    public class LinkViewModel : BaseINPC
    {
        public ObservableCollection<LinkController> LinkList{ get; set; }

        public LinkModel LinkModel
        {
            get
            {
                Debug.Assert(Controller != null && Controller.Model != null);
                return Controller.Model;
            }
        }
        
        private string _title;
        private string _annotation;
        private readonly LinkController _controller;
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
    }
}