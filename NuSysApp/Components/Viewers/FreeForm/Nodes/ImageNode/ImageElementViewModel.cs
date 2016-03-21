﻿using System;
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
    public class ImageElementViewModel : ElementViewModel
    {
        
        public ImageElementViewModel(ElementController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            
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
                Controller.LibraryElementModel.OnLoaded += async delegate
                {
                    await DisplayImage();
                };
            }
            RaisePropertyChanged("Image");
        }

        private async Task DisplayImage()
        {
            var image = Controller.LibraryElementModel.ViewUtilBucket.ContainsKey("image")
                       ? (BitmapImage)Controller.LibraryElementModel.ViewUtilBucket["image"]
                       : null;
            if (image != null)
            {
                Image = image;
            }
            else
            {
                Image = await MediaUtil.ByteArrayToBitmapImage(Convert.FromBase64String(Controller.LibraryElementModel.Data));

                // adjust the size of an image that is too large
                if (Image.PixelHeight > 300 || Image.PixelWidth > 300)
                {
                    double dim = Math.Max(Image.PixelWidth, Image.PixelHeight);
                    double scale = Math.Floor(dim / 300);
                    SetSize(Image.PixelWidth / scale, Image.PixelHeight / scale);
                }
                else
                {
                    SetSize(Image.PixelWidth, Image.PixelHeight);
                }
                Controller.LibraryElementModel.ViewUtilBucket["image"] = Image;
                RaisePropertyChanged("Image");
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