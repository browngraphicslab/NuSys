using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioMediaPlayer : UserControl
    {
        public AudioMediaPlayer()
        {
            this.InitializeComponent();
            MediaElement.SetValue(Canvas.ZIndexProperty, 1);
        }

        private void Stop_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaElement.CurrentState != MediaElementState.Stopped)
            {
                MediaElement.Stop();
            }
        }

        private void Pause_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaElement.CurrentState != MediaElementState.Paused)
            {
                MediaElement.Pause();
            }
        }

        private void Play_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaElement.CurrentState != MediaElementState.Playing)
            {
                MediaElement.Play();
            }
        }

        private void ProgressBar_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
         //   MediaElement.Position = new TimeSpan(Convert.ToInt64(e.NewValue * MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds));
        }

        private void ProgressBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // TODO Fire event to reposition Regions
        }

        private void MediaElement_OnMediaOpened(object sender, RoutedEventArgs e)
        {
            // TODO Fire event to add regions
        }

        private void ProgressBar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            MediaElement.Position = new TimeSpan(Convert.ToInt64(e.GetPosition(ProgressBar).X * MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds));
        }

        private void ProgressBar_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            // Might be useful for resizing regions
        }

        private void ProgressBar_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint((UIElement) sender).Properties.IsLeftButtonPressed)
            {
                double jumpToRatio = e.GetCurrentPoint((UIElement) sender).Position.X/ProgressBar.ActualWidth;
                double milliseconds = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds*jumpToRatio;

                TimeSpan time = new TimeSpan(0, 0, 0, 0, (int) milliseconds);
                MediaElement.Position = time;
            }

        }

        private void MediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement.Position = TimeSpan.Zero;
        }

        public async Task RenderImageSource(Grid RenderedGrid)
        {

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            double x = Grid.Height;
            Grid.Width = RenderedGrid.Width * 2;
            try
            {
                await renderTargetBitmap.RenderAsync(RenderedGrid, 1000, 100);
            }
            catch (Exception e)
            {
                return;
            }
            Grid.Width = x;
            VisualizationImage.Source = renderTargetBitmap;
            Grid.Children.Remove(RenderedGrid);

        }

        public Uri AudioSource
        {
            get { return MediaElement.Source; }

            set { MediaElement.Source = value; }
        }

        public TimeSpan Position
        {
            get { return MediaElement.Position; }
        }

    }

}
