using System;
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
    public class WebNodeViewModel : NodeViewModel
    {
        private double  _zoom = 1;
       
        public string Url { get; set; }
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

        public WebNodeViewModel(WebNodeModel model) : base(model)
        {
            ClipRect = new Rect(0,0, model.Width, model.Height);
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            Url = model.Url;
            model.SizeChanged += delegate(object source, WidthHeightUpdateEventArgs args)
            {
                Zoom = (Width / 1024.0);
                ClipRect = new Rect(0, 0, model.Width, model.Height);
                RaisePropertyChanged("Zoom");
                RaisePropertyChanged("ClipRect");
            };
            model.UrlChanged += delegate(object source, string url)
            {
                Url = url;
                RaisePropertyChanged("Url");
            };
            Zoom = (Width / 1024.0);
        }
    }
}