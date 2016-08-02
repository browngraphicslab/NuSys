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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoMediaPlayer : UserControl
    {
        private bool _loaded = false;
        //needed for "dragging" through scrub bar
        private bool _wasPlaying = false;
        public VideoMediaPlayer()
        {
            this.InitializeComponent();
        }

        public Uri Source
        {
            get { return playbackElement.Source; }
            set { playbackElement.Source = value; }
        }
        public TimeSpan Position
        {
            get { return playbackElement.Position; }
        }
        public TimelineMarker StartMarker { set; get; }
        public TimelineMarker EndMarker { set; get; }

        public MediaElement MediaPlayer => this.playbackElement;
        public ProgressBar ScrubBar => this.scrubBar;

        private void PlaybackElement_Onloaded(object sender, RoutedEventArgs e)
        {
        }

        private void PlaybackElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (DataContext is VideoDetailHomeTabViewModel)
            {
                var vmodel = DataContext as VideoDetailHomeTabViewModel;
                xAudioWrapper.Controller = vmodel.LibraryElementController;
            }
            else if (DataContext is VideoNodeViewModel)
            {
                var vmodel = DataContext as VideoNodeViewModel;
                xAudioWrapper.Controller = vmodel.Controller.LibraryElementController;
            }
            else
            {
                Debug.Fail("We should always be in a node or the detail view, if not we must add functionality here");
            }
            xAudioWrapper.ProcessLibraryElementController();
            if (this.DataContext is VideoNodeViewModel)
            {
                var vm = this.DataContext as VideoNodeViewModel;
                var model = vm.Model as VideoNodeModel;
                model.ResolutionX = playbackElement.AspectRatioWidth;
                model.ResolutionY = playbackElement.AspectRatioHeight;

                double width = this.ActualWidth;
                double height = this.ActualHeight;
                vm.Controller.SetSize(width, height);
            }
         //   playbackElement.Position = new TimeSpan(0);
            double normalizedMediaElementPosition = xAudioWrapper.AudioStart;
            double totalDuration = playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            double denormalizedMediaElementPosition = normalizedMediaElementPosition * totalDuration;

            TimeSpan startTime = new TimeSpan(0, 0, 0, 0, (int)denormalizedMediaElementPosition);
            TimeSpan endTime = new TimeSpan(0, 0, 0, 0, (int)(totalDuration * xAudioWrapper.AudioEnd));

            playbackElement.Position = startTime;

            playbackElement.Markers.Clear();

            StartMarker = new TimelineMarker();
            StartMarker.Time = startTime;
            EndMarker = new TimelineMarker();
            EndMarker.Time = endTime;

            playbackElement.Markers.Add(StartMarker);
            playbackElement.Markers.Add(EndMarker);


            ScrubBar.Minimum = totalDuration * xAudioWrapper.AudioStart;
            ScrubBar.Maximum = totalDuration * xAudioWrapper.AudioEnd;


            // set the right time stamp
            var converter = new PositionToStringConverter();
            var timeSpan = new TimeSpan(0, 0, 0, 0, (int)(totalDuration * xAudioWrapper.AudioEnd));
            xRightTimeStampTextBlock.Text = (string)converter.Convert(timeSpan, null, null, null); // this looks weird cause its a xaml converter 

        }
        public int AspectHeight { get { return playbackElement.AspectRatioHeight; } }
        public int AspectWidth { get { return playbackElement.AspectRatioWidth; } }
        private void PlaybackElementOnMediaEnded(object sender, RoutedEventArgs routedEventArgs)
        {
            play.Opacity = 1;
        }

        private void PlaybackElement_OnMediaOpened(object sender, RoutedEventArgs e)
        {
//            _temporaryLinkVisual = new Line();
//            _temporaryLinkVisual.Stroke = new SolidColorBrush(Colors.Aqua);
//            Grid.SetRow(_temporaryLinkVisual, 1);
//            _temporaryLinkVisual.StrokeThickness = scrubBar.ActualHeight;
//            _temporaryLinkVisual.Y1 = scrubBar.ActualHeight / 2 + scrubBar.Margin.Top;
//            _temporaryLinkVisual.Y2 = scrubBar.ActualHeight / 2 + scrubBar.Margin.Top;
//            _temporaryLinkVisual.PointerMoved += ScrubBar_OnPointerMoved;
//            _temporaryLinkVisual.PointerReleased += ScrubBar_OnPointerReleased;
//            _temporaryLinkVisual.Opacity = 1;
//                Binding b = new Binding();
//                b.ElementName = "playbackElement";
//                b.Path = new PropertyPath("Position.TotalMilliseconds");
//                scrubBar.SetBinding(ProgressBar.ValueProperty, b);
//
//                playbackElement.Play();
            
        }



        private void OnStop_Click(object sender, TappedRoutedEventArgs e)
        {
            playbackElement.Pause();
            _wasPlaying = false;

        }


        private async void OnPlay_Click(object sender, RoutedEventArgs e)
        {

            Binding b = new Binding();
            b.ElementName = "playbackElement";
            b.Path = new PropertyPath("Position.TotalMilliseconds");
            scrubBar.SetBinding(ProgressBar.ValueProperty, b);
            playbackElement.Play();
            _wasPlaying = true;
        }

        public void StopVideo()
        {
            playbackElement.Pause();
            _wasPlaying = false;
        }

        private void OnPause_Click(object sender, RoutedEventArgs e)
        {
            playbackElement.Stop();
            scrubBar.Value = 0;
            _wasPlaying = false;

            // e.Handled = true;
        }
        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            VideoNodeView_OnJump(new TimeSpan(0));
            _wasPlaying = false;

        }


        public void VideoNodeView_OnJump(TimeSpan time)
        {
            playbackElement.Position = time;
            if (playbackElement.CurrentState != MediaElementState.Playing)
            {
                Binding b = new Binding();
                b.ElementName = "playbackElement";
                b.Path = new PropertyPath("Position.TotalMilliseconds");
                scrubBar.SetBinding(ProgressBar.ValueProperty, b);

            }
        }

        public void onSeekedTo(double time)
        {
            double millliseconds = playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds * time;

            TimeSpan timespan = new TimeSpan(0, 0, 0, 0, (int)millliseconds);
            playbackElement.Position = timespan;
            Binding b = new Binding();
            b.ElementName = "playbackElement";
            b.Path = new PropertyPath("Position.TotalMilliseconds");
            scrubBar.SetBinding(ProgressBar.ValueProperty, b);
        }
        private void ControllerOnDisposed(object source, object args)
        {
            playbackElement.Stop();
            var vm = (VideoNodeViewModel) DataContext;
            vm.Controller.LibraryElementController.Loaded -= LoadVideo;
            vm.Controller.Disposed -= ControllerOnDisposed;
            DataContext = null;
        }

        private void LoadVideo(object sender)
        {
            var vm = DataContext as VideoNodeViewModel;
            playbackElement.Source = vm.Controller.LibraryElementController.GetSource();
        }

        private void SrubBar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            double position = e.GetPosition((UIElement)sender).X / scrubBar.ActualWidth;
            double normalizedMediaElementPosition = xAudioWrapper.AudioStart + position * (xAudioWrapper.AudioEnd - xAudioWrapper.AudioStart);
            double totalDuration = playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            double denormalizedMediaElementPosition = normalizedMediaElementPosition * totalDuration;
            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)denormalizedMediaElementPosition);
            playbackElement.Position = time;

            xAudioWrapper.CheckTimeForRegions(normalizedMediaElementPosition);

            if (playbackElement.CurrentState != MediaElementState.Playing)
            {

                Binding b = new Binding();
                b.ElementName = "playbackElement";
                b.Path = new PropertyPath("Position.TotalMilliseconds");
                scrubBar.SetBinding(ProgressBar.ValueProperty, b);
                //    MediaElement.Pause();
            }

        }

        private void ScrubBar_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {

            if (e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed)
            {
                double position = e.GetCurrentPoint((UIElement)sender).Position.X / scrubBar.ActualWidth;
                double normalizedMediaElementPosition = xAudioWrapper.AudioStart + position * (xAudioWrapper.AudioEnd - xAudioWrapper.AudioStart);
                double totalDuration = playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds;
                double denormalizedMediaElementPosition = normalizedMediaElementPosition * totalDuration;
                TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)denormalizedMediaElementPosition);
                //If not in bounds of current audio, don't update mediaelement position
                if (time.CompareTo(StartMarker.Time) < 0 || time.CompareTo(EndMarker.Time) > 0)
                {
                    return;
                }
                playbackElement.Position = time;

                if (playbackElement.CurrentState != MediaElementState.Playing)
                {
                    Binding b = new Binding();
                    b.ElementName = "playbackElement";
                    b.Path = new PropertyPath("Position.TotalMilliseconds");
                    scrubBar.SetBinding(ProgressBar.ValueProperty, b);

                }
                else
                {
                    ((UIElement)sender).CapturePointer(e.Pointer);
                    //MediaElement.Pause();
                }

                e.Handled = true;

            }
        }




        private void ScrubBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
       }



        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)DataContext;
            vm.Controller.RequestDelete();
        }




        private void ScrubBar_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_wasPlaying)
            {
                playbackElement.Play();

            }
            else
            {
                playbackElement.Pause();

            }
                ((UIElement)sender).ReleasePointerCapture(e.Pointer);
        }

        public void StopMedia()
        {
            playbackElement.Stop();
        }

        private void MediaElement_MarkerReached(object sender, TimelineMarkerRoutedEventArgs e)
        {
            if (e.Marker.Time == StartMarker.Time)
            {
            }
            else if (e.Marker.Time == EndMarker.Time)
            {
                //Goes back to start of region
                playbackElement.Pause();
                VideoNodeView_OnJump(StartMarker.Time);

            }
            //*** To avoid rounding issues, denormalized time of marker, as well as total duration, must both be
            //*** passed in because accurate check can't be made otherwise


            double totalDuration = playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds;
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
