using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class ScrubBarUIElement : RectangleUIElement
    {
        /// <summary>
        /// The media element associated with thsi
        /// </summary>
        private MediaElement _mediaElement;

        /// <summary>
        /// The highlighted portion of the scrubber
        /// </summary>
        private RectangleUIElement _highlightRect;

        /// <summary>
        /// The nonhighlighted portion of the scrubber
        /// </summary>
        private RectangleUIElement _backgroundRect;

        /// <summary>
        /// The selection bar for the scrubber
        /// </summary>
        private RectangleUIElement _scrubberBar;

        /// <summary>
        /// The helper value for the ScrubBarHighlightColor
        /// </summary>
        private Color _scrubBarHighlightColor = UIDefaults.ScrubBarHighlightColor;

        /// <summary>
        /// The color showing how much of the slider has been set, like the red color on a youtube player, the color to the left of the current position
        /// </summary>
        public Color ScrubBarHighlightColor
        {
            get { return _scrubBarHighlightColor; }
            set
            {
                _scrubBarHighlightColor = value;
                _highlightRect.Background = value;
            }
        }

        /// <summary>
        /// Helper value for ScrubBarBackgroundColor
        /// </summary>
        private Color _scrubBarBackgroundColor = UIDefaults.ScrubBarBackgroundColor;

        /// <summary>
        /// The regular background color for the scrubbar, the color to the right of the current position
        /// </summary>
        public Color ScrubBarBackgroundColor
        {
            get { return _scrubBarBackgroundColor; }
            set
            {
                _scrubBarBackgroundColor = value;
                _backgroundRect.Background = value;
            }
        }

        /// <summary>
        /// Helper value for scrubber bar color
        /// </summary>
        private Color _scrubberBarColor = UIDefaults.ScrubberBarColor;

        /// <summary>
        /// The color of the scrubber bar, which shows the current position on the scrub bar
        /// </summary>
        public Color ScrubberBarColor
        {
            get { return _scrubberBarColor; }
            set
            {
                _scrubberBarColor = value;
                _scrubberBar.Background = _scrubberBarColor;
            }
        }

        /// <summary>
        /// The width of the scrubber bar
        /// </summary>
        private float _scrubberBarWidth = 5;


        /// <summary>
        /// Helper value for the ScrubberPosition
        /// </summary>
        private float _scrubberPosition = UIDefaults.ScrubberPosition;

        private double _durationInMillis;
        private double _startTimeInMillis;
        private AudioLibraryElementController _controller;

        /// <summary>
        /// The position the scrubberBar is set to on the scrubber, this is normalized, so set to between 0 and 1.
        /// </summary>
        public float ScrubberPosition
        {
            get { return _scrubberPosition; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                else if (value > 1)
                {
                    value = 1;
                }
                _scrubberPosition = value;
            }
        }


        public ScrubBarUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, AudioLibraryElementController controller, MediaElement mediaElement) : base(parent, resourceCreator)
        {
            _controller = controller;
            _mediaElement = mediaElement;
            _mediaElement.MediaOpened += MediaElementOnMediaOpened;

            _backgroundRect = new RectangleUIElement(this, resourceCreator);
            InitializeBackgroundRectUI(_backgroundRect);
            AddChild(_backgroundRect);

            _highlightRect = new RectangleUIElement(this, resourceCreator);
            InitializeHighlightRectUI(_highlightRect);
            _highlightRect.IsHitTestVisible = false;
            AddChild(_highlightRect);

            _scrubberBar = new RectangleUIElement(this, resourceCreator);
            InitializeScubberBarUI(_scrubberBar);
            _scrubberBar.IsHitTestVisible = false;
            AddChild(_scrubberBar);

            // add manipulation events
            _backgroundRect.Dragged += OnScrubberDragged;
            _backgroundRect.Tapped += OnScrubberTapped;

        }

        private void MediaElementOnMediaOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _durationInMillis = _mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            _startTimeInMillis = _controller.AudioLibraryElementModel.NormalizedStartTime *
                                 _mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
        }


        /// <summary>
        /// Seeks to the point that was tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnScrubberTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            ScrubberPosition = currPoint.X/Width;

            UITask.Run(() =>
            {
                SetMediaElementToCurrentScrubPosition(_mediaElement);

            });
        }

        /// <summary>
        /// The dispose event remove manipulation events here
        /// </summary>
        public override void Dispose()
        {
            _scrubberBar.Dragged -= OnScrubberDragged;
            _backgroundRect.Tapped -= OnScrubberTapped;
            _highlightRect.Tapped -= OnScrubberTapped;
            _mediaElement.MediaOpened -= MediaElementOnMediaOpened;
        }

        /// <summary>
        /// Fired when the scrubber bar is dragged changes the position of the scrubber
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnScrubberDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            ScrubberPosition = currPoint.X / Width;
            UITask.Run(() =>
            {
                SetMediaElementToCurrentScrubPosition(_mediaElement);

            });
        }

        /// <summary>
        /// Initialize the ui for the scrubber bar, the portion that sets the selected point in time on the scrubber
        /// </summary>
        /// <param name="scrubberBar"></param>
        private void InitializeScubberBarUI(RectangleUIElement scrubberBar)
        {
            scrubberBar.Background = ScrubberBarColor;
            scrubberBar.Width = _scrubberBarWidth;
        }


        /// <summary>
        /// Initialize the background for the scrubber bar, the part to the right of the current position
        /// </summary>
        /// <param name="backgroundRect"></param>
        private void InitializeBackgroundRectUI(RectangleUIElement backgroundRect)
        {
            backgroundRect.Background = ScrubBarBackgroundColor;
        }

        /// <summary>
        /// Initialize the highlight for the scrubber bar, the part to the left of the current position
        /// </summary>
        /// <param name="highlightRect"></param>
        private void InitializeHighlightRectUI(RectangleUIElement highlightRect)
        {
            highlightRect.Background = ScrubBarHighlightColor;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // set the scrubber position based on the current position of the media player
            UITask.Run(() =>
            {
                ScrubberPosition = (float)_mediaElement.Position.TotalMilliseconds /
                                  (float)_mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;

            });

            if (float.IsNaN(ScrubberPosition))
            {
                return;
            }

            // get helper values for setting the offsets and position of elements
            var scrubberHeight = Height * 2 / 3;
            var scrubberVerticalOffset = (Height - scrubberHeight) / 2;

            // set the highlighted portion of the scrubber so that it is to the left of the scrubbar
            _highlightRect.Width = Math.Max(0, Math.Min(ScrubberPosition, 1))*Width;
            _highlightRect.Height = scrubberHeight;
            _highlightRect.Transform.LocalPosition = new Vector2(0, scrubberVerticalOffset);

            // set the unhighlited portion of the scrubber so that it is to the rigth of the scrubbar

            _backgroundRect.Width = Width;
            _backgroundRect.Height = scrubberHeight;
            _backgroundRect.Transform.LocalPosition = new Vector2(0, scrubberVerticalOffset);

            // set the scrubberbar size and position based on the slider position
            _scrubberBar.Height = Height;
            _scrubberBar.Transform.LocalPosition = new Vector2(Math.Max(0, Math.Min(ScrubberPosition, 1)) * Width - _scrubberBarWidth / 2, 0);

            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// Sets the media element to the current scrub position
        /// </summary>
        /// <param name="mediaElement"></param>
        private void SetMediaElementToCurrentScrubPosition(MediaElement mediaElement)
        {
            var currPositionInMilliSeconds = _durationInMillis * ScrubberPosition + _startTimeInMillis;
            var ts = new TimeSpan(0,0,0,0, (int) currPositionInMilliSeconds);
            mediaElement.Position = ts;
        }
    }
}
