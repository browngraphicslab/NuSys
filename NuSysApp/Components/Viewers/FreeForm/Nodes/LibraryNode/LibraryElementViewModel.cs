using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class LibraryElementViewModel : ElementViewModel
    {
        
        public LibraryElementViewModel(ElementController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));

            controller.ContentLoaded += async delegate(object source, NodeContentModel content)
            {
                Image = await MediaUtil.ByteArrayToBitmapImage(Convert.FromBase64String(content.Data));
                SetSize(Image.PixelWidth, Image.PixelHeight);
                RaisePropertyChanged("Image");
            };
        }


        public BitmapImage Image { get; set; }

        public override async Task Init()
        {
            RaisePropertyChanged("Image");
        }

        public override void SetSize(double width, double height)
        {
            if (Image.PixelWidth > Image.PixelHeight)
            {
                var r = Image.PixelHeight / (double)Image.PixelWidth;
                base.SetSize(width, width * r);
            }
            else
            {
                var r = Image.PixelWidth / (double)Image.PixelHeight;
                base.SetSize(height * r, height);
            }
        }
    }
}