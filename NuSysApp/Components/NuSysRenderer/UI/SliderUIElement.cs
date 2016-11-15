using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class SliderUIElement : RectangleUIElement
    {
        public delegate void OnSliderMovedHandler(SliderUIElement sender, double currSliderPosition);

        public event OnSliderMovedHandler OnSliderMoved;

        /// <summary>
        /// Minimum value that the slider can represent
        /// </summary>
        public int MinValue { get; set; }

        /// <summary>
        /// Maximum value that the slider can represent
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// The thumb is the circle that you move to set new values on the slider
        /// </summary>
        private EllipseUIElement _thumb;

        /// <summary>
        /// The helper value for ThumbColor, represents the color of the thumb
        /// </summary>
        private Color _thumbColor = UIDefaults.ThumbColor;

        /// <summary>
        /// The color of the thumb (the circle you move back and forth)
        /// </summary>
        public Color ThumbColor
        {
            get { return _thumbColor; }
            set
            {
                _thumbColor = value;
                _thumb.Background = _thumbColor;
            }
        }

        /// <summary>
        /// Get the current value of the slider
        /// </summary>
        public Double CurrentValue => SliderPosition*MaxValue - MinValue;

        /// <summary>
        /// The helper value for the slider highlight color, the color showing what has currently been set, like the red color on a youtube player
        /// </summary>
        private Color _sliderHighlightColor = UIDefaults.SliderHighlightColor;

        /// <summary>
        /// The color showing how much of the slider has been set, like the red color on a youtube player, the color to the left of the current position
        /// </summary>
        public Color SliderHighlightColor
        {
            get { return _sliderHighlightColor; }
            set
            {
                _sliderHighlightColor = value;
                _highlightSlider.Background = value;
            }
        }

        /// <summary>
        /// Helper value for SliderBackgroundColor
        /// </summary>
        private Color _sliderBackgroundColor = UIDefaults.SliderBackground;

        /// <summary>
        /// The regular background color for the slider, the color to the right of the current position
        /// </summary>
        public Color SliderBackgroundColor
        {
            get { return _sliderBackgroundColor; }
            set
            {
                _sliderBackgroundColor = value;
                _backgroundSlider.Background = value;
            }
        }

        /// <summary>
        /// Helper value for the SliderPosition
        /// </summary>
        private float _sliderPosition = UIDefaults.SliderPosition;

        /// <summary>
        /// The position the thumb is set to on the slider, this is normalized, so set to between 0 and 1.
        /// </summary>
        public float SliderPosition
        {
            get { return _sliderPosition; } 
            set {
                if (value < 0)
                {
                    value = 0;
                } else if (value > 1 )
                {
                    value = 1;
                }
                _sliderPosition = value;
                OnSliderMoved?.Invoke(this, SliderPosition);
            }
        }

        /// <summary>
        /// Visibility boolean for the slider tooltip
        /// </summary>
        public bool IsTooltipEnabled = UIDefaults.IsSliderTooltipEnabled;

        /// <summary>
        /// The rectangle representing the left portion of the slider before the thumb
        /// </summary>
        private RectangleUIElement _backgroundSlider;

        /// <summary>
        /// The rectangle representing the right portion of the slider after the thumb
        /// </summary>
        private RectangleUIElement _highlightSlider;

        /// <summary>
        /// The textbox used to display the tooltip text if IsTooltipEnabled is set to true
        /// </summary>
        private TextboxUIElement _toolTipUIElement;
        
        /// <summary>
        /// The width of the slider
        /// </summary>
        public float Width
        {
            get { return base.Width; }
            set { base.Width = value; }
        }

        /// <summary>
        /// The height of the slider
        /// </summary>
        public float Height
        {
            get { return base.Height; }
            set
            {
                base.Height = value;
                _thumb.Width = Height/3;
                _thumb.Height = Height/3;
            }
        }

        /// <summary>
        /// Creates a new slider UI element, also set the MinValue and MaxValue
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public SliderUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, int minValue, int maxValue) : base(parent, resourceCreator)
        {
            _backgroundSlider = new RectangleUIElement(this, resourceCreator);
            InitializeBackgroundSliderUI(_backgroundSlider);
            AddChild(_backgroundSlider);

            _highlightSlider = new RectangleUIElement(this, resourceCreator);
            InitializeHighlightSliderUI(_highlightSlider);
            AddChild(_highlightSlider);

            _thumb = new EllipseUIElement(this, resourceCreator);
            InitializeThumbUI(_thumb);
            AddChild(_thumb);

            _toolTipUIElement = new TextboxUIElement(this, resourceCreator);
            InitializeToolTipUI(_toolTipUIElement);
            AddChild(_toolTipUIElement);

            MinValue = minValue;
            MaxValue = maxValue;

            // set the default background
            Background = Colors.Transparent;

            // add manipulation events
            _thumb.Dragged += OnThumbDragged;
            _thumb.Pressed += OnThumbPressed;
            _thumb.Released += OnThumbReleased;

        }

        /// <summary>
        /// Fired when the slider is disposed
        /// </summary>
        public override void Dispose()
        {
            _thumb.Dragged -= OnThumbDragged;
            _thumb.Pressed -= OnThumbPressed;
            _thumb.Released -= OnThumbReleased;
            base.Dispose();
        }

        /// <summary>
        /// Fired when the thumb is released causes the tool tip to dissapear
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnThumbReleased(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _toolTipUIElement.IsVisible = false;
        }

        /// <summary>
        /// Fired when the thumb is pressed causes the tool tip to appear
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnThumbPressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (IsTooltipEnabled)
            {
                _toolTipUIElement.IsVisible = true;
            }
        }

        /// <summary>
        /// Fired when the thumb is dragged, changes the position of the slider
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnThumbDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SliderPosition += pointer.DeltaSinceLastUpdate.X/Width;
        }

        /// <summary>
        /// Initialize the UI for the tool tip
        /// </summary>
        /// <param name="toolTipUiElement"></param>
        private void InitializeToolTipUI(TextboxUIElement toolTipUiElement)
        {
            toolTipUiElement.Height = 25;
            toolTipUiElement.Width = 35;
            toolTipUiElement.TextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            toolTipUiElement.TextVerticalAlignment = CanvasVerticalAlignment.Center;
            toolTipUiElement.IsVisible = false;
        }

        /// <summary>
        /// Initialize the UI for the highlight slider
        /// </summary>
        /// <param name="highlightSlider"></param>
        private void InitializeHighlightSliderUI(RectangleUIElement highlightSlider)
        {
            highlightSlider.Background = SliderHighlightColor;
        }

        /// <summary>
        /// Initialize the UI for the unhighlighted slider
        /// </summary>
        /// <param name="unhighlightSlider"></param>
        private void InitializeBackgroundSliderUI(RectangleUIElement unhighlightSlider)
        {
            unhighlightSlider.Background = SliderBackgroundColor;
        }

        /// <summary>
        /// Initialize the UI for the thumb
        /// </summary>
        /// <param name="thumb"></param>
        private void InitializeThumbUI(EllipseUIElement thumb)
        {
            thumb.Background = ThumbColor;
        }



        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // get helper values for setting the offsets and position of elements
            var thumbDiameter = Height/2;
            var thumbVerticalOffset = Height/2 - thumbDiameter/2;
            var sliderHeight = thumbDiameter/2;
            var sliderVerticalOffset = Height/2 - sliderHeight/2;

            // set the highlighted portion of the slider so that it is to the left of the thumb
            _highlightSlider.Width = SliderPosition*Width;
            _highlightSlider.Height = sliderHeight;
            _highlightSlider.Transform.LocalPosition = new Vector2(0, sliderVerticalOffset);

            // set the unhighlited portion of the slider so that it is to the rigth of the thumb
            _backgroundSlider.Width = Width;
            _backgroundSlider.Height = sliderHeight;
            _backgroundSlider.Transform.LocalPosition = new Vector2(0, sliderVerticalOffset);

            // set the thumb size and position based on the slider position
            _thumb.Width = thumbDiameter;
            _thumb.Height = thumbDiameter;
            _thumb.Transform.LocalPosition = new Vector2(SliderPosition * Width - thumbDiameter / 2, thumbVerticalOffset);

            // if the tooltip is enabled set its current position and text
            if (IsTooltipEnabled && _toolTipUIElement.IsVisible)
            {
                _toolTipUIElement.Text = ((int) (SliderPosition*MaxValue - MinValue)).ToString(CultureInfo.InvariantCulture);
                _toolTipUIElement.Transform.LocalPosition = new Vector2(SliderPosition*Width - _toolTipUIElement.Width / 2, thumbVerticalOffset - _toolTipUIElement.Height);

            }

            base.Update(parentLocalToScreenTransform);
        }
    }
}
