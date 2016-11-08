using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class BaseMediaPlayerUIElement : RectangleUIElement
    {
        private ButtonUIElement _playPauseButton;
        private ButtonUIElement _volumeButton;
        private SliderUIElement _volumeSlider;
        private ScrubBarUIElement _scrubBar;
        private TextboxUIElement _currTimeAndDurationDisplay;
        private MediaElement _mediaElement;

        private RectangleUIElement _mediaContent;
      

        private CanvasBitmap _playImage;
        private CanvasBitmap _pauseImage;
        private CanvasBitmap _volumeImage;

        private RectangleUIElement _contentRect;


        private bool _isPlaying;

        private bool _isLoading;

        private bool _isMuted;

        private float _prevVolumePosition;

        private float _buttonsBarHeight = UIDefaults.MediaPlayerButtonBarHeight;
        private float _scubBarHeight = UIDefaults.MediaPlayerScrubBarHeight;

        /// <summary>
        /// Represents the start of the audio region
        /// </summary>
        private double _minTimeInMillis;

        /// <summary>
        /// Represents the end of the audio region
        /// </summary>
        private double _maxTimeInMillis;

        /// <summary>
        /// Represents the total time of the entire audio, not just the region
        /// </summary>
        private double _durationInMillis;

        private AudioLibraryElementController _controller;

        public BaseMediaPlayerUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

            _playPauseButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(this, resourceCreator));
            AddChild(_playPauseButton);

            _volumeButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(this, resourceCreator));
            AddChild(_volumeButton);

            _volumeSlider = new SliderUIElement(this, resourceCreator, 0, 100);
            AddChild(_volumeSlider);

            _currTimeAndDurationDisplay = new TextboxUIElement(this, resourceCreator);
            InitializeCurrTimeAndDurationDisplay(_currTimeAndDurationDisplay);
            AddChild(_currTimeAndDurationDisplay);

            _playPauseButton.OnPressed += _playPauseButton_Tapped;
            _volumeButton.OnPressed += OnVolumeButtonTapped;
            _volumeSlider.OnSliderMoved += VolumeSliderOnSliderMoved;

            _isLoading = true;

        }

        private void InitializeCurrTimeAndDurationDisplay(TextboxUIElement currTimeAndDurationDisplay)
        {
            _currTimeAndDurationDisplay.Background = Colors.Transparent;
            _currTimeAndDurationDisplay.TextHorizontalAlignment = CanvasHorizontalAlignment.Left;
            _currTimeAndDurationDisplay.TextVerticalAlignment = CanvasVerticalAlignment.Center;
        }

        public BaseMediaPlayerUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, VideoLibraryElementController controller) : this(parent, resourceCreator)
        {
            _controller = controller;
            InitializeMediaElement(controller);

            _scrubBar = new ScrubBarUIElement(this, resourceCreator, controller, _mediaElement);
            AddChild(_scrubBar);
        }

        public BaseMediaPlayerUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, AudioLibraryElementController controller) : this(parent, resourceCreator)
        {
            _controller = controller;
            InitializeMediaElement(controller);


            _mediaContent = new AudioMediaContentUIElement(this, resourceCreator, controller, _mediaElement);
            AddChild(_mediaContent);


            _scrubBar = new ScrubBarUIElement(this, resourceCreator, controller, _mediaElement);
            AddChild(_scrubBar);
        }

        public void InitializeMediaElement(AudioLibraryElementController controller)
        {
            _mediaElement = new MediaElement();
            _mediaElement.Source = new Uri(controller.Data);
            _mediaElement.AutoPlay = false;
            _mediaElement.AreTransportControlsEnabled = false;
            AddMediaElementToVisualTree(_mediaElement);

            _mediaElement.MediaOpened += OnMediaElementOpened;
        }

        private void OnMediaElementOpened(object sender, RoutedEventArgs e)
        {
            _durationInMillis = _mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            _minTimeInMillis = _controller.AudioLibraryElementModel.NormalizedStartTime *
                                 _mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            _maxTimeInMillis = _controller.AudioLibraryElementModel.NormalizedDuration *
                                 _mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds + _minTimeInMillis;

            var currentPosition = _mediaElement.Position.TotalMilliseconds;


            if (currentPosition >= _maxTimeInMillis || currentPosition <= _minTimeInMillis)
            {
                _mediaElement.Position = new TimeSpan(0, 0, 0, 0, (int)_minTimeInMillis);
            }
        }

        public void AddMediaElementToVisualTree(MediaElement _mediaElement)
        {
            SessionController.Instance.SessionView.MainCanvas.Children.Add(_mediaElement);
        }

        public void RemoveMediaElementFromVisualTree(MediaElement _mediaElement)
        {
            SessionController.Instance.SessionView.MainCanvas.Children.Remove(_mediaElement);
        }

        private void UpdateCurrentTimeAndDuration(TextboxUIElement currTimeAndDurationDisplay)
        {
           
            var currTimeInSecs = _mediaElement.Position.TotalSeconds;
            var totalTimeInSecs = _mediaElement.NaturalDuration.TimeSpan.TotalSeconds;

            var currTime = FormatTimeInHHMMSS((int) currTimeInSecs);
            var totalTime = FormatTimeInHHMMSS((int) totalTimeInSecs);
            currTimeAndDurationDisplay.Text = currTime + " \\ " + totalTime;
        }

        private string FormatTimeInHHMMSS(int timeInSecs)
        {
            var hours = timeInSecs/3600;
            var minutes = timeInSecs%3600/60;
            var seconds = timeInSecs%3600%60;

            return hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");

        }

        private void VolumeSliderOnSliderMoved(SliderUIElement sender, double currSliderPosition)
        {
            _isMuted = Math.Abs(currSliderPosition) < .001;
            UITask.Run(() =>
            {
                _mediaElement.Volume = currSliderPosition;
            });
        }

        private void OnVolumeButtonTapped(ButtonUIElement item, CanvasPointer pointer)
        {
            if (_isMuted)
            {
                UnMute();
            }
            else
            {
                Mute();
            }
        }

        private void Mute()
        {
            // save the previous volume position for unmuting
            _prevVolumePosition = _volumeSlider.SliderPosition;
            _volumeSlider.SliderPosition = 0;
            _isMuted = true;
        }

        private void UnMute()
        {
            _volumeSlider.SliderPosition = _prevVolumePosition;
            _isMuted = false;
        }

        public override void Dispose()
        {
            _playPauseButton.OnPressed -= _playPauseButton_Tapped;
            _volumeButton.OnPressed -= OnVolumeButtonTapped;
            _volumeSlider.OnSliderMoved -= VolumeSliderOnSliderMoved;
            _mediaElement.MediaOpened -= OnMediaElementOpened;
            RemoveMediaElementFromVisualTree(_mediaElement);
            base.Dispose();
        }

        public override async Task Load()
        {
            _playImage = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/node icons/icon_play.png"));
            _pauseImage = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/node icons/icon_pause.png"));
            _volumeImage = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/node icons/icon_link.png"));
            _isLoading = false;

            if (_isPlaying)
            {
                Play();
            }
            else
            {
                Pause();
            }
            _volumeButton.Image = _volumeImage;

            

            base.Load();
        }

        private void _playPauseButton_Tapped(ButtonUIElement item, CanvasPointer pointer)
        {
            if (_isPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        private void Play()
        {
            // we can't play if we're still loading the media player
            if (_isLoading)
            {
                return;
            }
            _playPauseButton.Image = _pauseImage;
            _isPlaying = true;
            UITask.Run(() =>
            {
                _mediaElement.Play();
            });
        }

        private void Pause()
        {
            // we can't pause if we are loading the media player
            if (_isLoading)
            {
                return;
            }
            _playPauseButton.Image = _playImage;
            _isPlaying = false;
            UITask.Run(() =>
            {
                _mediaElement.Pause();
            });
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_isLoading)
            {
                return;
            }

            UITask.Run(() =>
            {
                CheckIfEndOfAudio();
                UpdateCurrentTimeAndDuration(_currTimeAndDurationDisplay);

            });

            var buttonBottomSpacing = 5;
            var buttonHorizontalSpacing = 5;

            _playPauseButton.Width = 25;
            _playPauseButton.Height = 25;
            _playPauseButton.Transform.LocalPosition = new Vector2(buttonHorizontalSpacing, Height - _buttonsBarHeight / 2 - _playPauseButton.Height / 2);

            _volumeButton.Width = 25;
            _volumeButton.Height = 25;
            _volumeButton.Transform.LocalPosition = new Vector2(_playPauseButton.Width + 2 * buttonHorizontalSpacing, Height - _buttonsBarHeight / 2 - _volumeButton.Height / 2);

            _volumeSlider.Width = 100;
            _volumeSlider.Height = 25;
            _volumeSlider.Transform.LocalPosition = new Vector2(_playPauseButton.Width + _volumeButton.Width + 3 * buttonHorizontalSpacing, Height - _buttonsBarHeight / 2 - _volumeSlider.Height / 2);

            _currTimeAndDurationDisplay.Width = 200;
            _currTimeAndDurationDisplay.Height = 25;
            _currTimeAndDurationDisplay.Transform.LocalPosition = new Vector2(_volumeSlider.Width + _playPauseButton.Width + _volumeButton.Width + 5 * buttonHorizontalSpacing, Height - _buttonsBarHeight / 2 - _currTimeAndDurationDisplay.Height / 2);

            _scrubBar.Width = Width;
            _scrubBar.Height = _scubBarHeight;
            _scrubBar.Transform.LocalPosition = new Vector2(0, Height - _buttonsBarHeight - _scubBarHeight);

            _mediaContent.Width = Width;
            _mediaContent.Height = Height - _buttonsBarHeight - _scubBarHeight;
            



            base.Update(parentLocalToScreenTransform);
        }

        private void CheckIfEndOfAudio()
        {
            if (_mediaElement.Position.TotalMilliseconds > _maxTimeInMillis)
            {
                Pause();
                _mediaElement.Position = new TimeSpan(0,0,0,0,(int) _maxTimeInMillis);
            }
        }
    }

}
