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
    public class ImageNodeViewModel : NodeViewModel
    {
        

        public ImageNodeViewModel(ImageNodeModel model) : base(model)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));

        }

        public void Init()
        {
            SetSize(Width, Height);
            InkScale = new CompositeTransform();
        }

        public override void SetSize(double width, double height)
        {

            var Image = ((ImageNodeModel) Model).Image;
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