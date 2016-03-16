using System;
using Windows.UI.Xaml.Controls;

namespace NuSysApp.Controller
{
    public class MediaController
    {
        private MediaElement _playbackElement;
        public delegate void PlayEventHandler(MediaElement playbackElement);
        public event PlayEventHandler OnPlay;

        public delegate void StopEventHandler(MediaElement playbackElement);
        public event StopEventHandler OnStop;

        public delegate void PauseEventHandler(MediaElement playbackElement);
        public event PauseEventHandler OnPause = delegate {};

        public delegate void ScrubEventHandler(MediaElement playbackElement);
        public event ScrubEventHandler OnScrub;

        public MediaController(MediaElement playbackElement)
        {
            _playbackElement = playbackElement;
        }

        public MediaElement PlaybackElement
        {
            get { return _playbackElement; }
        }

        public void Play()
        {
            _playbackElement.Play();
            OnPlay?.Invoke(_playbackElement);
        }

        public void Stop()
        {
            _playbackElement.Position = new TimeSpan(0);

            _playbackElement.Stop();
            OnStop?.Invoke(_playbackElement);
            OnScrub?.Invoke(_playbackElement);
        }

        public void Pause()
        {
            _playbackElement.Pause();
            OnPause?.Invoke(_playbackElement);
        }

        public void Scrub(TimeSpan time)
        {
            _playbackElement.Position = time;
            OnScrub?.Invoke(_playbackElement);
        }

    }
}