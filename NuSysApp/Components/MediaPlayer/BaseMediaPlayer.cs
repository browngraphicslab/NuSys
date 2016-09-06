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
        private MediaElement _mediaElement = new MediaElement();

        /// <summary>
        /// the progress bar of the media element
        /// </summary>
        private MediaPlayerProgressBar _progressBar = new MediaPlayerProgressBar();

        private Button _playPauseButton = new Button()
        {
            Content = "play",
            Width = 80,
            Height = 35,
        };

        private readonly static int TimerMillisecondDelay = 10;

        private Timer _tickTimer;

        public BaseMediaPlayer()
        {
            Children.Add(_mediaElement);
            Children.Add(_progressBar);
            Children.Add(_playPauseButton);
            _tickTimer = new Timer(TimerTick, null, Timeout.Infinite, Timeout.Infinite);
            _mediaElement.AutoPlay = false;
            _mediaElement.MediaOpened += MediaElementOnLoaded;
            _playPauseButton.Tapped += PlayPauseButtonOnTapped;
            _playPauseButton.RenderTransform = new TranslateTransform();
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
                var duration = _mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
                var currentPosition = _mediaElement.Position.TotalMilliseconds;
                var max = (CurrentLibraryElementController.AudioLibraryElementModel.NormalizedDuration + CurrentLibraryElementController.AudioLibraryElementModel.NormalizedStartTime ) * duration;

                if (currentPosition >= max)
                {
                    _mediaElement.Position = new TimeSpan(0, 0, 0, 0, (int) max);
                    Pause();
                }
                var normalizedPosition = currentPosition/duration;
                _progressBar.UpdateTime(normalizedPosition);
            });
        }

        public void TogglePlayPause()
        {
            Debug.Assert(_mediaElement != null);
            if (_mediaElement == null)
            {
                return;
            }
            if (_mediaElement.CurrentState == MediaElementState.Playing)
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
            _mediaElement.Pause();
            _tickTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _playPauseButton.Content = "play";
        }

        public void Play()
        {
            var duration = _mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;

            var currentPosition = _mediaElement.Position.TotalMilliseconds;
            var min = CurrentLibraryElementController.AudioLibraryElementModel.NormalizedStartTime*duration;
            var max = (CurrentLibraryElementController.AudioLibraryElementModel.NormalizedDuration + min) * duration;

            if (currentPosition >= max)
            {
                _mediaElement.Position = new TimeSpan(0,0,0,0, (int )max);
                return;
            }
            else if (currentPosition <= min)
            {
                _mediaElement.Position = new TimeSpan(0, 0, 0, 0, (int)min);
            }

            _mediaElement.Play();
            _tickTimer.Change(new TimeSpan(0, 0, 0, 0, TimerMillisecondDelay), new TimeSpan(0, 0, 0, 0, TimerMillisecondDelay));
            _playPauseButton.Content = "pause";
        }


        /// <summary>
        /// the method that should be called to set the size of the media element itself.  
        /// This should be overriden in the sub classes.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public virtual void SetSize(double width, double height)
        {
            Debug.Assert(_mediaElement != null, "this should never be null");
            Width = width;
            Height = height;
            _mediaElement.Width = width;
            _mediaElement.Height = height;
            _progressBar.SetSize(width);
            ((TranslateTransform) _playPauseButton.RenderTransform).Y = height;
            ((TranslateTransform)_playPauseButton.RenderTransform).X = (width + _playPauseButton.Width )/ 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        public virtual void SetLibraryElement(AudioLibraryElementController controller)
        {
            Debug.Assert(controller != null);
            CurrentLibraryElementController = controller;
            _progressBar.SetLibraryElementController(controller);
            _mediaElement.Source = new Uri(controller.Data);
        }

        private void MediaElementOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Play();
        }
    }
}
