using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WinRTXamlToolkit.Tools;

namespace NuSysApp
{
    public class AudioPlayer : BaseMediaPlayer
    {
        public override void SetSize(double width, double height)
        {
            ProgressBar.SetHeight(height);
            Children.OfType<BackgroundImage>().ToList().ForEach(item => item.SetSize(width,height));
            base.SetSize(width, height);
        }

        public override void SetLibraryElement(AudioLibraryElementController controller, bool autoStartWhenLoaded = true)
        {
            if (controller != CurrentLibraryElementController)
            {
                Children.OfType<BackgroundImage>().ToList().ForEach(item => Children.Remove(item));
                Children.ForEach(child => Canvas.SetZIndex(child, 2));
                Children.Add(new BackgroundImage(controller, ProgressBar.Width, ProgressBar.Height));
            }
            base.SetLibraryElement(controller,autoStartWhenLoaded);
        }

        private class BackgroundImage : Canvas
        {
            private AudioLibraryElementController _controller;
            public BackgroundImage(AudioLibraryElementController controller, double width, double height)
            {
                Canvas.SetZIndex(this,1);
                Background = new SolidColorBrush(Colors.White);
                _controller = controller;
                var bitmap = new BitmapImage(_controller.LargeIconUri);
                var image = new Image() { Source = bitmap, Width = width / _controller.AudioLibraryElementModel.NormalizedDuration, Height = height };
                image.Stretch = Stretch.Fill;
                image.RenderTransform = new TranslateTransform();
                Children.Add(image);
                SetSize(width, height);
            }

            public void SetSize(double width, double height)
            {
                Width = width;
                Height = height;
                Clip = new RectangleGeometry()
                {
                    Rect = new Rect()
                    {
                        Width = width,
                        Height = height
                    }
                };
                var image = Children.OfType<Image>().FirstOrDefault();
                if (image != null)
                {
                    image.Width = width/_controller.AudioLibraryElementModel.NormalizedDuration;
                    image.Height = height;
                    var transform = image.RenderTransform as TranslateTransform;
                    transform.X = -image.Width * _controller.AudioLibraryElementModel.NormalizedStartTime;
                }
            }
        }
    }
}
