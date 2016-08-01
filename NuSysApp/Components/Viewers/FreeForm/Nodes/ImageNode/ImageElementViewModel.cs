using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media;
using NuSysApp.Util;
using System.Diagnostics;
using NusysIntermediate;

namespace NuSysApp
{
    public class ImageElementViewModel : ElementViewModel, Sizeable
    {
   
        public Sizeable View { get; set; }

        public LibraryElementController LibraryElementController{get { return Controller.LibraryElementController; }}
        public ImageElementViewModel(ElementController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));       
        }

        public void SizeChanged(object sender, double width, double height)
        {

        }
        public override void Dispose()
        {
            if (Controller != null)
            {
                Controller.LibraryElementController.Loaded -= LibraryElementModelOnOnLoaded;
            }
            if (Image != null)
            {
                Image.ImageOpened -= UpdateSizeFromModel;
            }
        }

        public BitmapImage Image { get; set; }


        public override async Task Init()
        {
            if (Controller.LibraryElementController.IsLoaded)
            {
                await DisplayImage();
            }
            else
            {
                Controller.LibraryElementController.Loaded += LibraryElementModelOnOnLoaded;
            }
            RaisePropertyChanged("Image");


        }

        private async void LibraryElementModelOnOnLoaded(object sender)
        {
            await DisplayImage();
        }

        private async Task DisplayImage()
        {
            var url = Controller.LibraryElementController.GetSource();
            Image = new BitmapImage();
            Image.UriSource = url;
            Image.ImageOpened += UpdateSizeFromModel;
            RaisePropertyChanged("Image");

        }
        private void UpdateSizeFromModel(object sender, object args)
        {
            var ratio = (double)Image.PixelHeight / (double)Image.PixelWidth;
            Controller.SetSize(Controller.Model.Width, Controller.Model.Width * ratio);
        }
        
        //public override void SetSize(double width, double height)
        //{
        //    if (Image == null)
        //    {
        //        return;
        //    }

        //    var ratio = (double)Image.PixelHeight / (double)Image.PixelWidth;
        //    base.SetSize(width, width * ratio);
        //}
        public override double GetRatio()
        {
            if (Image == null)
            {
                return 1;
            }
            if (LibraryElementController.LibraryElementModel is RectangleRegion)
            {
                var rect = LibraryElementController.LibraryElementModel as RectangleRegion;
                return ((double)Image.PixelHeight* rect.Height)/((double)Image.PixelWidth*rect.Width);
            }
            return (double)Image.PixelHeight / (double)Image.PixelWidth;
            
        }
        protected override void OnSizeChanged(object source, double width, double height)
        {
            // don't edit if we are in exploration or presentation mode
            if (SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION ||
                SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.PRESENTATION)
            {
                return;
            }

            SetSize(width,width * GetRatio());
        }

        public double GetWidth()
        {
            return Width;

        }

        public double GetHeight()
        {
            return Height;

        }
    }
}