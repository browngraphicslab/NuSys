using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class AudioMediaContentUIElement : RectangleUIElement
    {
        private AudioLibraryElementController _controller;
        private MediaElement _mediaElement;
        private CanvasBitmap _audioWaveImage;
        private bool _isLoading;
        private RectangleUIElement _shadowRect;
        private Color _shadowColor = UIDefaults.ShadowColor;
        private float _currentShadowPosition;
        private double _durationInMillis;
        private double _startTimeInMillis;

        public AudioMediaContentUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, AudioLibraryElementController controller, MediaElement mediaElement) : base(parent, resourceCreator)
        {
            _controller = controller;
            _mediaElement = mediaElement;
            _mediaElement.MediaOpened += MediaElementOnMediaOpened;
            _isLoading = true;
            _shadowRect = new RectangleUIElement(this, resourceCreator);
            InitializeShadowRectUI(_shadowRect);
            _shadowRect.IsHitTestVisible = false;
            AddChild(_shadowRect);

            Tapped += AudioMediaContentUIElement_Tapped;
            Dragged += AudioMediaContentUIElement_Dragged;
        }

        public override void Dispose()
        {
            _mediaElement.MediaOpened -= MediaElementOnMediaOpened;
            Tapped -= AudioMediaContentUIElement_Tapped;
            Dragged -= AudioMediaContentUIElement_Dragged;
            base.Dispose();
        }

        private void MediaElementOnMediaOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _durationInMillis = _mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            _startTimeInMillis = _controller.AudioLibraryElementModel.NormalizedStartTime *
                                 _mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
        }

        private void AudioMediaContentUIElement_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            _currentShadowPosition = currPoint.X / Width;
            UITask.Run(() =>
            {
                SetMediaElementToCurrentScrubPosition(_mediaElement);

            });
        }

        private void SetMediaElementToCurrentScrubPosition(MediaElement mediaElement)
        {
            var currPositionInMilliSeconds = _durationInMillis * _currentShadowPosition + _startTimeInMillis;
            var ts = new TimeSpan(0, 0, 0, 0, (int)currPositionInMilliSeconds);
            mediaElement.Position = ts;
        }

        private void AudioMediaContentUIElement_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            _currentShadowPosition = currPoint.X / Width;

            UITask.Run(() =>
            {
                SetMediaElementToCurrentScrubPosition(_mediaElement);

            });
        }

        private void InitializeShadowRectUI(RectangleUIElement shadowRect)
        {
            shadowRect.Background = _shadowColor;
        }

        public override async Task Load()
        {
            _audioWaveImage = await CanvasBitmap.LoadAsync(Canvas, _controller.LargeIconUri);
            Image = _audioWaveImage;
            _isLoading = false;

            base.Load();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_isLoading)
            {
                return;
            }

            // set the scrubber position based on the current position of the media player
            UITask.Run(() =>
            {
                _currentShadowPosition = (float)_mediaElement.Position.TotalMilliseconds /
                                  (float)_mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;

            });

            if (float.IsNaN(_currentShadowPosition))
            {
                return;
            }

            _shadowRect.Width = Math.Min(1, Math.Max(_currentShadowPosition, 0))*Width;
            _shadowRect.Height = Height;


            base.Update(parentLocalToScreenTransform);
        }
    }
}
