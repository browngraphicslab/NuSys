using System.Diagnostics;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    /// <summary>
    /// a draggable UI element - transparent background, with a label and an icon.
    /// </summary>
    public class TransparentButtonUIElement : ButtonUIElement
    {
        // ui variables
        private float _menuButtonWidth = 50;
        private float _menuButtonHeight = 50;
        private float _menuButtonSpacing = 10;

        public TransparentButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, int style = 0, string label = "")
            : base(parent, resourceCreator, new RectangleUIElement(parent, resourceCreator))
        {
            switch (style)
            {
                //primary style for ellipse
                case 0:
                    ButtonTextColor = Constants.DARK_BLUE;
                    break;
                //secondary style for ellipse
                case 1:
                    ButtonTextColor = Constants.RED;
                    break;
            }

            Height = _menuButtonHeight +10;
            Width = _menuButtonWidth;
            Background = Colors.Transparent;
            Bordercolor = Colors.Transparent;
            BorderWidth = 0;
            ImageBounds = new Rect(0, 0, _menuButtonWidth, _menuButtonHeight);
            ButtonText = label;

            SetOriginalValues();
        }

        /// <summary>
        /// returns a bounding box for the label that is the width of the button and is aligned at the bottom of the button
        /// </summary>
        /// <returns></returns>
        protected override Rect GetTextBoundingBox()
        {
            return new Rect(0, _menuButtonHeight, Width, Height);
        }

        /// <summary>
        /// returns a canvas text format that is centered with no trimming
        /// </summary>
        /// <returns></returns>
        protected override CanvasTextFormat GetCanvasTextFormat()
        {
            var textFormat = base.GetCanvasTextFormat();
            textFormat.WordWrapping = CanvasWordWrapping.WholeWord;
            textFormat.HorizontalAlignment = CanvasHorizontalAlignment.Center;
            textFormat.TrimmingGranularity = CanvasTextTrimmingGranularity.None;
            return textFormat;
        }

        public override void Resize(double e)
        {
            base.Resize(e);
            ImageBounds = new Rect(0, 0, _menuButtonWidth * e, _menuButtonHeight * e);
        }
    }
}