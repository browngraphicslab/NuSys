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
        /// private string to hold the current markdown's html value.
        /// </summary>
        private string _textHtml = "";

        /// <summary>
        /// private IsDirty boolean for the text layout object
        /// </summary>
        private bool _textLayoutIsDirty = false;

        /// <summary>
        /// Overriding the text from base class.
        /// The setter now updates the internal html
        /// </summary>
        public string Text
        {
            get { return base.Text; }
            set
            {
                _textHtml = CommonMarkConverter.Convert(value);
                _textLayoutIsDirty = true;
                base.Text = value;
            }
        }

        /// <summary>
        /// Overriding the width just sets the base value and requires the text layout to update
        /// </summary>
        public float Width
        {
            get { return base.Width; }
            set
            {
                base.Width = value;
                _textLayoutIsDirty = true;
            }
        }

        /// <summary>
        /// overidden height also sets the test layout is dirty bool to true.
        /// Other than that it just sets and gets the base value
        /// </summary>
        public float Height
        {
            get { return base.Height; }
            set
            {
                base.Height = value;
                _textLayoutIsDirty = true;
            }
        }

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
            if (!string.IsNullOrEmpty(base.Text))
            {
                Text = base.Text;
            }
        }

        /// <summary>
        /// Only use this if you want to bypass the Markdown-to-HTML parser.
        /// The passed in text must be valid html.  
        /// Dont use this method unless you know about the internals of this class
        /// </summary>
        /// <param name="html"></param>
        public void SetHtmlDirectly(string html)
        {
            _textHtml = html;
            _textLayoutIsDirty = true;
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
        /// This upadate override simply takes into account the _textLayoutIsDirty bool.
        /// If its true, it updates the private layout object
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_textLayoutIsDirty)
            {
                UpdateCanvasLayout();
                _textLayoutIsDirty = false;
            }
            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// overrides draw call of textboxuielement to call ds.drawlayout instead of ds.drawtext
        /// </summary>
        /// <param name="ds"></param>
        protected override void DrawText(CanvasDrawingSession ds)
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

        private void UpdateCanvasLayout()
        {
            _canvasTextLayout = _htmlParser.GetParsedText(_textHtml, Width - 2 * (BorderWidth + UIDefaults.XTextPadding),
    Height - 2 * (BorderWidth + UIDefaults.YTextPadding));
        }


    }



}
