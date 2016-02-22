using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class WebNodeViewModel : AtomViewModel
    {
        private double  _zoom = 1;
       
        public string Url { get; set; }
        public List<WebNodeModel.Webpage> History { get { return (Model as WebNodeModel).History; } } 
        public Rect ClipRect { get; set; }

        public double Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                RaisePropertyChanged("Zoom");
            }
        }

        public WebNodeViewModel(ElementInstanceController controller) : base(controller)
        {
            ClipRect = new Rect(0,0, controller.Model.Width, controller.Model.Height);
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));

            // TODO: refactor
            /*
            Url = controller.Model.Url;
            Controller.SizeChanged += delegate(object source, double width, double height)
            {
                Zoom = (Width / 1024.0);
                ClipRect = new Rect(0, 0, width, height);
                RaisePropertyChanged("Zoom");
                RaisePropertyChanged("ClipRect");
            };
            controller.Model.UrlChanged += delegate(object source, string url)
            {
                Url = url;
                RaisePropertyChanged("Url");
            };
            Zoom = (Width / 1024.0);
            */
        }
    }
}