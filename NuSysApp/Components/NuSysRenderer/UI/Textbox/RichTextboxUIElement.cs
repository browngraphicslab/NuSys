using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    /// <summary>
    /// class for a rich text element. this can do things that the regular textbox ui element can't - underlines, for example.
    /// since it shares a lot of properties in common with the text box ui element i am extending it for now, then just using the textbox CanvasTextFormat
    /// to create the rich text CanvasTextLayout.
    /// this might not be the right thing to do. ^^ double check this choice later.
    /// 
    /// NOTE: AS OF 1/9/17 THIS CLASS IS NOT TESTED
    /// </summary>
    public class RichTextboxUIElement : TextboxUIElement
    {
        /// <summary>
        /// the canvas text layout, the rich text equivalent of a canvas text format.
        /// this will use the textboxuielement's canvastextformat.
        /// </summary>
        private CanvasTextLayout _canvasTextLayout;
        public CanvasTextLayout CanvasTextLayout
        {
            get { return _canvasTextLayout; }
            set { _canvasTextLayout = value; }   
        }

        /// <summary>
        /// a boolean for whether the text is underlined.
        /// </summary>
        private bool _underlined;
        public bool Underlined
        {
            get { return _underlined; }
            set
            {
                _underlined = value;
                UpdateCanvasTextLayout();
            }
        }

        /// <summary>
        /// constructor for rich text box ui element
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public RichTextboxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            UpdateCanvasTextLayout();
        }

        /// <summary>
        /// updates the canvastextlayout object
        /// </summary>
        private void UpdateCanvasTextLayout()
        {
            CanvasTextLayout = new CanvasTextLayout(Canvas, Text, CanvasTextFormat, Width, Height);
            CanvasTextLayout.SetUnderline(0, Text.Length, Underlined);
        }

        /// <summary>
        /// overrides draw call of textboxuielement to call ds.drawlayout instead of ds.drawtext
        /// </summary>
        /// <param name="ds"></param>
        public override void DrawText(CanvasDrawingSession ds)
        {
            // save the current transform of the drawing session
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            if (Text != null)
            {
                Debug.Assert(Width - 2 * (BorderWidth + UIDefaults.XTextPadding) > 0 && Height - 2 * (BorderWidth + UIDefaults.YTextPadding) > 0, "these must be greater than zero or drawText crashes below");


                // update the font size based on the accessibility settings
                CanvasTextFormat.FontSize = FontSize * (float)SessionController.Instance.SessionSettings.TextScale;
                UpdateCanvasTextLayout();

                // draw the text within the proper bounds
                var x = BorderWidth + UIDefaults.XTextPadding;
                var y = BorderWidth + UIDefaults.YTextPadding;
                var width = Width - 2 * (BorderWidth + UIDefaults.XTextPadding);
                var height = Height - 2 * (BorderWidth + UIDefaults.YTextPadding);
                ds.DrawTextLayout(CanvasTextLayout, x, y, TextColor);
                //ds.DrawText(Text, new Rect(x, y, width, height), TextColor, CanvasTextFormat);
            }

            ds.Transform = orgTransform;
        }
    }
}
