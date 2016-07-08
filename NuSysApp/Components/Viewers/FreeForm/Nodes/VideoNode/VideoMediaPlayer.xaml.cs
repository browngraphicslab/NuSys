using System;
using System.Collections.Generic;
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
        Boolean _loaded = false;
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

        public MediaElement MediaPlayer => this.playbackElement;
        public ProgressBar ScrubBar => this.scrubBar;

        private void PlaybackElement_Onloaded(object sender, RoutedEventArgs e)
        {
        }

        private void PlaybackElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as VideoNodeViewModel;
            if (vm == null)
            {
                return;
            }
            var model = vm.Model as VideoNodeModel;
            model.ResolutionX = playbackElement.AspectRatioWidth;
            model.ResolutionY = playbackElement.AspectRatioHeight;

            double width = this.ActualWidth;
            double height = this.ActualHeight;
            vm.Controller.SetSize(width, height);
            playbackElement.Position = new TimeSpan(0);
            
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

        }


        private async void OnPlay_Click(object sender, RoutedEventArgs e)
        {

            Binding b = new Binding();
            b.ElementName = "playbackElement";
            b.Path = new PropertyPath("Position.TotalMilliseconds");
            scrubBar.SetBinding(ProgressBar.ValueProperty, b);
            playbackElement.Play();
        }

        public void StopVideo()
        {
            playbackElement.Pause();
        }

        private void OnPause_Click(object sender, RoutedEventArgs e)
        {
            playbackElement.Stop();
            scrubBar.Value = 0;
           // e.Handled = true;
        }
        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            VideoNodeView_OnJump(new TimeSpan(0));
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
            double ratio = e.GetPosition((UIElement)sender).X / scrubBar.ActualWidth;
            double millliseconds = playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds * ratio;

            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)millliseconds);
            playbackElement.Position = time;

            if (playbackElement.CurrentState != MediaElementState.Playing)
            {
                Binding b = new Binding();
                b.ElementName = "playbackElement";
                b.Path = new PropertyPath("Position.TotalMilliseconds");
                scrubBar.SetBinding(ProgressBar.ValueProperty, b);
                //playbackElement.Play();
            }

        }

        private void ScrubBar_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {

            if (e.GetCurrentPoint((UIElement) sender).Properties.IsLeftButtonPressed)
            {
                    double ratio = e.GetCurrentPoint((UIElement) sender).Position.X/scrubBar.ActualWidth;
                    double seconds = playbackElement.NaturalDuration.TimeSpan.TotalSeconds*ratio;

                    TimeSpan time = new TimeSpan(0, 0, (int) seconds);
                    playbackElement.Position = time;
                    if (playbackElement.CurrentState != MediaElementState.Playing)
                    {
                        Binding b = new Binding();
                        b.ElementName = "playbackElement";
                        b.Path = new PropertyPath("Position.TotalMilliseconds");
                        scrubBar.SetBinding(ProgressBar.ValueProperty, b);

                        //playbackElement.Play();
                    }
                    else
                    {
                        ((UIElement) sender).CapturePointer(e.Pointer);
                        playbackElement.Pause();
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
                playbackElement.Play();
                ((UIElement)sender).ReleasePointerCapture(e.Pointer);
        }

        public void StopMedia()
        {
            playbackElement.Stop();
        }

    }
}
