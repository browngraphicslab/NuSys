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
        private CompositeTransform _inkScale;

        public ImageNodeViewModel(ImageNodeModel model) : base(model)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            InkScale = new CompositeTransform { ScaleX = 1, ScaleY = 1 };
        }

        public void Init()
        {
            var Image = ((ImageNodeModel) Model).Image;
            if (Image.PixelWidth > Image.PixelHeight)
            {
                var r = Image.PixelHeight / (double)Image.PixelWidth;
                SetSize(Width, base.Width*r);
            }
            else
            {
                var r = Image.PixelWidth / (double)Image.PixelHeight;
                SetSize(base.Height*r, Width);
            }
        }

        public override void Resize(double dx, double dy)
        {
            double newDx, newDy;
            if (dx > dy)
            {
                newDx = dy * ((ImageNodeModel)Model).Image.PixelWidth / ((ImageNodeModel)Model).Image.PixelHeight;
                newDy = dy;
            }
            else
            {
                newDx = dx;
                newDy = dx * ((ImageNodeModel)Model).Image.PixelHeight / ((ImageNodeModel)Model).Image.PixelWidth;
            }
            if ((newDx / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleX + Width <= Constants.MinNodeSizeX && dx < 1) || (newDy / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleY + Height <= Constants.MinNodeSizeY && dy < 1))
            {
                return;
            }
            CompositeTransform ct = InkScale;
            ct.ScaleX *= (Width + newDx / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleX) / Width;
            ct.ScaleY *= (Height + newDy / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleY) / Height;
            InkScale = ct;

            base.Resize(newDx, newDy);
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
    }
}