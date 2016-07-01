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
            Image.ImageOpened -= UpdateSizeFromModel;
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
            var url = Model.LibraryId + ".jpg";
            if (Controller.LibraryElementModel.ServerUrl != null)
            {
                url = Controller.LibraryElementModel.ServerUrl;
            }
            Image = new BitmapImage();
            Image.UriSource = new Uri("http://" + WaitingRoomView.ServerName + "/" + url);
            Image.ImageOpened += UpdateSizeFromModel;
            RaisePropertyChanged("Image");
        }
        private void UpdateSizeFromModel(object sender, object args)
        {
            var ratio = (double)Image.PixelHeight / (double)Image.PixelWidth;
            Controller.SetSize(Controller.Model.Width, Controller.Model.Width * ratio);
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
            // don't edit if we are in exploration or presentation mode
            if (SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION ||
                SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.PRESENTATION)
            {
                return;
            }

            SetSize(width,height);
        }
    }
}