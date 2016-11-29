using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class TextboxUIElement : RectangleUIElement
    {

        /// <summary>
        /// The text to be displayed in the textbox
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The horizontal alignment of the text within the textbox
        /// </summary>
        public CanvasHorizontalAlignment TextHorizontalAlignment { get; set; }

        /// <summary>
        /// The vertical alignment of the text within the textbox
        /// </summary>
        public CanvasVerticalAlignment TextVerticalAlignment { get; set; }

        /// <summary>
        /// The color of the text within the text box
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        /// The style of the text within the text box, normal or italic. oblique is not bold. 
        /// </summary>
        public FontStyle FontStyle { get; set; }

        /// <summary>
        /// The size of the text in the textbox. 
        /// </summary>
        public float FontSize { get; set; }

        /// <summary>
        /// The font of the text in the textbox
        /// </summary>
        public string FontFamily { get; set; }

        /// <summary>
        /// The default break point at which text moves to a new line to avoid overflow
        /// </summary>
        public CanvasWordWrapping Wrapping { get; set; }

        /// <summary>
        /// The sign used to show that text has overflown the end of the text box
        /// </summary>
        public CanvasTrimmingSign TrimmingSign { get; set; }

        /// <summary>
        /// The granularity chosen to break off the end of text if text has overflown the end of the text box
        /// </summary>
        public CanvasTextTrimmingGranularity TrimmingGranularity { get; set; }

        public TextboxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set default values
            TextHorizontalAlignment = UIDefaults.TextHorizontalAlignment;
            TextVerticalAlignment = UIDefaults.TextVerticalAlignment;
            TextColor = UIDefaults.TextColor;
            FontStyle = UIDefaults.FontStyle;
            FontSize = UIDefaults.FontSize;
            FontFamily = UIDefaults.FontFamily;
            Wrapping = UIDefaults.Wrapping;
            TrimmingSign = UIDefaults.TrimmingSign;
            TrimmingGranularity = UIDefaults.TrimmingGranularity;
            BorderWidth = 0;
        }

        /// <summary>
        /// Draws the background and the border and the text
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            // return if the item is disposed or is not visible
            if (IsDisposed || !IsVisible)
                return;
            
            // draw the background and borders
            base.Draw(ds);

            // draw the text
            DrawText(ds);
        }

        /// <summary>
        /// Draws the text within the textbox
        /// </summary>
        /// <param name="ds"></param>
        public void DrawText(CanvasDrawingSession ds)
        {
            // save the current transform of the drawing session
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            if (Text != null)
            {
                // create a text format object
                var textFormat = new CanvasTextFormat
                {
                    HorizontalAlignment = TextHorizontalAlignment,
                    VerticalAlignment = TextVerticalAlignment,
                    WordWrapping = Wrapping,
                    TrimmingGranularity = TrimmingGranularity,
                    TrimmingSign = TrimmingSign,
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    FontStyle = FontStyle,
                };


                Debug.Assert(Width - 2*BorderWidth > 0 && Height - 2*BorderWidth > 0, "these must be greater than zero or drawText crashes below");

                // draw the text within the bounds (text auto fills the rect) with text color ButtonTextcolor, and the
                // just created textFormat
                ds.DrawText(Text,
                    new Rect(BorderWidth, BorderWidth, Width - 2 * BorderWidth, Height - 2 * BorderWidth),
                    TextColor, textFormat);
            }

            ds.Transform = orgTransform;
        }



    }
}
