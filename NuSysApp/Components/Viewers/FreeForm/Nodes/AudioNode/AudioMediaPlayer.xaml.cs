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
        //needed for "dragging" through scrub bar
        private bool _wasPlaying = false;
        public ProgressBar ScrubBar => this.ProgressBar;
        public MediaElement MediaPlayer => this.MediaElement;

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
                _wasPlaying = false;

            }
        }

        private void Pause_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaElement.CurrentState != MediaElementState.Paused)
            {
                MediaElement.Pause();
                _wasPlaying = false;

            }
        }

        private void Play_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaElement.CurrentState != MediaElementState.Playing)
            {
                Binding b = new Binding();
                b.ElementName = "MediaElement";
                b.Path = new PropertyPath("Position.TotalMilliseconds");
                ProgressBar.SetBinding(ProgressBar.ValueProperty, b);
                MediaElement.Play();
                _wasPlaying = true;

            }
        }


        private void MediaElement_OnMediaOpened(object sender, RoutedEventArgs e)
        {
            //var vm = this.DataContext as AudioNodeViewModel;
            //if (vm == null)
            //{
            //    return;
            //}


            //double width = this.ActualWidth;
            //double height = this.ActualHeight;
            //vm.LibraryElementController.SetSize(width, height);

            MediaElement.Position = new TimeSpan(0);


        }

        private void ProgressBar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //MediaElement.Position = new TimeSpan(Convert.ToInt64(e.GetPosition(ProgressBar).X * MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds));

            double ratio = e.GetPosition((UIElement)sender).X / ProgressBar.ActualWidth;
            double millliseconds = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds * ratio;

            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)millliseconds);
            MediaElement.Position = time;

            if (MediaElement.CurrentState != MediaElementState.Playing)
            {
                Binding b = new Binding();
                b.ElementName = "MediaElement";
                b.Path = new PropertyPath("Position.TotalMilliseconds");
                ProgressBar.SetBinding(ProgressBar.ValueProperty, b);
                MediaElement.Pause();
            }
        }

        private void ProgressBar_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_wasPlaying)
            {
                MediaElement.Play();

            }
            else
            {
                MediaElement.Pause();

            }
            ((UIElement)sender).ReleasePointerCapture(e.Pointer);
        }
        public void onSeekedTo(double time)
        {
            double millliseconds = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds * time;

            TimeSpan timespan = new TimeSpan(0, 0, 0, 0, (int)millliseconds);
            MediaElement.Position = timespan;
            Binding b = new Binding();
            b.ElementName = "MediaElement";
            b.Path = new PropertyPath("Position.TotalMilliseconds");
            ProgressBar.SetBinding(ProgressBar.ValueProperty, b);
        }
        private void ProgressBar_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            /*
            if (e.GetCurrentPoint((UIElement) sender).Properties.IsLeftButtonPressed)
            {
                double jumpToRatio = e.GetCurrentPoint((UIElement) sender).Position.X/ProgressBar.ActualWidth;
                double milliseconds = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds*jumpToRatio;

                TimeSpan time = new TimeSpan(0, 0, 0, 0, (int) milliseconds);
                MediaElement.Position = time;
            }*/

            if (e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed)
            {
                double ratio = e.GetCurrentPoint((UIElement)sender).Position.X / ProgressBar.ActualWidth;
                double seconds = MediaElement.NaturalDuration.TimeSpan.TotalSeconds * ratio;

                TimeSpan time = new TimeSpan(0, 0, (int)seconds);
                MediaElement.Position = time;
                if (MediaElement.CurrentState != MediaElementState.Playing)
                {
                    Binding b = new Binding();
                    b.ElementName = "MediaElement";
                    b.Path = new PropertyPath("Position.TotalMilliseconds");
                    ProgressBar.SetBinding(ProgressBar.ValueProperty, b);

                }
                else
                {
                    ((UIElement)sender).CapturePointer(e.Pointer);
                    //MediaElement.Pause();
                }

                e.Handled = true;
            }

        }

        private void MediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            Audio_OnJump(new TimeSpan(0));
            _wasPlaying = false;

        }

        public void Audio_OnJump(TimeSpan time)
        {
            MediaElement.Position = time;
            if (MediaElement.CurrentState != MediaElementState.Playing)
            {
                Binding b = new Binding();
                b.ElementName = "MediaElement";
                b.Path = new PropertyPath("Position.TotalMilliseconds");
                ProgressBar.SetBinding(ProgressBar.ValueProperty, b);

            }
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
        public void StopMusic()
        {
            MediaElement.Stop();
        }

    }

}
