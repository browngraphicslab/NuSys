using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoMediaPlayer : UserControl
    {
        public VideoMediaPlayer()
        {
            this.InitializeComponent();
            xAudioWrapper.OnRegionsUpdated += XAudioWrapper_OnRegionsUpdated;
            xAudioWrapper.OnRegionSeeked += onSeekedTo;
            xAudioWrapper.OnIntervalChanged += XAudioWrapper_OnIntervalChanged;
        }

        public Uri Source
        {
            get { return MediaElement.Source; }
            set { MediaElement.Source = value; }
        }
        public TimeSpan Position
        {
            get { return MediaElement.Position; }
        }
        public TimelineMarker StartMarker { set; get; }
        public TimelineMarker EndMarker { set; get; }
        public Binding PositionBinding { get; set; }
        public AudioWrapper AudioWrapper => xAudioWrapper;
        public MediaElement MediaPlayer => this.MediaElement;
        public ProgressBar ScrubBar => this.scrubBar;
        private void XAudioWrapper_OnRegionsUpdated(object sender, List<double> regionMarkers)
        {
            MediaElement.Markers.Clear();
            //Start and end must be preserved
            //     MediaElement.Markers.Add(StartMarker);
            if (EndMarker != null)
                MediaElement.Markers.Add(EndMarker);

            foreach (var normalizedTimelineMarkerTime in regionMarkers)
            {
                var marker = new TimelineMarker();
                double totalDuration = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
                marker.Time = new TimeSpan(0, 0, 0, 0, (int)(normalizedTimelineMarkerTime * totalDuration));
                //adds each marker to the mediaelement's markers
                MediaElement.Markers.Add(marker);
            }
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            PositionBinding = new Binding();
            PositionBinding.ElementName = "MediaElement";
            PositionBinding.Path = new PropertyPath("Position.TotalMilliseconds");
            PositionBinding.Mode = BindingMode.TwoWay;
            scrubBar.SetBinding(ProgressBar.ValueProperty, PositionBinding);
            if (DataContext is VideoDetailHomeTabViewModel)
            {
                var vmodel = DataContext as VideoDetailHomeTabViewModel;
                xAudioWrapper.Controller = vmodel.LibraryElementController;
            }
            else
            {
                // Debug.Fail("We should always be in a node or the detail view, if not we must add functionality here");
                DataContext = NuSysRenderer.Instance.ActiveVideoRenderItem.ViewModel;
                xAudioWrapper.Controller = NuSysRenderer.Instance.ActiveVideoRenderItem.ViewModel.Controller.LibraryElementController;
            }
            xAudioWrapper.ProcessLibraryElementController();

            if (this.DataContext is VideoNodeViewModel)
            {
                return;
                var vm = this.DataContext as VideoNodeViewModel;
                var model = vm.Model as VideoNodeModel;
                model.ResolutionX = MediaElement.AspectRatioWidth;
                model.ResolutionY = MediaElement.AspectRatioHeight;

                double width = this.ActualWidth;
                double height = this.ActualHeight;
                vm.Controller.SetSize(width, height);

            }
         //   MediaElement.Position = new TimeSpan(0);
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
        public int AspectHeight { get { return MediaElement.AspectRatioHeight; } }
        public int AspectWidth { get { return MediaElement.AspectRatioWidth; } }
        private void MediaElementOnMediaEnded(object sender, RoutedEventArgs routedEventArgs)
        {
            play.Opacity = 1;
        }

        private void OnStop_Click(object sender, TappedRoutedEventArgs e)
        {
            MediaElement.Pause();

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
        private async void OnPlay_Click(object sender, RoutedEventArgs e)
        {


            MediaElement.Play();
        }

        public void StopVideo()
        {
            MediaElement.Pause();
        }

        private void OnPause_Click(object sender, RoutedEventArgs e)
        {
            MediaElement.Stop();
            scrubBar.Value = 0;

            // e.Handled = true;
        }
        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            VideoNodeView_OnJump(new TimeSpan(0));

        }


        public void VideoNodeView_OnJump(TimeSpan time)
        {
            MediaElement.Position = time;

        }

        public void onSeekedTo(double time)
        {
            double millliseconds = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds * time;

            TimeSpan timespan = new TimeSpan(0, 0, 0, 0, (int)millliseconds);
            MediaElement.Position = timespan;
        }
        private void ControllerOnDisposed(object source, object args)
        {
            MediaElement.Stop();
            var vm = (VideoNodeViewModel) DataContext;
            vm.Controller.Disposed -= ControllerOnDisposed;
            DataContext = null;
        }

        private void LoadVideo(object sender)
        {
            var vm = DataContext as VideoNodeViewModel;
            MediaElement.Source = new Uri(vm.Controller.LibraryElementController.Data);
        }

        private void SrubBar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            double position = e.GetPosition((UIElement)sender).X / scrubBar.ActualWidth;
            double normalizedMediaElementPosition = xAudioWrapper.AudioStart + position * (xAudioWrapper.AudioEnd - xAudioWrapper.AudioStart);
            double totalDuration = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            double denormalizedMediaElementPosition = normalizedMediaElementPosition * totalDuration;
            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)denormalizedMediaElementPosition);
            MediaElement.Position = time;

            xAudioWrapper.CheckTimeForRegions(normalizedMediaElementPosition);

        }

        private void ScrubBar_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {

            if (e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed)
            {
                double position = e.GetCurrentPoint((UIElement)sender).Position.X / scrubBar.ActualWidth;
                double normalizedMediaElementPosition = xAudioWrapper.AudioStart + position * (xAudioWrapper.AudioEnd - xAudioWrapper.AudioStart);
                double totalDuration = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
                double denormalizedMediaElementPosition = normalizedMediaElementPosition * totalDuration;
                TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)denormalizedMediaElementPosition);
                //If not in bounds of current audio, don't update mediaelement position
                if (time.CompareTo(StartMarker.Time) < 0 || time.CompareTo(EndMarker.Time) > 0)
                {
                    return;
                }
                MediaElement.Position = time;


               ((UIElement)sender).CapturePointer(e.Pointer);


                e.Handled = true;

            }
        }


        private void ScrubBar_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
                ((UIElement)sender).ReleasePointerCapture(e.Pointer);
        }

        public void StopMedia()
        {
            MediaElement.Stop();
        }

        private void MediaElement_MarkerReached(object sender, TimelineMarkerRoutedEventArgs e)
        {
            if (StartMarker != null && e.Marker.Time == StartMarker.Time)
            {
            }
            else if (EndMarker != null && e.Marker.Time == EndMarker.Time)
            {
                //Goes back to start of region
                MediaElement.Pause();
                VideoNodeView_OnJump(StartMarker.Time);

            }
            //*** To avoid rounding issues, denormalized time of marker, as well as total duration, must both be
            //*** passed in because accurate check can't be made otherwise


            double totalDuration = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            var denormalizedTime = e.Marker.Time.TotalMilliseconds;
            xAudioWrapper.CheckMarker(denormalizedTime, totalDuration);
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var rect = new Rect(0, 0, this.ActualWidth, this.ActualHeight);
            var rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = rect;
            this.Clip = rectangleGeometry;
        }
    }
}
