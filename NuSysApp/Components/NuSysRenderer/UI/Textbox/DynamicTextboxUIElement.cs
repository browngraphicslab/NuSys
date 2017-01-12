using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class DynamicTextboxUIElement : TextboxUIElement
    {

        /// <summary>
        /// The width of the DynamicTextboxUIElement, if you set this then the textbox will resize vertically
        /// </summary>
        public override float Width
        {
            get { return _calculateWidth ? CalculateWidthBasedOnText() : base.Width; }
            set
            {
                // calculateWidth is false, so that we are calculating the height instead
                _calculateWidth = false;
                base.Width = value;
                base.Height = CalculateHeightBasedOnText();
            }
        }

        /// <summary>
        /// The height of the DynamicTextboxUIElement, if you set this then the textbox will resize horizontally
        /// </summary>
        public override float Height
        {
            get { return _calculateWidth ? base.Height : CalculateHeightBasedOnText(); }
            set
            {
                // calculateWidth is true, so that we are calculating the width instead
                _calculateWidth = true;
                base.Height = value;
                base.Width = CalculateWidthBasedOnText();
            }
        }

        /// <summary>
        /// The text of the dynamic rectangle, refreshes the dimensions whenever this is changed
        /// </summary>
        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                RefreshDimensions();
            }
        }

        /// <summary>
        /// true if the user set the height and we want to resize the width
        /// dynamically, false if the user set the width and we want to set
        /// the height dynamically
        /// </summary>
        private bool _calculateWidth { get; set; }

        /// <summary>
        /// true if the DynamicTextbox has been loaded
        /// </summary>
        private bool _loaded;

        /// <summary>
        /// YOU MUST CALL THE LOAD METHOD TO GET THIS TEXTBOX TO WORK
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public DynamicTextboxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the width so that we default to resizing vertically
            Width = UIDefaults.Width;
        }

        public override Task Load()
        {
            _loaded = true;
            RefreshDimensions();
            return base.Load();
        }


        /// <summary>
        /// refreshes the dimensions of the DynamicTextbox, do this whenever the size of the textbox might have changed
        /// </summary>
        private void RefreshDimensions()
        {
            if (!_loaded)
            {
                return;
            }

            if (_calculateWidth)
            {
                base.Width = CalculateWidthBasedOnText();

            }
            else
            {
                base.Height = CalculateHeightBasedOnText();
            }
        }

        /// <summary>
        /// Refresh the dimensions when the base text format is changed
        /// </summary>
        protected override void UpdateCanvasTextFormat()
        {
            base.UpdateCanvasTextFormat();
            RefreshDimensions();
        }


        /// <summary>
        /// Gets the CanvasTextLayout based on whether we are calculating the width or the height for use in Calculate methods
        /// </summary>
        /// <param name="resourceCreator"></param>
        /// <returns></returns>
        private CanvasTextLayout CreateTextLayout(ICanvasResourceCreator resourceCreator)
        {
            if (!_loaded)
            {
                return null;
            }

            // if we are calculating the width, then make the width the maximum float value and use the height
            if (_calculateWidth)
            {
                return new CanvasTextLayout(resourceCreator, Text, CanvasTextFormat, float.MaxValue,
                    Height - 2 * (BorderWidth + UIDefaults.YTextPadding));
            }

            // if we are calculating the height, then make the height the maximum float value and use the width
            return new CanvasTextLayout(resourceCreator, Text, CanvasTextFormat,
                Width - 2 * (BorderWidth + UIDefaults.XTextPadding), float.MaxValue);
        }

        /// <summary>
        /// Calculates the needed width based on the text
        /// </summary>
        /// <returns></returns>
        private float CalculateWidthBasedOnText()
        {
            if (!_loaded)
            {
                return UIDefaults.Width;
            }

            return (float) CreateTextLayout(Canvas).LayoutBounds.Width + 2 * (BorderWidth + UIDefaults.XTextPadding);
        }

        /// <summary>
        /// Calculates the needed height based on the text
        /// </summary>
        /// <returns></returns>
        private float CalculateHeightBasedOnText()
        {
            if (!_loaded)
            {
                return UIDefaults.Height;
            }
            return (float) CreateTextLayout(Canvas).LayoutBounds.Height + 2*(BorderWidth + UIDefaults.YTextPadding);
        }

        /// <summary>
        /// Allows the user to bypass loading the dynamic textbox, make sure you know what you're doing here
        /// </summary>
        public void SetLoaded()
        {
            _loaded = true;
            RefreshDimensions();
        }
    }
}
