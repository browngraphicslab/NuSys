using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Util;

namespace NuSysApp
{
    public class ImageElementViewModel : ElementViewModel
    {    
        public ImageElementViewModel(ElementController controller) : base(controller)
        {
            
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));       
        }

        public override void Dispose()
        {
            Controller.LibraryElementModel.OnLoaded -= LibraryElementModelOnOnLoaded;
        }

        public BitmapImage Image { get; set; }

        public override async Task Init()
        {
            if (Controller.LibraryElementModel.Loaded)
            {
                await DisplayImage();
            }
            else
            {
                Controller.LibraryElementModel.OnLoaded += LibraryElementModelOnOnLoaded;
            }
            RaisePropertyChanged("Image");
        }

        private void LibraryElementModelOnOnLoaded()
        {
            DisplayImage();
        }

        private async Task DisplayImage()
        {
            var image = Controller.LibraryElementModel.ViewUtilBucket.ContainsKey("image")
                       ? (BitmapImage)Controller.LibraryElementModel.ViewUtilBucket["image"]
                       : null;
            if (image != null)
            {
                Image = image;
                var ratio = (double)Image.PixelHeight / (double)Image.PixelWidth;
                Controller.SetSize(Controller.Model.Width, Controller.Model.Width * ratio);
            }
            else
            {
                Image = await MediaUtil.ByteArrayToBitmapImage(Convert.FromBase64String(Controller.LibraryElementModel.Data));
                Controller.LibraryElementModel.ViewUtilBucket["image"] = Image;
                var ratio = (double)Image.PixelHeight / (double)Image.PixelWidth;
                Controller.SetSize(Controller.Model.Width, Controller.Model.Width * ratio);
            }
            
            RaisePropertyChanged("Image");
        }
        /*
        public override void SetSize(double width, double height)
        {
            if (Image == null)
            {
                return;
            }

            var ratio = (double)Image.PixelHeight / (double)Image.PixelWidth;
            base.SetSize(width, width * ratio);
        }*/
        public override double GetRatio()
        {
            return Image == null ? 1 :(double)Image.PixelHeight / (double)Image.PixelWidth;
        }
        protected override void OnSizeChanged(object source, double width, double height)
        {
            SetSize(width,height);
        }
    }
}