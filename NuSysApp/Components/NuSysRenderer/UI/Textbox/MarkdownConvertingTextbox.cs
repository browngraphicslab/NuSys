﻿using System;
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
using Microsoft.Graphics.Canvas.Geometry;
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
        /// Vertical scrollbar used in conjunction with the markdown
        /// </summary>
        private ScrollBarUIElement _verticalScrollBar;

        /// <summary>
        /// The x offset of the upper left corner of the text not including borderwidth or UIDefaults.XTextpadding
        /// </summary>
        private float _xOffset;

        /// <summary>
        /// The y offset of the upper left corner of the text not including borderwidth or UIDefaults.YTextpadding
        /// </summary>
        private float _yOffset;

        /// <summary>
        /// The initial x offset while dragging
        /// </summary>
        private float _initialDragXOffset;

        /// <summary>
        /// The initial y offset while dragging
        /// </summary>
        private float _initialDragYOffset;

        /// <summary>
        /// Position is a float from 0 to 1 representing the start of the scroll bar, fired whenever the scrollbar position changes
        /// </summary>
        public event ScrollBarUIElement.ScrollBarPositionChangedHandler ScrollBarPositionChanged;

        /// <summary>
        /// public bool to hide or show the scrollbar and allow or disallow scrolling
        /// </summary>
        public bool Scrollable
        {
            get { return _verticalScrollBar.IsVisible; }
            set { _verticalScrollBar.IsVisible = value; }
        }

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

            // add the vertical scroll bar to the markdown converting textbox
            _verticalScrollBar = new ScrollBarUIElement(this, resourceCreator, ScrollBarUIElement.Orientation.Vertical);
            _verticalScrollBar.Width = 15;
            AddChild(_verticalScrollBar);

            _verticalScrollBar.ScrollBarPositionChanged += _verticalScrollBar_ScrollBarPositionChanged;
            DragStarted += MarkdownConvertingTextbox_DragStarted;
            Dragged += MarkdownConvertingTextbox_Dragged;
            PointerWheelChanged += MarkdownConvertingTextbox_PointerWheelChanged;
        }

        private void MarkdownConvertingTextbox_PointerWheelChanged(InteractiveBaseRenderItem item, CanvasPointer pointer, float delta)
        {
            if (!Scrollable)
            {
                return;
            }
            _yOffset -= (float)(_canvasTextLayout.LayoutBoundsIncludingTrailingWhitespace.Height * (delta > 0 ? -.05 : .05));
            BoundYOffset();
        }

        private void MarkdownConvertingTextbox_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (!Scrollable)
            {
                return;
            }
            _yOffset = _initialDragYOffset + pointer.Delta.Y;
            BoundYOffset();
        }

        private void MarkdownConvertingTextbox_DragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _initialDragXOffset = _xOffset;
            _initialDragYOffset = _yOffset;
        }

        /// <summary>
        /// Bound the y offset
        /// </summary>
        public void BoundYOffset()
        {
            if (!_resourcesCreated)
            {
                return;
            }

            _yOffset = Math.Min(0, _yOffset);

            _yOffset = (float) Math.Max(-(_canvasTextLayout.LayoutBoundsIncludingTrailingWhitespace.Height - Height + 2 * (UIDefaults.YTextPadding + BorderWidth)), _yOffset);

            // shift the text so it fills the textbox if it can
            if (Math.Abs(_verticalScrollBar.Range - 1) < .001)
            {
                _yOffset = 0;
            }
        }

        /// <summary>
        /// Event fired whenever the markdown converting textbox's vertical scroll bar position changes
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position"></param>
        private void _verticalScrollBar_ScrollBarPositionChanged(object source, float position)
        {
            if (!Scrollable)
            {
                return;
            }
            _yOffset = (float) (-position * _canvasTextLayout.LayoutBoundsIncludingTrailingWhitespace.Height);
            BoundYOffset();
            ScrollBarPositionChanged?.Invoke(this, position);
        }

        public override void Dispose()
        {
            _verticalScrollBar.ScrollBarPositionChanged -= _verticalScrollBar_ScrollBarPositionChanged;
            DragStarted -= MarkdownConvertingTextbox_DragStarted;
            Dragged -= MarkdownConvertingTextbox_Dragged;
            PointerWheelChanged -= MarkdownConvertingTextbox_PointerWheelChanged;

            base.Dispose();
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

            if (_resourcesCreated)
            {
                _verticalScrollBar.Height = Height - 2 * BorderWidth;
                _verticalScrollBar.Transform.LocalPosition = new Vector2(Width - BorderWidth - _verticalScrollBar.Width,
                    BorderWidth);

                // set the position and rang eof the vertical scroll bar
                SetVerticalScrollBarPositionAndRange();

                // shift the text so it fills the textbox if it can
                if (Math.Abs(_verticalScrollBar.Range - 1) < .001)
                {
                    _yOffset = 0;
                }
            }

            base.Update(parentLocalToScreenTransform);
        }

        private void SetVerticalScrollBarPositionAndRange()
        {
            if (!_resourcesCreated)
            {
                return;
            }
            var _vertScrollPrevPosition = _verticalScrollBar.Position;

            _verticalScrollBar.Position = (float)(-_yOffset / _canvasTextLayout.LayoutBoundsIncludingTrailingWhitespace.Height);
            _verticalScrollBar.Range =
                (float)
                    ((Height - 2 * (BorderWidth + UIDefaults.YTextPadding)) /
                     _canvasTextLayout.LayoutBoundsIncludingTrailingWhitespace.Height);

            BoundVerticalScrollBarPosition();



            if (Math.Abs(_vertScrollPrevPosition - _verticalScrollBar.Position) > .005)
            {
                ScrollBarPositionChanged?.Invoke(this, _verticalScrollBar.Position);
            }
        }

        public void SetVerticalScrollBarPosition(float newPosition)
        {
            _yOffset = (float)(-newPosition * _canvasTextLayout.LayoutBoundsIncludingTrailingWhitespace.Height);
            BoundYOffset();
        }


        /// <summary>
        /// Call this to bound the vertical scroll bar
        /// </summary>
        private void BoundVerticalScrollBarPosition()
        {
            // bound the vertical scroll bar postiion
            if (_verticalScrollBar.Position + _verticalScrollBar.Range > 1)
            {
                _verticalScrollBar.Position = 1 - _verticalScrollBar.Range;
            }
            if (_verticalScrollBar.Position < 0)
            {
                _verticalScrollBar.Position = 0;
            }
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
                using (
                    ds.CreateLayer(1,
                        CanvasGeometry.CreateRectangle(Canvas, BorderWidth + UIDefaults.XTextPadding,
                            BorderWidth + UIDefaults.YTextPadding,
                            Width - 2*(BorderWidth + UIDefaults.XTextPadding),
                            Height - 2*(BorderWidth + UIDefaults.YTextPadding))))
                {
                    // draw the text within the proper bounds
                    try
                    {
                        ds.DrawTextLayout(_canvasTextLayout, _xOffset + BorderWidth + UIDefaults.XTextPadding,
                            _yOffset + BorderWidth + UIDefaults.YTextPadding, TextColor);
                    }
                    catch (Exception e)
                    {
                        //TODO fix this 
                    }
                }
            }

            ds.Transform = orgTransform;
        }

        private void UpdateCanvasLayout()
        {
            _canvasTextLayout = _htmlParser.GetParsedText(_textHtml, Width - 2 * (BorderWidth + UIDefaults.XTextPadding), double.MaxValue);
        }


    }



}