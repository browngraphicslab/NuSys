using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
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

        private string text { get; set; }

        /// <summary>
        /// The text to be displayed in the textbox.
        /// </summary>
        public override string Text
        {
            get { return text; }
            set
            {
                text = value;
                if (_constructed)
                {
                    EditableTextboxUIElement_TextChanged(this, value);
                }
            }
        }

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

        private CanvasPointer _draggedPointer;
        private bool _dragging;

        // The current text layout
        public CanvasTextLayout TextLayout { get; set; }

        // Text format currently being used
        protected CanvasTextFormat TextFormat;

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

        // Directional offsets of where the top left corner of the text is
        private double _xOffset;
        private double _yOffset;

        // Maximum offsets
        private double _maxXOffset;
        private double _maxYOffset;

        // Min and max indices in the text to be drawn on the screen
        private int _maxIndex;
        private int _minIndex;

        /// <summary>
        /// The color of the placeholder text
        /// </summary>
        public Color PlaceHolderTextColor { get; set; } = UIDefaults.PlaceHolderTextColor;

        /// <summary>
        /// The placeholder text to display on the Scrollable textbox
        /// </summary>
        public string PlaceHolderText { get; set; } = string.Empty;

        private bool _constructed;

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

            // Initializing the textformat
            TextFormat = new CanvasTextFormat
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

            TextLayout = new CanvasTextLayout(resourceCreator, Text, TextFormat,
                                              Width - 2 * (BorderWidth + UIDefaults.XTextPadding),
                                              Height - 2 * (BorderWidth + UIDefaults.YTextPadding));

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

            _dragging = false;

            _constructed = true;

            // Add cursor as child of the textbox
            this.AddChild(_cursor);

            // Set up events
            this.Pressed += EditableTextboxUIElement_Pressed;
            this.OnFocusGained += EditableTextboxUIElement_OnFocusGained;
            this.OnFocusLost += EditableTextboxUIElement_OnFocusLost;
            this.KeyPressed += EditableTextboxUIElement_KeyPressed;
            this.KeyReleased += EditableTextboxUIElement_KeyReleased;
            this.DragStarted += ScrollableTextboxUIElement_DragStarted;
            this.DragCompleted += ScrollableTextboxUIElement_DragCompleted;
            this.DoubleTapped += ScrollableTextboxUIElement_DoubleTapped;

        }

        /// <summary>
        /// Used to select a whole word when the box is double tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ScrollableTextboxUIElement_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ClearSelection();

            var loc = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            Vector2 pos = new Vector2(loc.X - UIDefaults.XTextPadding,
                                      loc.Y - UIDefaults.YTextPadding);
            int charIndex = GetHitIndex(pos);
            if (Text == "")
            {
                return;
            }

            char c = Text[charIndex];

            if (Char.IsWhiteSpace(c))
            {
                return;
            }
            int start = charIndex;
            int end = charIndex;

            while (start > 0)
            {
                char prevC = Text[start - 1];
                if (Char.IsWhiteSpace(prevC))
                {
                    break;
                } else
                {
                    start--;
                }
            }

            while (end < (Text.Length - 1))
            {
                char nextC = Text[end + 1];
                if (Char.IsWhiteSpace(nextC))
                {
                    break;
                }
                else
                {
                    end++;
                }
            }

            _hasSelection = true;
            _selectionStartIndex = start;
            _selectionEndIndex = end;
        }

        /// <summary>
        /// Fired whenever a drag that was started on this element is completed
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ScrollableTextboxUIElement_DragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _dragging = false;
        }

        /// <summary>
        /// Fired whenever a drag is started on this element
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ScrollableTextboxUIElement_DragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _dragging = true;
            _draggedPointer = pointer;
            _hasSelection = true;
        }

        /// <summary>
        /// Fired whenever the text is changed in this textbox. Updates the current text layout
        /// </summary>
        /// <param name="item"></param>
        /// <param name="text"></param>
        private void EditableTextboxUIElement_TextChanged(InteractiveBaseRenderItem item, string text)
        {
            TextLayout = CreateTextLayout(item.ResourceCreator);
            Rect r = TextLayout.LayoutBounds;
            _maxXOffset = r.Width <= (Width - 2 * UIDefaults.XTextPadding) ? 0 : r.Width - (Width - 2 * (UIDefaults.XTextPadding + BorderWidth));
            _maxYOffset = r.Height <= (Height - 2 * UIDefaults.YTextPadding) ? 0 : r.Height - (Height - 2 * (UIDefaults.YTextPadding + BorderWidth));
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
        /// Fired when text box is pressed on - used to move the cursor to the location of the press
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void EditableTextboxUIElement_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ClearSelection();
            var loc = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            Vector2 pos = new Vector2(loc.X - UIDefaults.XTextPadding, 
                                      loc.Y - UIDefaults.YTextPadding);
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
                if (_hasSelection)
                {
                    int firstIndex = Math.Min(_selectionStartIndex, _selectionEndIndex);
                    int length = Math.Abs(_selectionEndIndex - _selectionStartIndex) + 1;

                    Text = Text.Remove(firstIndex, length);
                    CursorCharacterIndex -= length;
                    if (CursorCharacterIndex < -1)
                    {
                        CursorCharacterIndex = -1;
                    }
                    OnTextChanged(Text);
                    ClearSelection();
                } else if (CursorCharacterIndex >= 0)
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
                if (_hasSelection)
                {
                    int firstIndex = Math.Min(_selectionStartIndex, _selectionEndIndex);
                    int length = Math.Abs(_selectionEndIndex - _selectionStartIndex) + 1;

                    Text = Text.Remove(firstIndex, length);
                    CursorCharacterIndex -= length;
                    if (CursorCharacterIndex < -1)
                    {
                        CursorCharacterIndex = -1;
                    }
                    OnTextChanged(Text);
                    ClearSelection();
                } else if (CursorCharacterIndex < (Text.Length-1))
                {
                    Text = Text.Remove(CursorCharacterIndex+1, 1);
                    OnTextChanged(Text);
                }
            }
            // Move cursor left
            else if (args.VirtualKey == VirtualKey.Left)
            {
                _currCursorX = 0;
                if (CursorCharacterIndex < 1)
                {
                    CursorCharacterIndex = -1;
                }
                else
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
            else if (args.VirtualKey == VirtualKey.Up)
            {
                CanvasTextLayoutRegion textLayoutRegion;
                TextLayout.GetCaretPosition(CursorCharacterIndex, false, out textLayoutRegion);

                Rect bounds = textLayoutRegion.LayoutBounds;
                bounds.X += _xOffset;
                bounds.Y += _yOffset;
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
                bounds.X += _xOffset;
                bounds.Y += _yOffset;
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
        }

        /// <summary>
        /// Triggered when this textbox loses focus
        /// </summary>
        /// <param name="item"></param>
        private void EditableTextboxUIElement_OnFocusLost(BaseRenderItem item)
        {
            // hide blinking cursor
            BorderWidth = 0;
            ClearSelection();
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
            UpdateCursorLoc();
            _cursor.Draw(ds);
        }

        /// <summary>
        /// Updates the cursors location before drawing it to ensure it sta
        /// </summary>
        private void UpdateCursorLoc()
        {
            Vector2 newCursorLoc;
            CanvasTextLayoutRegion textLayoutRegion;
            // Sets the cursor's location based on the offsets
            // Cursor should be to the right of characters except when it is -1, then it should
            // be to the left of the first character
            if (CursorCharacterIndex > -1)
            {
                TextLayout.GetCaretPosition(CursorCharacterIndex, false, out textLayoutRegion);

                Rect bounds = textLayoutRegion.LayoutBounds;
                bounds.X += _xOffset;
                bounds.Y += _yOffset;

                newCursorLoc = new Vector2((float)bounds.Right + UIDefaults.XTextPadding + BorderWidth,
                                           (float)bounds.Top + UIDefaults.YTextPadding);
            } else
            {
                TextLayout.GetCaretPosition(0, false, out textLayoutRegion);

                Rect bounds = textLayoutRegion.LayoutBounds;
                bounds.X += _xOffset;
                bounds.Y += _yOffset;

                newCursorLoc = new Vector2((float)bounds.Left + UIDefaults.XTextPadding + BorderWidth,
                                           (float)bounds.Top + UIDefaults.YTextPadding);
            }

            // Only want to shift the box if the cursor is visible
            if (_cursor.IsVisible)
            {
                CheckTextInBounds(newCursorLoc);
            }
            _cursor.Transform.LocalPosition = newCursorLoc;
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


                CanvasTextLayoutRegion[] descriptions = TextLayout.GetCharacterRegions(firstIndex, length);
                foreach (CanvasTextLayoutRegion description in descriptions)
                {
                    ICanvasBrush b = new CanvasSolidColorBrush(ds, Colors.Black);
                    b.Opacity = 0.25f;

                    Rect r = description.LayoutBounds;
                    r.X += _xOffset;
                    r.Y += _yOffset;

                    // Only draw selection within the bounds of the textbox
                    using (ds.CreateLayer(1, CanvasGeometry.CreateRectangle(Canvas, 0, 0, Width - 2 * (BorderWidth + UIDefaults.XTextPadding), 
                                                                            Height - (UIDefaults.YTextPadding + 2*BorderWidth))))
                    {
                        ds.FillRectangle(r, b);
                    }
                }
            }
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            // Shift the textbox based on where the user is dragging
            if (_dragging)
            {
                ShiftTextOnDrag();
            }
            // Make sure the text fits to the box if able
            ShiftTextToFit();

            DrawCursor(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            var t1 = Matrix3x2.CreateTranslation(UIDefaults.XTextPadding, UIDefaults.YTextPadding);
            ds.Transform = ds.Transform * t1;

            DrawSelection(ds);

            DrawPlaceHolderText(ds);


            ds.Transform = orgTransform;

        }

        /// <summary>
        /// Shifts the text to stay visible as the user is highlighting text
        /// </summary>
        private void ShiftTextOnDrag()
        {

            var loc = Vector2.Transform(_draggedPointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            Vector2 pos = new Vector2(loc.X - UIDefaults.XTextPadding,
                                      loc.Y - UIDefaults.YTextPadding);
            _selectionEndIndex = GetHitIndex(pos);

            // Update y offset if vertical scrolling textbox, x offset otherwise
            if (_scrollVert)
            {
                if (pos.Y < 0)
                {
                    double under = -pos.Y;
                    _yOffset += under;
                    if (_yOffset > 0)
                    {
                        _yOffset = 0;
                    }
                } else if (pos.Y > (Height - 2*UIDefaults.YTextPadding))
                {
                    double over = pos.Y - (Height - 2 * UIDefaults.YTextPadding);
                    _yOffset -= over;
                    if (_yOffset < -_maxYOffset)
                    {
                        _yOffset = -_maxYOffset;
                    }
                }
            } else
            {
                if (pos.X < 0)
                {
                    double under = -pos.X;
                    _xOffset += under;
                    if (_xOffset > 0)
                    {
                        _xOffset = 0;
                    }
                }
                else if (pos.X > (Width - 2 * UIDefaults.XTextPadding))
                {
                    double over = pos.X - (Width - 2 * UIDefaults.XTextPadding);
                    _xOffset -= over;
                    if (_xOffset < -_maxXOffset)
                    {
                        _xOffset = -_maxXOffset;
                    }
                }
            }  
        }

        /// <summary>
        /// Shifts the text to fill the box as much as possible. Looks for empty shifts text accordingly.
        /// </summary>
        private void ShiftTextToFit()
        {
            if (Text.Length == 0)
            {
                return;
            }
            CanvasTextLayoutRegion textLayoutRegion;
            TextLayout.GetCaretPosition(Text.Length-1, false, out textLayoutRegion);
            Rect r = textLayoutRegion.LayoutBounds;
            r.X += _xOffset;
            r.Y += _yOffset;

            if (_scrollVert)
            {
                double h = TextLayout.LayoutBounds.Height;
                double end = r.Bottom;
                double size = Height - 2 * (BorderWidth + UIDefaults.YTextPadding);
                if (end < size && h > size)
                {
                    double under = size - end;
                    _yOffset += under;
                }
            } else
            {
                double w = TextLayout.LayoutBounds.Width;
                double end = r.Right;
                double size = Width - 2 * (BorderWidth + UIDefaults.XTextPadding);
                if (end < size && w > size)
                {
                    double under = size - end;
                    _xOffset += under;
                }
            }
            
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
            var textLayout = _scrollVert ? new CanvasTextLayout(resourceCreator, Text, TextFormat,
                                           Width - 2 * (BorderWidth + UIDefaults.XTextPadding), float.MaxValue) :
                                           new CanvasTextLayout(resourceCreator, Text, TextFormat, float.MaxValue,
                                           Height - 2 * (BorderWidth + UIDefaults.YTextPadding));

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

                // Only draw text in bounds of textbox
                using (ds.CreateLayer(1, CanvasGeometry.CreateRectangle(Canvas, BorderWidth + UIDefaults.XTextPadding, BorderWidth + UIDefaults.YTextPadding,
                                                                        Width - 2 * (BorderWidth + UIDefaults.XTextPadding),
                                                                        Height - 2 * (BorderWidth + UIDefaults.YTextPadding))))
                {
                    if (_scrollVert)
                    {
                        ds.DrawText(Text, new Rect(BorderWidth + UIDefaults.XTextPadding + _xOffset,
                                    BorderWidth + UIDefaults.YTextPadding + _yOffset,
                                    Width - 2 * (BorderWidth + UIDefaults.XTextPadding), double.MaxValue),
                                    TextColor, TextFormat);
                    } else
                    {
                        ds.DrawText(Text, new Rect(BorderWidth + UIDefaults.XTextPadding + _xOffset,
                                    BorderWidth + UIDefaults.YTextPadding + _yOffset, double.MaxValue,
                                    Height - 2 * (BorderWidth + UIDefaults.YTextPadding)),
                                    TextColor, TextFormat);
                    }
                }
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
                    Text = Text.Insert(CursorCharacterIndex+1, text);
                } else
                {
                    CursorCharacterIndex++;
                    Text = Text.Insert(CursorCharacterIndex+1, text);
                }  
                CursorCharacterIndex += text.Length;
                OnTextChanged(Text);
            }
        }

        /// <summary>
        /// Checks whether the text to be drawn is in bounds based on the cursor position
        /// </summary>
        private void CheckTextInBounds(Vector2 cursorLoc)
        {
            if (_scrollVert)
            {
                if ((cursorLoc.Y + _cursor.Height) > (Height - (UIDefaults.YTextPadding + BorderWidth)))
                {
                    double over = (cursorLoc.Y + _cursor.Height) - (Height - (UIDefaults.YTextPadding + BorderWidth));
                    _yOffset -= over;
                    UpdateCursorLoc();
                }
                else if (cursorLoc.Y < (UIDefaults.YTextPadding))
                {
                    double under = (UIDefaults.YTextPadding) - cursorLoc.Y;
                    _yOffset += under;
                    UpdateCursorLoc();
                }
            } else
            {
                if (cursorLoc.X > (Width - (UIDefaults.XTextPadding + BorderWidth)))
                {
                    double over = cursorLoc.X - (Width - (UIDefaults.XTextPadding + BorderWidth));
                    _xOffset -= over;
                    UpdateCursorLoc();
                }
                else if (cursorLoc.X < (UIDefaults.XTextPadding))
                {
                    double under = (UIDefaults.XTextPadding) - cursorLoc.X;
                    _xOffset += under + _cursor.Width;
                    UpdateCursorLoc();
                }
            }
            
        }

        /// <summary>
        /// Draws the placeholder text if the textbox is empty
        /// </summary>
        /// <param name="ds"></param>
        private void DrawPlaceHolderText(CanvasDrawingSession ds)
        {
            if (string.IsNullOrEmpty(Text))
            {
                var orgTransform = ds.Transform;
                ds.Transform = Transform.LocalToScreenMatrix;
                ds.DrawText(PlaceHolderText, new Rect(BorderWidth + UIDefaults.XTextPadding,
            BorderWidth + UIDefaults.YTextPadding,
            Width - 2 * (BorderWidth + UIDefaults.XTextPadding), double.MaxValue),
            PlaceHolderTextColor, TextFormat);
                ds.Transform = orgTransform;
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

        private String GetSelectedWord(int charIndex)
        {
            return "";
        }

        // FUNCTIONS TO CONVERT KEYCODE TO STRING UNICODE CHARACTER
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
