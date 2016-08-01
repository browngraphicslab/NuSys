using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
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

        public TimeSpan ScrubBarPosition { set; get; }

        public TimelineMarker StartMarker { set; get; }
        public TimelineMarker EndMarker { set; get; }
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
            }
        }


        private void MediaElement_OnMediaOpened(object sender, RoutedEventArgs e)
        {

            if (DataContext is AudioDetailHomeTabViewModel)
            {
                var vm = DataContext as AudioDetailHomeTabViewModel;
                xAudioWrapper.Controller = vm.LibraryElementController;
            }
            else if (DataContext is AudioNodeViewModel)
            {
                var vm = DataContext as AudioNodeViewModel;
                xAudioWrapper.Controller = vm.Controller.LibraryElementController;
            }
            else
            {
                Debug.Fail("We should always be in a node or the detail view, if not we must add functionality here");
            }
            xAudioWrapper.ProcessLibraryElementController();

            //After updating audiowrapper, set position dyanmically:
            double normalizedMediaElementPosition = xAudioWrapper.AudioStart;
            double totalDuration = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            double denormalizedMediaElementPosition = normalizedMediaElementPosition * totalDuration;

            TimeSpan startTime = new TimeSpan(0, 0, 0, 0, (int)denormalizedMediaElementPosition);
            TimeSpan endTime = new TimeSpan(0, 0, 0, 0, (int)(totalDuration * xAudioWrapper.AudioEnd));

            MediaElement.Position = startTime;

            MediaElement.Markers.Clear();

            StartMarker = new TimelineMarker();
            StartMarker.Time = startTime;
            EndMarker = new TimelineMarker();
            EndMarker.Time = endTime;

            MediaElement.Markers.Add(StartMarker);
            MediaElement.Markers.Add(EndMarker);

            ScrubBar.Minimum = totalDuration * xAudioWrapper.AudioStart;
            ScrubBar.Maximum = totalDuration * xAudioWrapper.AudioEnd;


            // set the right time stamp
            var converter = new PositionToStringConverter();
            var timeSpan = new TimeSpan(0, 0, 0, 0, (int)(totalDuration * xAudioWrapper.AudioEnd));
            xRightTimeStampTextBlock.Text = (string)converter.Convert(timeSpan, null, null, null); // this looks weird cause its a xaml converter 


        }

        private void ProgressBar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            double position = e.GetPosition((UIElement)sender).X / ProgressBar.ActualWidth;
            double normalizedMediaElementPosition = xAudioWrapper.AudioStart + position * (xAudioWrapper.AudioEnd - xAudioWrapper.AudioStart);
            double totalDuration = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            double denormalizedMediaElementPosition = normalizedMediaElementPosition * totalDuration;
            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)denormalizedMediaElementPosition);
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
            if (e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed)
            {
                double position = e.GetCurrentPoint((UIElement)sender).Position.X / ProgressBar.ActualWidth;
                double normalizedMediaElementPosition = xAudioWrapper.AudioStart + position * (xAudioWrapper.AudioEnd - xAudioWrapper.AudioStart);
                double totalDuration = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
                double denormalizedMediaElementPosition = normalizedMediaElementPosition * totalDuration;
                TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)denormalizedMediaElementPosition);
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

        private void MediaElement_MarkerReached(object sender, TimelineMarkerRoutedEventArgs e)
        {
            if (e.Marker.Time == StartMarker.Time)
            {
                //MediaElement.Stop();
            }
            else if (e.Marker.Time == EndMarker.Time)
            {
                //MediaElement.Stop();
                MediaElement.Pause();
                Audio_OnJump(StartMarker.Time);

            }
        }
    }

}
