using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI;


namespace NuSysApp
{
    public class ScrollableTextboxUIElement : TextboxUIElement
    {
        // Delegate for the TextChanged event: Takes in the new string of text
        public delegate void TextHandler(InteractiveBaseRenderItem item, String text);

        // Text events
        public event TextHandler TextChanged;
        public event TextHandler TextCopied;
        public event TextHandler TextCut;
        public event TextHandler TextPasted;

        // Keeps track of whether the user has highlighted
        // any text
        public  bool _hasSelection;
        // Boolean for when this textbox has focus
        private bool _hasFocus;
        // Boolean for when the control key is held down in order to handle
        // cut/copy/paste events
        private bool _isCtrlPressed = false;

        // The current text layout
        public CanvasTextLayout TextLayout { get; set; }

        // Text format currently being used
        private CanvasTextFormat _textFormat;

        // Direction this textbox scrolls
        private bool _scrollVert;

        // The rectangle representing the cursor
        public RectangleUIElement _cursor;
        // Holds the index in the text string that the cursor is
        // currently located at
        public int CursorCharacterIndex { get; set; }
        // Preserves the x position when moving the cursor up and down
        // in the text box
        private float _currCursorX = 0;

        // Beginning and ending indices of the current highlighted text
        public int _selectionStartIndex = 0;
        public int _selectionEndIndex = 0;

        // Incremented at every update call and used to control how often
        // the cursor blinks
        private int _blinkCounter = 0;

        // Rectangle that represents the box where the text can be drawn in
        private Rect _viewBox;

        // Directional offsets of where the top left corner of the text is
        private double _xOffset;
        private double _yOffset;

        // Min and max indices in the text to be drawn on the screen
        private int _maxIndex;
        private int _minIndex;

        /// <summary>
        /// The currently shown text
        /// </summary>
        private String _activeText;

        /// <summary>
        /// Models a text box which the user can type into and edit
        /// Inherits from TextboxUIElement
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public ScrollableTextboxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, 
            bool scrollVert, bool showScrollBar) : base(parent, resourceCreator)
        {
            _scrollVert = scrollVert;
            _hasSelection = false;
            TextVerticalAlignment = CanvasVerticalAlignment.Top;
            // Don't wrap text if it is a horizontally scrolling textbox
            Wrapping = scrollVert ? CanvasWordWrapping.WholeWord : CanvasWordWrapping.NoWrap;
            TrimmingSign = CanvasTrimmingSign.None;

            _textFormat = new CanvasTextFormat
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

            _xOffset = 0;
            _yOffset = 0;

            _activeText = Text;

            TextLayout = new CanvasTextLayout(resourceCreator, Text, _textFormat,
                Width - 2 * (BorderWidth + UIDefaults.XTextPadding),
                Height - 2 * (BorderWidth + UIDefaults.YTextPadding));

            _viewBox = new Rect(BorderWidth, BorderWidth, Width - 2 * (BorderWidth), Height - 2 * (BorderWidth));


            // Get the size the cursor should be
            CursorCharacterIndex = -1;
            CanvasTextLayoutRegion textLayoutRegion;
            TextLayout.GetCaretPosition(0, false, out textLayoutRegion);          
            Rect bounds = textLayoutRegion.LayoutBounds;

            // Initialize cursor
            _cursor = new RectangleUIElement(this, resourceCreator);
            _cursor.Width = 2;
            _cursor.Height = (float)bounds.Height;
            _cursor.Background = Colors.Black;
            _cursor.IsVisible = false;

            // Add cursor as child of the textbox
            this.AddChild(_cursor);

            this.Pressed += EditableTextboxUIElement_Pressed;
            this.Dragged += EditableTextboxUIElement_Dragged;

            this.OnFocusGained += EditableTextboxUIElement_OnFocusGained;
            this.OnFocusLost += EditableTextboxUIElement_OnFocusLost;
            this.KeyPressed += EditableTextboxUIElement_KeyPressed;
            this.KeyReleased += EditableTextboxUIElement_KeyReleased;
            this.TextChanged += EditableTextboxUIElement_TextChanged;

        }

