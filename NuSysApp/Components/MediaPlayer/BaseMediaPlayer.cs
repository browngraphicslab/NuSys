using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    /// <summary>
    /// the base class for the media players.  
    /// </summary>
    public class BaseMediaPlayer : Canvas
    {
        /// <summary>
        /// the current library element controller for this media player.
        /// </summary>
        public AudioLibraryElementController CurrentLibraryElementController { get; private set; }

        /// <summary>
        /// the media element xaml component that will be playing the audio and video.
        /// </summary>
        protected MediaElement MediaElement = new MediaElement();

        /// <summary>
        /// the progress bar of the media element
        /// </summary>
        protected MediaPlayerProgressBar ProgressBar = new MediaPlayerProgressBar();

        protected Button PlayPauseButton = new Button()
        {
            Content = "play",
            Width = 80,
            Height = 35,
        };

        private readonly static int TimerMillisecondDelay = 10;

        private Timer _tickTimer;

        public BaseMediaPlayer()
        {
            Children.Add(MediaElement);
            Children.Add(ProgressBar);
            Children.Add(PlayPauseButton);
            ProgressBar.Tapped += ProgressBarOnTapped;

            _tickTimer = new Timer(TimerTick, null, Timeout.Infinite, Timeout.Infinite);
            MediaElement.AutoPlay = false;
            MediaElement.MediaOpened += MediaElementOnLoaded;
            PlayPauseButton.Tapped += PlayPauseButtonOnTapped;
            PlayPauseButton.RenderTransform = new TranslateTransform();
            ProgressBar.RenderTransform = new TranslateTransform();
        }

        private void ProgressBarOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            var newMilliseconds = (int)((((tappedRoutedEventArgs.GetPosition(ProgressBar).X/ProgressBar.Width)*
                       CurrentLibraryElementController.AudioLibraryElementModel.NormalizedDuration) +
                      CurrentLibraryElementController.AudioLibraryElementModel.NormalizedStartTime)*
                     MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds);
            MediaElement.Position = new TimeSpan(0,0,0,0,newMilliseconds);
            TimerTick("Forced");
        }

        public void Dispose()
        {
            if (ProgressBar != null)
            {
                ProgressBar.Tapped += ProgressBarOnTapped;
                ProgressBar.Dispose();
            }
            if (MediaElement != null)
            {
                MediaElement.MediaOpened += MediaElementOnLoaded;
            }
            if (PlayPauseButton != null)
            {
                PlayPauseButton.Tapped += PlayPauseButtonOnTapped;
            }
        }

        private void PlayPauseButtonOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            TogglePlayPause();
        }

        /// <summary>
        /// method called every TimerMillisecondDelay milliseconds when the video is being played.
        /// </summary>
        /// <param name="state"></param>
        private void TimerTick(object state)
        {
            UITask.Run(delegate
            {
                var duration = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
                var currentPosition = MediaElement.Position.TotalMilliseconds;
                var max = (CurrentLibraryElementController.AudioLibraryElementModel.NormalizedDuration + CurrentLibraryElementController.AudioLibraryElementModel.NormalizedStartTime ) * duration;

                if (currentPosition >= max)
                {
                    MediaElement.Position = new TimeSpan(0, 0, 0, 0, (int) max);
                    Pause();
                }
                var normalizedPosition = currentPosition/duration;
                ProgressBar.UpdateTime(normalizedPosition);
            });
        }

        public void TogglePlayPause()
        {
            Debug.Assert(MediaElement != null);
            if (MediaElement == null)
            {
                return;
            }
            if (MediaElement.CurrentState == MediaElementState.Playing)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        public void Pause()
        {
            MediaElement.Pause();
            _tickTimer.Change(Timeout.Infinite, Timeout.Infinite);
            PlayPauseButton.Content = "play";
        }

        public void Play()
        {
            var duration = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;

            var currentPosition = MediaElement.Position.TotalMilliseconds;
            var min = CurrentLibraryElementController.AudioLibraryElementModel.NormalizedStartTime*duration;
            var max = (CurrentLibraryElementController.AudioLibraryElementModel.NormalizedDuration + min) * duration;

            if (currentPosition >= max)
            {
                MediaElement.Position = new TimeSpan(0,0,0,0, (int )max);
                return;
            }
            else if (currentPosition <= min)
            {
                MediaElement.Position = new TimeSpan(0, 0, 0, 0, (int)min);
            }

            MediaElement.Play();
            _tickTimer.Change(new TimeSpan(0, 0, 0, 0, TimerMillisecondDelay), new TimeSpan(0, 0, 0, 0, TimerMillisecondDelay));
            PlayPauseButton.Content = "pause";
        }


        /// <summary>
        /// the method that should be called to set the size of the media element itself.  
        /// This should be overriden in the sub classes.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public virtual void SetSize(double width, double height)
        {
            Debug.Assert(MediaElement != null, "this should never be null");
            Width = width;
            Height = height;
            MediaElement.Width = width;
            MediaElement.Height = height;
            ProgressBar.SetWidth(width);
            ((TranslateTransform) PlayPauseButton.RenderTransform).Y = height;
            ((TranslateTransform)PlayPauseButton.RenderTransform).X = (width + PlayPauseButton.Width )/ 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        public virtual void SetLibraryElement(AudioLibraryElementController controller)
        {
            Debug.Assert(controller != null);
            if (controller != CurrentLibraryElementController)
            {
                CurrentLibraryElementController = controller;
                ProgressBar.SetLibraryElementController(controller);
                MediaElement.Source = new Uri(controller.Data);
            }
        }

        private void MediaElementOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Play();
        }
    }
}
