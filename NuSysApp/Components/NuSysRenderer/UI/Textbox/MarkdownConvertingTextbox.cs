using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Windows.UI;
using CommonMark;
using CommonMark.Syntax;
using NuSysApp.Components.NuSysRenderer.UI.Textbox.Markdown;

namespace NuSysApp
{
    /// <summary>
    /// class for a rich text element. this can do things that the regular textbox ui element can't - underlines, for example.
    /// since it shares a lot of properties in common with the text box ui element i am extending it for now, then just using the textbox CanvasTextFormat
    /// to create the rich text CanvasTextLayout.
    /// 
    /// TODO: add font weight, style, family, etc. properties to this so people can set font__ on a specific group of characters in text
    /// </summary>
    public class MarkdownConvertingTextbox : TextboxUIElement
    {
        /// <summary>
        /// the canvas text layout, the rich text equivalent of a canvas text format.
        /// this will use the textboxuielement's canvastextformat.
        /// </summary>
        private CanvasTextLayout _canvasTextLayout;

        /// <summary>
        /// true if the resources have been created fro the markdown converting textbox
        /// </summary>
        private bool _resourcesCreated;

        /// <summary>
        /// html parser used to parse the converted markdown html into a CanvasTextLayout
        /// </summary>
        private HTMLParser _htmlParser;

        /// <summary>
        /// YOU MUST CALL LOAD ON THE MARKDWON CONVERTING TEXTBOX
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public MarkdownConvertingTextbox(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the default HTML formatter to use the custom one we are building
            CommonMarkSettings.Default.OutputDelegate =
                (doc, output, settings) =>
                new CustomHtmlFormatter(output, settings).WriteDocument(doc);

            // enable the strike through tilde by default
            CommonMarkSettings.Default.AdditionalFeatures = CommonMarkAdditionalFeatures.StrikethroughTilde;
        }

        public override Task Load()
        {
            CreateTextResources();
            return base.Load();
        }

        /// <summary>
        /// Create resources here that rely on the textbox being loaded
        /// </summary>
        protected virtual void CreateTextResources()
        {
            // set the default CanvasTextformat
            base.UpdateCanvasTextFormat();
            TextVerticalAlignment = CanvasVerticalAlignment.Top;

            _htmlParser = new HTMLParser(ResourceCreator, CanvasTextFormat);

            // update the canvas text layout
            CreateCanvasTextLayout();

            // yay all the resources have been created
            _resourcesCreated = true;
        }

        /// <summary>
        /// Update the canvas text layout
        /// </summary>
        private void CreateCanvasTextLayout()
        {
            // set the canvas text layout to a new canvas text layout
            _canvasTextLayout = new CanvasTextLayout(Canvas, Text, CanvasTextFormat, Width - 2 * (BorderWidth + UIDefaults.XTextPadding), Height - 2 * (BorderWidth + UIDefaults.YTextPadding));

        }
            
        /// <summary>
        /// Updates the canvas text format
        /// </summary>
        protected override void UpdateCanvasTextFormat()
        {
            if (!_resourcesCreated)
            {
                return;
            }

            // create the new canvas text format
            CanvasTextFormat = new CanvasTextFormat()
            {
                HorizontalAlignment = TextHorizontalAlignment,
                VerticalAlignment = TextVerticalAlignment,
                WordWrapping = Wrapping,
                TrimmingGranularity = TrimmingGranularity,
                TrimmingSign = TrimmingSign,
            };

            // update the canvas text format in the html parser
            _htmlParser.UpdateCanvasTextFormat(CanvasTextFormat);
        }

        /// <summary>
        /// underlines a substring of the string
        /// </summary>
        /// <param name="start"></param>
        /// <param name="charCount"></param>
        public void UnderlineFragment(int start, int charCount)
        {
            if(_resourcesCreated)
                _canvasTextLayout.SetUnderline(start, charCount, true);
        }

        /// <summary>
        /// set font family for substring
        /// </summary>
        /// <param name="start"></param>
        /// <param name="charCount"></param>
        /// <param name="fontFamily"></param>
        public void SetFontFragment(int start, int charCount, string fontFamily)
        {
            if(_resourcesCreated)
                _canvasTextLayout.SetFontFamily(start, charCount, fontFamily);
        }

        /// <summary>
        /// set font style for substring
        /// </summary>
        /// <param name="start"></param>
        /// <param name="charCount"></param>
        /// <param name="fontStyle"></param>
        public void SetFontStyle(int start, int charCount, Windows.UI.Text.FontStyle fontStyle)
        {
            if(_resourcesCreated)
                _canvasTextLayout.SetFontStyle(start, charCount, fontStyle);
        }

        /// <summary>
        /// set font size for substring
        /// </summary>
        /// <param name="start"></param>
        /// <param name="charCount"></param>
        /// <param name="size"></param>
        public void SetFontSize(int start, int charCount, float size)
        {
            if (_resourcesCreated)
                _canvasTextLayout.SetFontSize(start, charCount, size);
        }
             
        /// <summary>
        /// overrides draw call of textboxuielement to call ds.drawlayout instead of ds.drawtext
        /// </summary>
        /// <param name="ds"></param>
        public override void DrawText(CanvasDrawingSession ds)
        {
            if (!_resourcesCreated)
            {
                return;
            }

            // save the current transform of the drawing session
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            if (Text != null)
            {
                Debug.Assert(Width - 2 * (BorderWidth + UIDefaults.XTextPadding) > 0 && Height - 2 * (BorderWidth + UIDefaults.YTextPadding) > 0, "these must be greater than zero or drawText crashes below");


                // update the font size based on the accessibility settings
                //CanvasTextFormat.FontSize = FontSize * (float)SessionController.Instance.SessionSettings.TextScale;


                // draw the text within the proper bounds
                var xOffset = BorderWidth + UIDefaults.XTextPadding;
                var yOffset = BorderWidth + UIDefaults.YTextPadding;

                ds.DrawTextLayout(_canvasTextLayout, xOffset, yOffset, TextColor);
                //ds.DrawText(Text, new Rect(x, y, width, height), TextColor, CanvasTextFormat);
            }

            ds.Transform = orgTransform;
        }

        public void UpdateMarkdown(string markDownText)
        {
            var html = CommonMarkConverter.Convert(markDownText);


            _canvasTextLayout = _htmlParser.GetParsedText(html, Width - 2*(BorderWidth + UIDefaults.XTextPadding),
                Height - 2*(BorderWidth + UIDefaults.YTextPadding));
        }



    }



}
