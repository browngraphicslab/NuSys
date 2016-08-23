using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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
        public event PauseEventHandler OnPause;

        public delegate void ScrubJumpEventHandler(MediaElement playbackElement);
        public event ScrubJumpEventHandler OnScrubJump;

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
            if (_playbackElement.CurrentState != MediaElementState.Playing)
            {
                _playbackElement.Play();
                OnPlay?.Invoke(_playbackElement);
            }
        }

        public void Stop()
        {
            if (_playbackElement.CurrentState != MediaElementState.Stopped)
            {
                _playbackElement.Position = new TimeSpan(0);

                _playbackElement.Stop();
                OnStop?.Invoke(_playbackElement);
                OnScrubJump?.Invoke(_playbackElement);
            }
            
        }

        public void Pause()
        {
            if (_playbackElement.CurrentState != MediaElementState.Paused)
            {
                _playbackElement.Pause();
                OnPause?.Invoke(_playbackElement);
            }
        }

        public void ScrubJump(TimeSpan time)
        {
            _playbackElement.Position = time;
            OnScrub?.Invoke(_playbackElement);
        }

        public void Scrub()
        {
            OnScrub?.Invoke(_playbackElement);
        }

    }
}