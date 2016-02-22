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
    public class ImageNodeViewModel : ElementInstanceViewModel
    {
        
        public ImageNodeViewModel(ElementInstanceController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            InkScale = new CompositeTransform();
        }


        public BitmapImage Image { get;
            set; }

        public override async Task Init()
        {
            byte[] data = null;
            var content = SessionController.Instance.ContentController.Get(((ElementInstanceModel) Model).ContentId);

            if (content != null) { 
                data = Convert.FromBase64String(content.Data);
                Image = await MediaUtil.ByteArrayToBitmapImage(data);
                SetSize(Width, Height);
            }
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