        /// <summary>
        /// Fired whenever the text is changed in this textbox. Updates the current text layout
        /// </summary>
        /// <param name="item"></param>
        /// <param name="text"></param>
        private void EditableTextboxUIElement_TextChanged(InteractiveBaseRenderItem item, string text)
        {
            TextLayout = CreateTextLayout(item.ResourceCreator);
        }

        /// <summary>
        /// Fired when key is released while this element has focus
        /// </summary>
        /// <param name="args"></param>
        private void EditableTextboxUIElement_KeyReleased(Windows.UI.Core.KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Control)
            {
                _isCtrlPressed = false;
            }
        }

        /// <summary>
        /// Fired when mouse is dragged while this element has focus. Used to highlight text
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void EditableTextboxUIElement_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            //TODO Make this work for vertically scrolling textboxes

            Vector2 pos = new Vector2(pointer.CurrentPoint.X - UIDefaults.XTextPadding - (float)this.Transform.LocalPosition.X,
                                      pointer.CurrentPoint.Y - UIDefaults.YTextPadding - (float)this.Transform.LocalPosition.Y);
            _selectionEndIndex = GetHitIndex(pos);

            // Update selection to be in bounds so the textbox scrolls with the selection
            if (_selectionEndIndex > _maxIndex)
            {
                String over = Text.Substring(_maxIndex, _selectionEndIndex - _maxIndex);

                var textLayout = new CanvasTextLayout(item.ResourceCreator, over, _textFormat,
                    TextLayout.GetMinimumLineLength(),
                    Height - 2 * (BorderWidth + UIDefaults.YTextPadding));

                Rect bounds = textLayout.LayoutBounds;

                _xOffset -= bounds.Width;
                RefreshActiveText();
            }
            else if (_selectionEndIndex < _minIndex)
            {
                String under = Text.Substring(_selectionEndIndex, _minIndex - _selectionEndIndex);

                var textLayout = new CanvasTextLayout(item.ResourceCreator, under, _textFormat,
                    TextLayout.GetMinimumLineLength(),
                    Height - 2 * (BorderWidth + UIDefaults.YTextPadding));

                Rect bounds = textLayout.LayoutBounds;

                _xOffset += bounds.Width;
                RefreshActiveText();
            }
            _hasSelection = true;
        }

        /// <summary>
        /// Fired when text box is pressed on - used to move the cursor to the location of the press
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void EditableTextboxUIElement_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ClearSelection();
            Vector2 pos = new Vector2(pointer.CurrentPoint.X - UIDefaults.XTextPadding - (float)this.Transform.LocalPosition.X, 
                                      pointer.CurrentPoint.Y - UIDefaults.YTextPadding - (float)this.Transform.LocalPosition.Y);
            int charIndex = GetHitIndex(pos);
            CursorCharacterIndex = charIndex;
            if (Text == "")
            {
                CursorCharacterIndex = -1;
            }

            _selectionStartIndex = charIndex;
        }

        /// <summary>
        /// Fired when key is pressed on the textbox while it has focus
        /// </summary>
        /// <param name="args"></param>
        private void EditableTextboxUIElement_KeyPressed(Windows.UI.Core.KeyEventArgs args)
        {
            _cursor.IsVisible = true;

            //Backspace Key
            if (args.VirtualKey == VirtualKey.Back)
            {
                _currCursorX = 0;              
                if (CursorCharacterIndex > 0)
                {
                    Text = Text.Remove(CursorCharacterIndex, 1);
                    OnTextChanged(Text);
                    CursorCharacterIndex--;
                }
            }
            // Delete Key
            else if (args.VirtualKey == VirtualKey.Delete)
            {
                _currCursorX = 0;
                if (CursorCharacterIndex < (Text.Length-1))
                {
                    Text = Text.Remove(CursorCharacterIndex+1, 1);
                    OnTextChanged(Text);
                }
            }
            // Move cursor left
            else if (args.VirtualKey == VirtualKey.Left)
            {
                _currCursorX = 0;
                if ((Text.Length == 1) && CursorCharacterIndex < 2)
                {
                    CursorCharacterIndex = -1;
                }
                else if (CursorCharacterIndex >= 0)
                {
                    CursorCharacterIndex--;
                }
            }
            // Move cursor right
            else if (args.VirtualKey == VirtualKey.Right)
            {
                _currCursorX = 0;
                if (CursorCharacterIndex < (Text.Length - 1))
                {
                    CursorCharacterIndex++;
                }
            }
            // Move cursor up
            // TODO: Deal with scrolling vertically here
            else if (args.VirtualKey == VirtualKey.Up)
            {
                CanvasTextLayoutRegion textLayoutRegion;
                TextLayout.GetCaretPosition(CursorCharacterIndex, false, out textLayoutRegion);

                Rect bounds = textLayoutRegion.LayoutBounds;
                Vector2 pos = new Vector2((float)(bounds.Left + bounds.Width / 2),
                                          (float)(bounds.Top - _cursor.Height / 2));
                if (_currCursorX != 0)
                {
                    pos.X = _currCursorX;
                }
                int charIndex = GetHitIndex(pos);
                CursorCharacterIndex = charIndex;
                _currCursorX = pos.X;
            }
            // Move cursor down
            else if (args.VirtualKey == VirtualKey.Down)
            {
                CanvasTextLayoutRegion textLayoutRegion;
                TextLayout.GetCaretPosition(CursorCharacterIndex, false, out textLayoutRegion);

                Rect bounds = textLayoutRegion.LayoutBounds;
                Vector2 pos = new Vector2((float)(bounds.Left + bounds.Width / 2),
                                          (float)(bounds.Bottom + _cursor.Height / 2));
                if (_currCursorX != 0)
                {
                    pos.X = _currCursorX;
                }
                int charIndex = GetHitIndex(pos);
                CursorCharacterIndex = charIndex;
                _currCursorX = pos.X;
            }
            // Control button pressed
            else if (args.VirtualKey == VirtualKey.Control)
            {
                _isCtrlPressed = true;
            }
            // Special case z,x,c keys while control is pressed
            else if (args.VirtualKey == VirtualKey.C && _isCtrlPressed)
            {
                Copy();
            }
            else if (args.VirtualKey == VirtualKey.X && _isCtrlPressed)
            {
                Cut();
            }
            else if (args.VirtualKey == VirtualKey.V && _isCtrlPressed)
            {
                Paste();
            }
            else if (args.VirtualKey == VirtualKey.Tab)
            {
                Text = Text.Insert(CursorCharacterIndex + 1, "    ");
                CursorCharacterIndex += 4;
            }
            // Type the letter into the box
            else
            {

                String s = KeyCodeToUnicode(args.VirtualKey);
                if (s.Length > 0)
                {
                    _currCursorX = 0;
                    Text = Text.Insert(CursorCharacterIndex + 1, s);
                    CursorCharacterIndex++;
                    OnTextChanged(Text);

                }
            }

            CheckTextInBounds();
        }

        /// <summary>
        /// Triggered when this textbox loses focus
        /// </summary>
        /// <param name="item"></param>
        private void EditableTextboxUIElement_OnFocusLost(BaseRenderItem item)
        {
            // hide blinking cursor
            BorderWidth = 0;
            _hasFocus = false;
            _cursor.IsVisible = false;
        }

        /// <summary>
        /// Triggered when this textbox gains focus
        /// </summary>
        /// <param name="item"></param>
        private void EditableTextboxUIElement_OnFocusGained(BaseRenderItem item)
        {
            // show blinking cursor at end of text
            BorderWidth = 2;
            _hasFocus = true;
            _cursor.IsVisible = true;
        }

        /// <summary>
        /// Draw the cursor at its appropriate position
        /// </summary>
        /// <param name="ds"></param>
        private void DrawCursor(CanvasDrawingSession ds)
        {
            CanvasTextLayoutRegion textLayoutRegion;
            if (CursorCharacterIndex > -1)
            {
                TextLayout.GetCaretPosition(CursorCharacterIndex, false, out textLayoutRegion);

                Rect bounds = textLayoutRegion.LayoutBounds;
                bounds.X += _xOffset;
                bounds.Y += _yOffset;

                _cursor.Transform.LocalPosition = new Vector2((float)bounds.Right + UIDefaults.XTextPadding, (
                    float)bounds.Top + UIDefaults.YTextPadding);
            } else
            {
                TextLayout.GetCaretPosition(0, false, out textLayoutRegion);

                Rect bounds = textLayoutRegion.LayoutBounds;

                _cursor.Transform.LocalPosition = new Vector2((float)bounds.Left + UIDefaults.XTextPadding, (
                    float)bounds.Top + UIDefaults.YTextPadding);
            }

            _cursor.Draw(ds);
        }

        /// <summary>
        /// Draws the highlight on top of the selected text
        /// </summary>
        /// <param name="ds"></param>
        private void DrawSelection(CanvasDrawingSession ds)
        {
            // Highlight selected characters
            if (_hasSelection && (_selectionStartIndex != _selectionEndIndex))
            {
                _cursor.IsVisible = false;
                int firstIndex = Math.Min(_selectionStartIndex, _selectionEndIndex);
                int length = Math.Abs(_selectionEndIndex - _selectionStartIndex) + 1;

                // Make sure we don't draw selection that is out of bounds
                if (firstIndex + length > _maxIndex)
                {
                    int diff = (firstIndex + length) - _maxIndex;
                    length -= diff;
                }
                else if (firstIndex < _minIndex)
                {
                    int diff = _minIndex - firstIndex;
                    length -= diff;
                    firstIndex = _minIndex;
                }

                CanvasTextLayoutRegion[] descriptions = TextLayout.GetCharacterRegions(firstIndex, length);
                foreach (CanvasTextLayoutRegion description in descriptions)
                {
                    ICanvasBrush b = new CanvasSolidColorBrush(ds, Colors.Black);
                    b.Opacity = 0.25f;

                    Rect r = description.LayoutBounds;
                    r.X += _xOffset;
                    r.Y += _yOffset;
                    ds.FillRectangle(r, b);
                }
            }
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            if (Width != _viewBox.Width || Height != _viewBox.Height)
            {
                UpdateDimensions();
            }

            DrawCursor(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            var t1 = Matrix3x2.CreateTranslation(UIDefaults.XTextPadding, UIDefaults.YTextPadding);
            ds.Transform = ds.Transform * t1;

            DrawSelection(ds);

            //for (int i = 0; i < Text.Length; i++)
            //{
            //    CanvasTextLayoutRegion textLayoutRegion;
            //    TextLayout.GetCaretPosition(i, false, out textLayoutRegion);

            //    ds.DrawRectangle(textLayoutRegion.LayoutBounds, Colors.Blue, 2);
            //}

            ds.Transform = orgTransform;

        }

        /// <summary>
        /// If there is a resize, update the view box
        /// </summary>
        private void UpdateDimensions()
        {
            _viewBox = new Rect(0, 0, Width - 2 * (BorderWidth + UIDefaults.XTextPadding),
                                Height - 2 * (BorderWidth + UIDefaults.YTextPadding));
        }

        /// <summary>
        /// Update the blinking cursor
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            base.Update(parentLocalToScreenTransform);

            if (_hasFocus && !_hasSelection)
            {
                _blinkCounter++;
                if (_blinkCounter % 20 == 0)
                {
                    _cursor.IsVisible = false;
                }
                if (_blinkCounter % 40 == 0)
                {
                    _cursor.IsVisible = true;
                    _blinkCounter = 0;
                }
            }
           
        }

        public override void Dispose()
        {
            this.Pressed -= EditableTextboxUIElement_Pressed;
            this.Dragged -= EditableTextboxUIElement_Dragged;

            this.OnFocusGained -= EditableTextboxUIElement_OnFocusGained;
            this.OnFocusLost -= EditableTextboxUIElement_OnFocusLost;
            this.KeyPressed -= EditableTextboxUIElement_KeyPressed;
            this.KeyReleased -= EditableTextboxUIElement_KeyReleased;

            base.Dispose();
        }

        /// <summary>
        /// Get the character index of a mouse press point 
        /// Used to move cursor
        /// </summary>
        /// <param name="mouseOverPt"></param>
        /// <returns></returns>
        public virtual int GetHitIndex(Vector2 mouseOverPt)
        {
            CanvasTextLayoutRegion textLayoutRegion;
            TextLayout.HitTest(
                // Correct for offset
                mouseOverPt.X - (float)_xOffset,
                mouseOverPt.Y - (float)_yOffset,
                out textLayoutRegion);
            return textLayoutRegion.CharacterIndex;
        }

        /// <summary>
        /// Update text layout to the current text
        /// </summary>
        /// <param name="resourceCreator"></param>
        /// <returns></returns>
        public virtual CanvasTextLayout CreateTextLayout(ICanvasResourceCreator resourceCreator)
        {
            var textLayout = _scrollVert ? new CanvasTextLayout(resourceCreator, Text, _textFormat,
                                           Width - 2 * (BorderWidth + UIDefaults.XTextPadding), float.MaxValue) :
                                           new CanvasTextLayout(resourceCreator, Text, _textFormat, float.MaxValue,
                                           Width - 2 * (BorderWidth + UIDefaults.XTextPadding));

            return textLayout;
        }

        /// <summary>
        /// Clear the current selection
        /// </summary>
        public void ClearSelection()
        {
            _hasSelection = false;
            _selectionStartIndex = 0;
            _selectionEndIndex = 0;
        }

        // Override the textbox drawtext method to only draw the active text
        public override void DrawText(CanvasDrawingSession ds)
        {
            // save the current transform of the drawing session
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            if (Text != null)
            {

                Debug.Assert(Width - 2 * BorderWidth > 0 && Height - 2 * BorderWidth > 0, "these must be greater than zero or drawText crashes below");

                ds.DrawText(_activeText,
                            new Rect(BorderWidth + UIDefaults.XTextPadding, 
                            BorderWidth + UIDefaults.YTextPadding,
                            Width - 2 * (BorderWidth + UIDefaults.XTextPadding),
                            Height - 2 * (BorderWidth + UIDefaults.YTextPadding)),
                            TextColor, _textFormat);
            }

            ds.Transform = orgTransform;
        }

        /// <summary>
        /// Copies the current selection in the text box into the clipboard
        /// </summary>
        private void Copy()
        {
            if (_hasSelection)
            {
                int firstIndex = Math.Min(_selectionStartIndex, _selectionEndIndex);
                int length = Math.Abs(_selectionEndIndex - _selectionStartIndex) + 1;
                String selection = Text.Substring(firstIndex, length);

                SessionController.Instance.DataPackage.SetText(selection);

                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(SessionController.Instance.DataPackage);
                OnTextCopied(selection);
            }
        }

        /// <summary>
        /// Cuts current selection from the textbox and copies it into the clipboard
        /// </summary>
        private void Cut()
        {
            if (_hasSelection)
            {
                int firstIndex = Math.Min(_selectionStartIndex, _selectionEndIndex);
                int length = Math.Abs(_selectionEndIndex - _selectionStartIndex) + 1;
                String selection = Text.Substring(firstIndex, length);

                SessionController.Instance.DataPackage.SetText(selection);

                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(SessionController.Instance.DataPackage);
                OnTextCut(selection);
                
                Text = Text.Remove(firstIndex, length);
                CursorCharacterIndex -= length;
                if (CursorCharacterIndex < -1)
                {
                    CursorCharacterIndex = -1;
                }
                OnTextChanged(Text);
                RefreshActiveText();
                // Unhighlight selected text
                ClearSelection();
            }
        }

        /// <summary>
        /// Pastes text from clipboard into current location in textbox indicated by the cursor
        /// </summary>
        private async void Paste()
        {
            DataPackageView dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                // If text is selected, paste over it
                if (_hasSelection)
                {
                    int firstIndex = Math.Min(_selectionStartIndex, _selectionEndIndex);
                    int length = Math.Abs(_selectionEndIndex - _selectionStartIndex) + 1;
                    String selection = Text.Substring(firstIndex, length);

                    Text = Text.Remove(firstIndex, length);
                    CursorCharacterIndex -= (length - 1);
                    ClearSelection();
                }
                string text = await dataPackageView.GetTextAsync();
                OnTextPasted(text);
                // Paste text from clipboard into the text
                if (CursorCharacterIndex != -1)
                {
                    Debug.Assert(CursorCharacterIndex <= (Text.Length - 1));
                    Text = Text.Insert(CursorCharacterIndex, text);
                } else
                {
                    CursorCharacterIndex++;
                    Text = Text.Insert(0, text);
                }  
                CursorCharacterIndex += (text.Length-1);
                OnTextChanged(Text);
                CheckTextInBounds();
            }
        }

        private void CheckTextInBounds()
        {
            //RefreshActiveText();
            // Don't need to calculate offsets if all the text fits within the borders
            //Debug.WriteLine("Min length: "+TextLayout.GetMinimumLineLength());
            //Debug.WriteLine("Width: "+Width);
            if (TextLayout.LayoutBounds.Width < (Width - 2*(BorderWidth + UIDefaults.XTextPadding)))
            {
                _activeText = Text;
                _minIndex = 0;
                _maxIndex = Text.Length - 1;
                return;
            }

            //RefreshActiveText();

            Debug.WriteLine(CursorCharacterIndex);
            Debug.WriteLine("MIN: " + _minIndex + " MAX: " + _maxIndex);
            if (CursorCharacterIndex > _maxIndex)
            {
                String over = Text.Substring(_maxIndex, CursorCharacterIndex - _maxIndex);

                var textLayout = new CanvasTextLayout(ResourceCreator, over, _textFormat,
                    TextLayout.GetMinimumLineLength(),
                    Height - 2 * (BorderWidth + UIDefaults.YTextPadding));

                Rect bounds = textLayout.LayoutBounds;

                _xOffset -= bounds.Width;
                _maxIndex += over.Length;
                //_minIndex += over.Length;

                int lowIndex = _minIndex-1;
                               
                double underWidth = 0;
                
                while (underWidth <= bounds.Width)
                {
                    lowIndex++;
                    CanvasTextLayoutRegion textLayoutRegion;
                    TextLayout.GetCaretPosition(lowIndex, true, out textLayoutRegion);

                    underWidth += textLayoutRegion.LayoutBounds.Width;
                }
                //_minIndex = (lowIndex == _minIndex) ? lowIndex : lowIndex - 1;
                _minIndex = lowIndex;

            }
            else if (CursorCharacterIndex < _minIndex && CursorCharacterIndex != -1)
            {
                String under = Text.Substring(CursorCharacterIndex, _minIndex - CursorCharacterIndex);

                var textLayout = new CanvasTextLayout(ResourceCreator, under, _textFormat,
                    TextLayout.GetMinimumLineLength(),
                    Height - 2 * (BorderWidth + UIDefaults.YTextPadding));

                Rect bounds = textLayout.LayoutBounds;

                _xOffset += bounds.Width;
                _minIndex -= under.Length;

                int highIndex = _maxIndex;
                CanvasTextLayoutRegion textLayoutRegion;
                float overWidth = 0;

                while (overWidth <= bounds.Width)
                {
                    TextLayout.GetCaretPosition(highIndex, false, out textLayoutRegion);

                    overWidth += (float)textLayoutRegion.LayoutBounds.Width;
                    highIndex++;
                }
                _maxIndex = highIndex;
            }

            _activeText = Text.Substring(_minIndex, _maxIndex - _minIndex + 1);

            //RefreshActiveText();
        }

        /// <summary>
        /// Refreshes the active text to be the text that is currently in the bounds of the viewing
        /// box. Updates min and max indices accordingly
        /// </summary>
        public void RefreshActiveText()
        {
            int startI = Int32.MaxValue;
            int endI = Int32.MaxValue;
            Rect intersection;
            int i;
            bool emptyRect = false;
            for (i = 0; i < Text.Length; i++)
            {
                CanvasTextLayoutRegion textLayoutRegion;
                TextLayout.GetCaretPosition(i, false, out textLayoutRegion);

                intersection = textLayoutRegion.LayoutBounds;
                intersection.X += _xOffset;
                intersection.Y += _yOffset;

                Debug.WriteLine("THIS: " + (intersection.X + intersection.Width));
                Debug.WriteLine("THAT: " + (_viewBox.Right));

                //intersection.Intersect(_viewBox);
                emptyRect = false;
                if ((intersection.X + intersection.Width) > _viewBox.Right || intersection.X < _viewBox.Left)
                {
                    emptyRect = true;
                }
                

                //if (intersection.Width < textLayoutRegion.LayoutBounds.Width || intersection == Rect.Empty)
                //{
                //    emptyRect = true;
                //}

                if (startI == Int32.MaxValue && !emptyRect)
                {
                    startI = i;
                }

                if (startI != Int32.MaxValue)
                {
                    if (emptyRect)
                    {
                        endI = i-1;
                        break;
                    }
                }
            }

            if (startI != Int32.MaxValue && endI == Int32.MaxValue)
            {
                endI = i;
            }

            if (startI != Int32.MaxValue)
            {
                _activeText = Text.Substring(startI, endI - startI);
                _minIndex = startI;
                _maxIndex = endI;
            }
            else
            {
                _activeText = "";
                _minIndex = 0;
                _maxIndex = 0;
            }

        }

        /// <summary>
        /// Fired when the text of this textbox is changed
        /// Fires the TextChanged event when possible
        /// </summary>
        /// <param name="text"></param>
        public virtual void OnTextChanged(String text)
        {
            TextChanged?.Invoke(this, text);
        }

        /// <summary>
        /// Fired when text in this textbox is copied
        /// Fires the TextCopied event when possible
        /// </summary>
        /// <param name="text">The text that is copied to the clipboard</param>
        public virtual void OnTextCopied(String text)
        {
            TextCopied?.Invoke(this, text);
        }

        /// <summary>
        /// Fired when the text of this textbox is cut
        /// Fires the TextCut event when possible
        /// </summary>
        /// <param name="text">The text that is copied to the clipboard</param>
        public virtual void OnTextCut(String text)
        {
            TextCut?.Invoke(this, text);
        }

        /// <summary>
        /// Fired when the text is pasted into this textbox
        /// Fires the TextPasted event when possible
        /// </summary>
        /// <param name="text">The text that is pasted into the textbox</param>
        public virtual void OnTextPasted(String text)
        {
            TextPasted?.Invoke(this, text);
        }

        /// <summary>
        /// Convert key code to its ascii character/string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string KeyCodeToUnicode(VirtualKey key)
        {
            byte[] keyboardState = new byte[255];
            bool keyboardStateStatus = GetKeyboardState(keyboardState);

            if (!keyboardStateStatus)
            {
                return "";
            }

            uint virtualKeyCode = (uint)key;
            uint scanCode = MapVirtualKey(virtualKeyCode, 0);
            IntPtr inputLocaleIdentifier = GetKeyboardLayout(0);

            StringBuilder result = new StringBuilder();
            ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, result, (int)5, (uint)0, inputLocaleIdentifier);

            return result.ToString();
        }

        [DllImport("user32.dll")]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        static extern int ToUnicodeEx(uint wVirtKey,
            uint wScanCode, byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff,
            int cchBuff, uint wFlags, IntPtr dwhkl);

    }
}
