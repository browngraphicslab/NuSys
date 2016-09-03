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
        public ProgressBar ScrubBar => this.ProgressBar;
        public MediaElement MediaPlayer => this.MediaElement;
        public AudioWrapper AudioWrapper => xAudioWrapper;
        public TimeSpan ScrubBarPosition { set; get; }

        public TimelineMarker StartMarker { set; get; }
        public TimelineMarker EndMarker { set; get; }
        public Binding positionBinding { get; set; }
        public AudioMediaPlayer()
        {
            this.InitializeComponent();
            MediaElement.SetValue(Canvas.ZIndexProperty, 1);
            //When regions are updated (added/removed/timechanged), run method:
            xAudioWrapper.OnRegionsUpdated += XAudioWrapper_OnRegionsUpdated;
            xAudioWrapper.OnRegionSeeked += onSeekedTo;
            xAudioWrapper.OnIntervalChanged += XAudioWrapper_OnIntervalChanged;
            positionBinding = new Binding();
            DataContextChanged += AudioMediaPlayer_DataContextChanged;

        }


        /// <summary>
        /// Called when the DataContext changes, sets the Visualization Image on the media player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AudioMediaPlayer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext is AudioNodeViewModel)
            {
                VisualizationImage.Source = new BitmapImage((DataContext as AudioNodeViewModel)?.Controller.LibraryElementController.LargeIconUri);
            }
            else if (DataContext is AudioDetailHomeTabViewModel)
            {
                VisualizationImage.Source =
                    new BitmapImage(
                        (DataContext as AudioDetailHomeTabViewModel)?.LibraryElementController.LargeIconUri);
            }
            else if (DataContext == null)
            {
                // do nothing this is generic for when the DataContext isn't set
            }
            else
            {
                Debug.Fail($"Add Support for Visualization of audio datacontext {DataContext} here");
            }
        }

        private void XAudioWrapper_OnIntervalChanged(object sender, double start, double end)
        {

            //After updating audiowrapper, set position dyanmically:
            double normalizedMediaElementPosition = xAudioWrapper.AudioStart;
            double totalDuration = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            double denormalizedMediaElementPosition = normalizedMediaElementPosition * totalDuration;

            TimeSpan startTime = new TimeSpan(0, 0, 0, 0, (int)denormalizedMediaElementPosition);
            TimeSpan endTime = new TimeSpan(0, 0, 0, 0, (int)(totalDuration * xAudioWrapper.AudioEnd));

            MediaElement.Position = startTime;

            MediaElement.Markers.Remove(EndMarker);

            StartMarker = new TimelineMarker();
            StartMarker.Time = startTime;
            EndMarker = new TimelineMarker();
            EndMarker.Time = endTime;

            MediaElement.Markers.Add(EndMarker);

            ScrubBar.Minimum = totalDuration * xAudioWrapper.AudioStart;
            ScrubBar.Maximum = totalDuration * xAudioWrapper.AudioEnd;

            // set the right time stamp
            var converter = new PositionToStringConverter();
            var timeSpan = new TimeSpan(0, 0, 0, 0, (int)(totalDuration * xAudioWrapper.AudioEnd));
            xRightTimeStampTextBlock.Text = (string)converter.Convert(timeSpan, null, null, null); // this looks weird cause its a xaml converter 

        }

        /// <summary>
        /// Clears MediaElement TimelineMarkers and refreshes them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="regionMarkers"></param>
        private void XAudioWrapper_OnRegionsUpdated(object sender, List<double> regionMarkers)
        {
            
            MediaElement.Markers.Clear();
            //Start and end must be preserved
       //     MediaElement.Markers.Add(StartMarker);
            MediaElement.Markers.Add(EndMarker);
            
            foreach (var normalizedTimelineMarkerTime in regionMarkers)
            {
                var marker = new TimelineMarker();
                double totalDuration = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
                marker.Time = new TimeSpan(0,0,0,0, (int)(normalizedTimelineMarkerTime * totalDuration));
                //adds each marker to the mediaelement's markers
                MediaElement.Markers.Add(marker);
            }
        }

        private void Stop_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaElement.CurrentState != MediaElementState.Stopped)
            {
                MediaElement.Stop();
                Play.Visibility = Visibility.Visible;
                Pause.Visibility = Visibility.Collapsed;
            }
        }

        private void Pause_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaElement.CurrentState != MediaElementState.Paused)
            {
                MediaElement.Pause();
                Play.Visibility = Visibility.Visible;
                Pause.Visibility = Visibility.Collapsed;
            }
        }

        private async void Play_OnTapped(object sender, TappedRoutedEventArgs e)
        {

            if (MediaElement.CurrentState != MediaElementState.Playing)
            {
                MediaElement.Play();
                Play.Visibility = Visibility.Collapsed;
                Pause.Visibility = Visibility.Visible;
            }
        }


        private void MediaElement_OnMediaOpened(object sender, RoutedEventArgs e)
        {


            positionBinding.ElementName = "MediaElement";
            positionBinding.Path = new PropertyPath("Position.TotalMilliseconds");
            positionBinding.Mode = BindingMode.TwoWay;

            ProgressBar.SetBinding(ProgressBar.ValueProperty, positionBinding);

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

        //    MediaElement.Markers.Add(StartMarker);
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

            xAudioWrapper.CheckTimeForRegions(normalizedMediaElementPosition);

        }

        private void ProgressBar_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (MediaElement.CurrentState == MediaElementState.Playing)
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
            //Binding b = new Binding();
            //b.ElementName = "MediaElement";
            //b.Path = new PropertyPath("Position.TotalMilliseconds");
            //ProgressBar.SetBinding(ProgressBar.ValueProperty, b);
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
                //If not in bounds of current audio, don't update mediaelement position

                if (StartMarker == null || EndMarker == null)
                {
                    return;
                }
                Debug.Assert(StartMarker != null);
                Debug.Assert(EndMarker != null);
                if (time.CompareTo(StartMarker.Time) < 0 || time.CompareTo(EndMarker.Time) > 0)
                {
                    return;
                }
                MediaElement.Position = time;


                e.Handled = true;
            }

        }

        private void MediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            Restart(new TimeSpan(0));
        }

        public void Restart(TimeSpan time)
        {
            MediaElement.Position = time;
            Play.Visibility = Visibility.Visible;
            Pause.Visibility = Visibility.Collapsed;

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

        /// <summary>
        /// Called whenever the mediaelement reaches one of its TimelineMarkers.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaElement_MarkerReached(object sender, TimelineMarkerRoutedEventArgs e)
        {
            if (e.Marker.Time == StartMarker.Time)
            {
            }
            else if (e.Marker.Time == EndMarker.Time)
            {
                //Goes back to start of region
                MediaElement.Pause();
                Restart(StartMarker.Time);

            }
            //*** To avoid rounding issues, denormalized time of marker, as well as total duration, must both be
            //*** passed in because accurate check can't be made otherwise


            double totalDuration = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            var denormalizedTime = e.Marker.Time.TotalMilliseconds;
            xAudioWrapper.CheckMarker(denormalizedTime, totalDuration);
        }
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var rect = new Rect(0, 0,this.ActualWidth,this.ActualHeight);
            var rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = rect;
            this.Clip = rectangleGeometry;
        }


        public void Dispose()
        {
            MediaElement.Stop();
            xAudioWrapper.OnRegionsUpdated -= XAudioWrapper_OnRegionsUpdated;
            xAudioWrapper.OnRegionSeeked -= onSeekedTo;
            DataContextChanged -= AudioMediaPlayer_DataContextChanged;
            xAudioWrapper.Dispose();
        }
    }

}
