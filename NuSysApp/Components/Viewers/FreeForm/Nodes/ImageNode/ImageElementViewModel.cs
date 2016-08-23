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
    public class ImageElementViewModel : ElementViewModel
    {
        public ImageElementViewModel(ElementController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));

        }

        public override void Dispose()
        {
            if (Image != null)
            {
                Image.ImageOpened -= UpdateSizeFromModel;
            }
            base.Dispose();
        }

        public BitmapImage Image { get; set; }

        public override async Task Init()
        {
            if (!Controller.LibraryElementController.ContentLoaded)
            {
                await Controller.LibraryElementController.LoadContentDataModelAsync();
            }
            await DisplayImage();

            RaisePropertyChanged("Image");


        }

        private async Task DisplayImage()
        {
            var url = new Uri(Controller.LibraryElementController.Data);
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

        public override double GetRatio()
        {
            if (Image == null)
            {
                return 1;
            }
            if (Controller.LibraryElementModel is RectangleRegion)
            {
                var rect = Controller.LibraryElementModel as RectangleRegion;
                return ((double)Image.PixelHeight* rect.Height)/((double)Image.PixelWidth*rect.Width);
            }
            return (double)Image.PixelHeight / (double)Image.PixelWidth;
            
        }
        protected override void OnSizeChanged(object source, double width, double height)
        {
            if (width * GetRatio() < Constants.MinNodeSize)
            {
                return; // If the height becomes smaller than the minimum node size then we don't apply the size changed, applying the height's change causes weird behaviour
            }
            
            SetSize(width, width * GetRatio());
            
        }
    }
}