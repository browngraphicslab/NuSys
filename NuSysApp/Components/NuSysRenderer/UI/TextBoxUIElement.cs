using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class TextBoxUIElement : RectangleUIElement
    {
        /// <summary>
        /// Object that controls font and layout options for drawing text
        /// </summary>
        private CanvasTextFormat _format;

        /// <summary>
        /// A drawabble piece of formatted text
        /// </summary>
        private CanvasTextLayout _textLayout;

        /// <summary>
        /// The text in the textbox
        /// </summary>
        public string TextBoxText { get; set; }

        /// <summary>
        /// The Horizontal Alignment of the text
        /// </summary>
        public CanvasHorizontalAlignment HorizontalTextAlignment { get; set; }

        /// <summary>
        /// The Vertical Alignment of the text
        /// </summary>
        public CanvasVerticalAlignment VerticalTextAlignment { get; set; }

        /// <summary>
        /// The distance in pixels between the border and the start of the texts
        /// </summary>
        public float Margin { get; set; }

        /// <summary>
        ///  The index of the first character in selection
        /// </summary>
        public int SelectionStartIndex { get; private set; }

        /// <summary>
        /// The index of the last character in selection
        /// </summary>
        public int SelectionEndIndex { get; private set; }

        /// <summary>
        /// True if the TextboxUIElement currently has text selected
        /// </summary>
        public bool HasSelection { get; private set; }

        /// <summary>
        /// The Color of Selected Text
        /// </summary>
        public Color SelectionColor { get; set; }

        /// <summary>
        /// The Highlight of Selected Text
        /// </summary>
        public Color SelectionHighlight { get; set; }

        /// <summary>
        /// Contains the bounds the text should exist in. The upper left x is contained in x.
        /// The upper left y is contained in y. The lower right x is contained in z. The lower right
        /// y is contained in w.
        /// </summary>
        private Vector4 _textLayoutBounds => new Vector4(
            BorderWidth + Margin, // accounts for borderwidth and margin padding
            BorderWidth + Margin, 
            Width - Margin - BorderWidth,
            Height - Margin - BorderWidth);

        /// <summary>
        /// The width of the text layout. Bottom right x minus top left x
        /// </summary>
        private float _textLayoutWidth => _textLayoutBounds.Z - _textLayoutBounds.X;

        /// <summary>
        /// The height of the text layout. Bottom right y minus top left y
        /// </summary>
        private float _textLayoutHeight => _textLayoutBounds.W - _textLayoutBounds.Y;

        /// <summary>
        /// The color of the text
        /// </summary>
        public Color TextColor { get; set; }

        public TextBoxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            

        }

        /// <summary>
        /// The Load event. Add event handlers here.
        /// </summary>
        /// <returns></returns>
        public override Task Load()
        {
            Pressed += TextBoxUIElement_Pressed;
            Dragged += TextBoxUIElement_Dragged;
            return base.Load();
        }

        /// <summary>
        /// Fired when a pointer is dragged on the TextBoxUIElement
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void TextBoxUIElement_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SelectionEndIndex = GetHitIndex(pointer);
        }

        /// <summary>
        /// Fired when a pointer is pressed on the TextBoxUIElement
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void TextBoxUIElement_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SelectionStartIndex = GetHitIndex(pointer);
            SelectionEndIndex = SelectionStartIndex;
        }

        /// <summary>
        /// The dispose event. Remove Event handlers here.
        /// </summary>
        public override void Dispose()
        {
            Pressed -= TextBoxUIElement_Pressed;
            Dragged -= TextBoxUIElement_Dragged;
            base.Dispose();
        }

        public override void Draw(CanvasDrawingSession ds)
        {

            base.Draw(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            // create the TextLayout before every draw call //todo find a way to cache this its not very smart to do it here
            CreateTextLayout(Canvas, _textLayoutWidth, _textLayoutHeight);


            if (HasSelection)
            {
                int firstIndex = Math.Min(SelectionStartIndex, SelectionEndIndex);
                int length = Math.Abs(SelectionEndIndex - SelectionStartIndex) + 1;
                CanvasTextLayoutRegion[] descriptions = _textLayout.GetCharacterRegions(firstIndex, length);
                foreach (CanvasTextLayoutRegion description in descriptions)
                {
                    ds.FillRectangle(InflateRect(description.LayoutBounds), new CanvasSolidColorBrush(Canvas, SelectionHighlight));
                }
                _textLayout.SetBrush(firstIndex, length, new CanvasSolidColorBrush(Canvas, SelectionColor));
            }

            ds.DrawTextLayout(_textLayout, Margin + BorderWidth, Margin + BorderWidth, TextColor );

            ds.Transform = orgTransform;

        }


        /// <summary>
        /// Called to create the text layouts
        /// </summary>
        /// <param name="resourceCreator"></param>
        /// <param name="canvasWidth"></param>
        /// <param name="canvasHeight"></param>
        /// <returns></returns>
        private CanvasTextLayout CreateTextLayout(ICanvasResourceCreator resourceCreator, float textLayoutWidth,
            float textLayoutHeight)
        {
            // create a new CanvasTextFormat
            _format = new CanvasTextFormat();

            // set the vertical and horizontal alignment of the text
            _format.HorizontalAlignment = HorizontalTextAlignment;
            _format.VerticalAlignment = VerticalTextAlignment;
            // set the Trim to trim on words so trimming occurs on words and words don't split on the line
            _format.TrimmingGranularity = CanvasTextTrimmingGranularity.Word;

            //todo set the font family
            //_format.FontFamily = 

            // create the text layout, the width is the bottom right x  minus the top left x, and the height is bottom right y
            // minus top left y
            _textLayout = new CanvasTextLayout(Canvas, TextBoxText, _format, textLayoutWidth, textLayoutHeight);

            return _textLayout;
        }

        /// <summary>
        /// Returns the index of the character the pointer is currently over
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        private int GetHitIndex(CanvasPointer pointer)
        {
            CanvasTextLayoutRegion textLayoutRegion;

            // convert the point from screen to local coordinates
            var currentPosition = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);

            // add the offset to shift the pointer's position relative to the _textLayoutBounds
            currentPosition.X += _textLayoutBounds.X;
            currentPosition.Y += _textLayoutBounds.Y;

            HasSelection = _textLayout.HitTest(
                currentPosition.X,
                currentPosition.Y,
                out textLayoutRegion);
            return textLayoutRegion.CharacterIndex;
        }

        /// <summary>
        /// Clears the currently selected text
        /// </summary>
        private void ClearSelection()
        {
            HasSelection = false;
            SelectionStartIndex = 0;
            SelectionEndIndex = 0;
        }

        Rect InflateRect(Rect r)
        {
            return new Rect(
                new Point(Math.Floor(r.Left), Math.Floor(r.Top)),
                new Point(Math.Ceiling(r.Right), Math.Ceiling(r.Bottom)));
        }
    }
}
