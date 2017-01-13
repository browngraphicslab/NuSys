using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class ScrollableTextboxUIElement : TextboxUIElement
    {
        /// <summary>
        /// Delegate for the TextChanged event: Takes in the new string of text
        /// </summary>
        /// <param name="item"></param>
        /// <param name="text"></param>
        public delegate void TextHandler(InteractiveBaseRenderItem item, String text);

        /// <summary>
        /// private helper for public property text
        /// </summary>
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
                if (_loaded)
                {
                    EditableTextboxUIElement_TextChanged(this, value);
                    OnTextChanged(text);
                }
            }
        }

        /// <summary>
        /// Event fired when the text is changed
        /// </summary>
        public event TextHandler TextChanged;

        /// <summary>
        /// Event fired when the text is copied
        /// </summary>
        public event TextHandler TextCopied;

        /// <summary>
        /// Event fired when the text is cut
        /// </summary>
        public event TextHandler TextCut;

        /// <summary>
        /// Event fired when the text is pasted
        /// </summary>
        public event TextHandler TextPasted;

        /// <summary>
        /// If the user has made a selection of text this is true
        /// </summary>
        private  bool _hasSelection;
        
        /// <summary>
        /// true if the textbox has focus
        /// </summary>
        private bool _hasFocus;


        /// <summary>
        /// True if the control key is pressed, used for keyboard shortcuts
        /// </summary>
        private bool _isCtrlPressed;

        /// <summary>
        /// Todo say what this is used for
        /// </summary>
        private CanvasPointer _draggedPointer;

        /// <summary>
        /// Todo say what this is used for
        /// </summary>
        private bool _dragging;

        /// <summary>
        /// The CanvasTextLayout, usedful for getting the dimensions and location of text
        /// </summary>
        public CanvasTextLayout TextLayout { get; set; }

        /// <summary>
        /// The direction that the textbox scrolls in
        /// </summary>
        private bool _scrollVert;

        /// <summary>
        /// The text caret used to navigate through text
        /// </summary>
        private RectangleUIElement _caret;


        /// <summary>
        /// The caret character index is the zero based index of the character the caret is to the right of
        /// thus the caret can be at location -1 if it is to the left of the first character in the textbox
        /// </summary>
        public int CaretCharacterIndex { get; set; }

        // Preserves the x position when moving the cursor up and down
        // in the text box
        private float _currCaretX;

        /// <summary>
        /// The start of the current selection
        /// </summary>
        public int _selectionStartIndex;

        /// <summary>
        /// The end of the current selection
        /// </summary>
        public int _selectionEndIndex;

        /// <summary>
        /// counter used to display a blinking cursor, cursor blinks based on this count //todo refactor this so it is the same on different computers
        /// </summary>
        private int _blinkCounter;

        /// <summary>
        /// The top left corner of the text x position
        /// </summary>
        private double _xOffset;

        /// <summary>
        /// The top left corner of the text y position
        /// </summary>
        private double _yOffset;

        /// <summary>
        /// The top left corner of the text layout bounds
        /// </summary>
        private Vector2 _textUpperLeft;

        // Maximum offsets
        private double _maxXOffset;
        private double _maxYOffset;

        // Min and max indices in the text to be drawn on the screen
        private int _maxIndex;
        private int _minIndex;
        private bool _loaded;

        /// <summary>
        /// The color of the placeholder text
        /// </summary>
        public Color PlaceHolderTextColor { get; set; } = UIDefaults.PlaceHolderTextColor;

        /// <summary>
        /// The placeholder text to display on the Scrollable textbox
        /// </summary>
        public string PlaceHolderText { get; set; } = string.Empty;

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

            // the initial caret character index is to the left of the first character
            CaretCharacterIndex = -1;

        }

        /// <summary>
        /// Create the resources for the textbox, shifted to the load method
        /// </summary>
        /// <param name="resourceCreator"></param>
        private void CreateResources(ICanvasResourceCreatorWithDpi resourceCreator)
        {
            // initialize a new canvas text format
            UpdateCanvasTextFormat();

            //todo remove these lines
            _xOffset = 0;
            _yOffset = 0;

            // set the initial position of the upper left corner of the text
            _textUpperLeft = new Vector2(0, 0);

            // initialize a new canvas text layout
            UpdateCanvasTextLayout();

            // initialize a new caret and as a child to the textbox
            InitializeNewCaret();

            // set the caret height to reflect the line height
            UpdateCaretHeight();

            _dragging = false;

            // Set up events
            Pressed += EditableTextboxUIElement_Pressed;
            OnFocusGained += EditableTextboxUIElement_OnFocusGained;
            OnFocusLost += EditableTextboxUIElement_OnFocusLost;
            KeyPressed += EditableTextboxUIElement_KeyPressed;
            KeyReleased += EditableTextboxUIElement_KeyReleased;
            DragStarted += ScrollableTextboxUIElement_DragStarted;
            DragCompleted += ScrollableTextboxUIElement_DragCompleted;
            DoubleTapped += ScrollableTextboxUIElement_DoubleTapped;
        }

        /// <summary>
        /// Initializes a new caret and adds it as a child to the textbox
        /// </summary>
        private void InitializeNewCaret()
        {
            // Initialize cursor and add it to the textbox
            _caret = new RectangleUIElement(this, ResourceCreator)
            {
                Width = 2,
                Background = Colors.Black,
                IsVisible = false
            };
            AddChild(_caret);
        }

        /// <summary>
        /// Update the height of the caret to reflect the height of the line
        /// this should be performed whenever the height of a line changes (font size etc.)
        /// </summary>
        private void UpdateCaretHeight()
        {
            // if we don't have access to the proper resources 
            // then just return
            if (!_loaded)
            {
                return;
            }

            // use the bounds returned by the GetCaretPosition call
            // to set the size of the cursor to the size of the height of the line
            CanvasTextLayoutRegion textLayoutRegion;
            TextLayout.GetCaretPosition(0, false, out textLayoutRegion);
            Rect bounds = textLayoutRegion.LayoutBounds;

            Debug.Assert(_caret != null);
            _caret.Height = (float)bounds.Height;
        }

        /// <summary>
        /// Creates a new text layout. Should be called whenever the height or width changes
        /// </summary>
        /// <param name="resourceCreator"></param>
        /// <returns></returns>
        public virtual void UpdateCanvasTextLayout()
        {
            // if we haven't loaded the resource creator may not exist yet, so return
            if (!_loaded)
            {
                return;
            }

            // otherwise create a new CanvasTextLayout, if we are scrolling vertically then we don't care about
            // the vertical height so we give the text layout the option of using up to float.MaxValue pixels
            // as its height, if we are scrolling horizontally we don't care about the horizontal height so we
            // give the text layout the option of using up to float.MaxValue pixels as its width
            // the actual text layout returned has LayoutBounds, which extend only to the height and width
            // needed to display text
            TextLayout = _scrollVert ? new CanvasTextLayout(ResourceCreator, Text, CanvasTextFormat,
                                           Width - 2 * (BorderWidth + UIDefaults.XTextPadding), float.MaxValue) :
                                           new CanvasTextLayout(ResourceCreator, Text, CanvasTextFormat, float.MaxValue,
                                           Height - 2 * (BorderWidth + UIDefaults.YTextPadding));

        }

        /// <summary>
        /// Load the scrollable textbox ui elements resources, we need to do this
        /// because the textbox relies on elements which require the ResourceCreator to be completely instantiated
        /// There is no way of assuring that resources have been created until the Load call, especially when adding 
        /// elements to the NuSessionViewer
        /// </summary>
        /// <returns></returns>
        public override Task Load()
        {
            // we have been loaded, so the proper resources exist at this point
            _loaded = true;
            CreateResources(ResourceCreator);
            return base.Load();
        }

        /// <summary>
        /// Used to select a whole word when the box is double tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ScrollableTextboxUIElement_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ClearSelection(false);

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
                }
                start--;
            }

            while (end < (Text.Length - 1))
            {
                char nextC = Text[end + 1];
                if (Char.IsWhiteSpace(nextC))
                {
                    break;
                }
                end++;
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
            UpdateCanvasTextLayout();
            Rect r = TextLayout.LayoutBounds;
            _maxXOffset = r.Width <= (Width - 2 * UIDefaults.XTextPadding) ? 0 : r.Width - (Width - 2 * (UIDefaults.XTextPadding + BorderWidth));
            _maxYOffset = r.Height <= (Height - 2 * UIDefaults.YTextPadding) ? 0 : r.Height - (Height - 2 * (UIDefaults.YTextPadding + BorderWidth));
        }

        /// <summary>
        /// Fired when key is released while this element has focus
        /// </summary>
        /// <param name="args"></param>
        private void EditableTextboxUIElement_KeyReleased(KeyEventArgs args)
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
            ClearSelection(false);
            var loc = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            Vector2 pos = new Vector2(loc.X - UIDefaults.XTextPadding, 
                                      loc.Y - UIDefaults.YTextPadding);
            int charIndex = GetHitIndex(pos);
            CaretCharacterIndex = charIndex;

            // check if the previous character was a new line
            if (CaretCharacterIndex != 0 && CaretCharacterIndex < Text.Length &&
                Text.Substring(CaretCharacterIndex - 1, 1) == "\n")
            {
                CaretCharacterIndex--;
            }

            // bound the caret character index to the length of the text
            if (CaretCharacterIndex >= Text.Length)
            {
                CaretCharacterIndex = Text.Length - 1;
            }

            if (Text == "")
            {
                CaretCharacterIndex = -1;
            }

            _selectionStartIndex = charIndex;
        }

        /// <summary>
        /// Fired when key is pressed on the textbox while it has focus
        /// </summary>
        /// <param name="args"></param>
        private void EditableTextboxUIElement_KeyPressed(KeyEventArgs args)
        {

            // set the caret visibility to true
            _caret.IsVisible = true;

            //Backspace Key
            if (args.VirtualKey == VirtualKey.Back)
            {
                _currCaretX = 0;
                if (_hasSelection)
                {
                    ClearSelection();
                }
                // as long as there is at least one character to delete
                else if (CaretCharacterIndex >= 0)
                {
                    // remove the character from the text
                    Text = Text.Remove(CaretCharacterIndex, 1);
                    // fire the text changed event
                    OnTextChanged(Text);
                    // decrement the current character so its on the previous character
                    CaretCharacterIndex--;
                }
            }
            // Delete Key
            else if (args.VirtualKey == VirtualKey.Delete)
            {
                _currCaretX = 0;
                if (_hasSelection)
                {
                    ClearSelection();
                }
                // as long as we are not the last character in the document
                // remove the next character, 
                else if (CaretCharacterIndex < (Text.Length-1))
                {
                    Text = Text.Remove(CaretCharacterIndex+1, 1);
                    OnTextChanged(Text);
                }
            }
            // Move cursor left
            else if (args.VirtualKey == VirtualKey.Left)
            {
                _currCaretX = 0;

                if (_hasSelection)
                {
                    CaretCharacterIndex = Math.Min(_selectionStartIndex, _selectionEndIndex) - 1;
                    CaretCharacterIndex = Math.Max(CaretCharacterIndex, -1);
                    ClearSelection(false);
                }
                else
                {
                    // try to decrement the CaretCharacterIndex, but do not let the CaretCharacter
                    // index decrement below -1
                    CaretCharacterIndex = Math.Max(-1, CaretCharacterIndex - 1);
                }              
            }
            // Move cursor right
            else if (args.VirtualKey == VirtualKey.Right)
            {
                _currCaretX = 0;

                if (_hasSelection)
                {
                    CaretCharacterIndex = Math.Max(_selectionStartIndex, _selectionEndIndex);
                    CaretCharacterIndex = Math.Min(Text.Length - 1, CaretCharacterIndex);
                    ClearSelection(false);
                }
                else
                {
                    // try to incremenet the CaretCharacterIndex, but do not let the CaretCharacter
                    // index increment to beyond the length of the text - 1. 
                    // ('a' is text with length 1, CaretCharacterIndex 0 is to the right of 'a') thats why we subtract one from the length
                    CaretCharacterIndex = Math.Min(Text.Length - 1, CaretCharacterIndex + 1);
                }


            }
            // Move cursor up
            else if (args.VirtualKey == VirtualKey.Up)
            {
                // Use the layout bounds of the cursor to calculate the
                // index of the character directly above the current one
                CanvasTextLayoutRegion textLayoutRegion;
                TextLayout.GetCaretPosition(CaretCharacterIndex, false, out textLayoutRegion);
                Rect bounds = textLayoutRegion.LayoutBounds;
                bounds.X += _textUpperLeft.X;
                bounds.Y += _textUpperLeft.Y;

                // decrement the y point of the position we are checking
                Vector2 pos = new Vector2((float)(bounds.Left + bounds.Width / 2),
                                          (float)(bounds.Top - _caret.Height / 2));
                if (_currCaretX != 0)
                {
                    pos.X = _currCaretX;
                }

                // get the hit index of the character above the current one
                int charIndex = GetHitIndex(pos);
                CaretCharacterIndex = charIndex;
                _currCaretX = pos.X;
            }
            // Move cursor down
            else if (args.VirtualKey == VirtualKey.Down)
            {
                // use GetCaretPosition to get the current position of the caret
                CanvasTextLayoutRegion textLayoutRegion;
                TextLayout.GetCaretPosition(CaretCharacterIndex, false, out textLayoutRegion);

                Rect bounds = textLayoutRegion.LayoutBounds;
                bounds.X += _xOffset;
                bounds.Y += _yOffset;

                // increment the y axis of the current position by the height of the line
                Vector2 pos = new Vector2((float)(bounds.Left + bounds.Width / 2),
                                          (float)(bounds.Bottom + _caret.Height / 2));
                if (_currCaretX != 0)
                {
                    pos.X = _currCaretX;
                }

                // get the index of the character at the pointer directly below the current line
                int charIndex = GetHitIndex(pos);

                // set the Caret Character Index to the new character index
                CaretCharacterIndex = charIndex;
                _currCaretX = pos.X;
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
                if (_hasSelection)
                {
                    ClearSelection();
                }
                Text = Text.Insert(CaretCharacterIndex + 1, "    ");
                CaretCharacterIndex += 4;
            } else if (args.VirtualKey == VirtualKey.Enter)
            {
                if (_hasSelection)
                {
                    ClearSelection();
                }
                _currCaretX = 0;
                Text = Text.Insert(CaretCharacterIndex + 1, "\n");
                CaretCharacterIndex++;
                OnTextChanged(Text);
            }
            // Type the letter into the box
            else
            {
                string s = KeyCodeToUnicode(args.VirtualKey);
                if (s.Length > 0)
                {
                    if (_hasSelection)
                    {
                        ClearSelection();
                    }

                    _currCaretX = 0;
                    Text = Text.Insert(CaretCharacterIndex + 1, s);
                    CaretCharacterIndex++;
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
            //ClearSelection();
            _hasFocus = false;
            //_cursor.IsVisible = false;
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
            _caret.IsVisible = true;
        }

        /// <summary>
        /// Draw the cursor at its appropriate position
        /// </summary>
        /// <param name="ds"></param>
        private void DrawCursor(CanvasDrawingSession ds)
        {
            UpdateCursorLoc();
            _caret.Draw(ds);
        }

        /// <summary>
        /// Updates the cursors location before drawing it to ensure it sta
        /// </summary>
        private void UpdateCursorLoc()
        {
            if (!_loaded)
            {
                return;
            }

            Vector2 newCursorLoc;
            CanvasTextLayoutRegion textLayoutRegion;
            // Sets the cursor's location based on the offsets
            // Cursor should be to the right of characters except when it is -1, then it should
            // be to the left of the first character
            if (CaretCharacterIndex > -1)
            {
                TextLayout.GetCaretPosition(CaretCharacterIndex, false, out textLayoutRegion);

                Rect bounds = textLayoutRegion.LayoutBounds;
                bounds.X += _xOffset;
                bounds.Y += _yOffset;

                if (Text.Substring(CaretCharacterIndex, 1) == "\r" || Text.Substring(CaretCharacterIndex, 1) == "\n")
                {
                    // move the cursor to the next line
                    bounds.Y += bounds.Height;
                    bounds.X = _xOffset;
                }

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
            if (_caret.IsVisible)
            {
                CheckTextInBounds(newCursorLoc);
            }
            _caret.Transform.LocalPosition = newCursorLoc;
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
                _caret.IsVisible = false;
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
            if (!_loaded)
            {
                return;
            }

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

            if (!_hasSelection)
            {
                _blinkCounter++;
                if (_blinkCounter % 20 == 0)
                {
                    _caret.IsVisible = false;
                }
                if (_blinkCounter % 40 == 0)
                {
                    _caret.IsVisible = true;
                    _blinkCounter = 0;
                }
            }

            base.Update(parentLocalToScreenTransform);           
        }

        public override void Dispose()
        {
            Pressed -= EditableTextboxUIElement_Pressed;

            OnFocusGained -= EditableTextboxUIElement_OnFocusGained;
            OnFocusLost -= EditableTextboxUIElement_OnFocusLost;
            KeyPressed -= EditableTextboxUIElement_KeyPressed;
            KeyReleased -= EditableTextboxUIElement_KeyReleased;

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
        /// Clear the current selection safely, returns null if the current selection
        /// had an invalid index
        /// </summary>
        public string ClearSelection(bool deleteSelection = true)
        {
            // dont return anything if we don't actually have selection
            if (!_hasSelection)
            {
                _selectionStartIndex = 0;
                _selectionEndIndex = 0;
                return null;
            }

            string selection;

            // get the lower index of the selection
            int firstIndex = Math.Min(_selectionStartIndex, _selectionEndIndex);

            // get the length of the selection
            int length = Math.Abs(_selectionEndIndex - _selectionStartIndex) + 1;

            // try to remove the selection
            try
            {
                selection = Text.Substring(firstIndex, length);
                if (deleteSelection)
                {
                    Text = Text.Remove(firstIndex, length);
                    CaretCharacterIndex = Math.Max(CaretCharacterIndex - length, -1);
                }

            }
            catch (ArgumentOutOfRangeException e)
            {
                selection = null;
            }

            // we no longer have selection
            _hasSelection = false;
            _selectionStartIndex = 0;
            _selectionEndIndex = 0;

            // return the string we removed
            return selection;
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
                                    TextColor, CanvasTextFormat);
                    } else
                    {
                        ds.DrawText(Text, new Rect(BorderWidth + UIDefaults.XTextPadding + _xOffset,
                                    BorderWidth + UIDefaults.YTextPadding + _yOffset, double.MaxValue,
                                    Height - 2 * (BorderWidth + UIDefaults.YTextPadding)),
                                    TextColor, CanvasTextFormat);
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

                Clipboard.SetContent(SessionController.Instance.DataPackage);
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
                // get the selected text and clear the selection
                var selectedText = ClearSelection();

                // set the text of the clipboard to the selected text
                SessionController.Instance.DataPackage.SetText(selectedText);
                Clipboard.SetContent(SessionController.Instance.DataPackage);

                // fired the text cut event with the selected event
                OnTextCut(selectedText);
                              
            }
        }

        /// <summary>
        /// Pastes text from clipboard into current location in textbox indicated by the cursor
        /// </summary>
        private async void Paste()
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                // If text is selected, paste over it
                if (_hasSelection)
                {
                    int firstIndex = Math.Min(_selectionStartIndex, _selectionEndIndex);
                    int length = Math.Abs(_selectionEndIndex - _selectionStartIndex) + 1;
                    try
                    {
                        string selection = Text.Substring(firstIndex, length);
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        return;
                    }

                    Text = Text.Remove(firstIndex, length);
                    CaretCharacterIndex -= (length);
                    ClearSelection();
                }
                string text = await dataPackageView.GetTextAsync();
                OnTextPasted(text);
                // Paste text from clipboard into the text
                if (CaretCharacterIndex != -1)
                {

                    Debug.Assert(CaretCharacterIndex <= (Text.Length - 1));
                    Text = Text.Insert(CaretCharacterIndex+1, text);
                } else
                {
                    Text = Text.Insert(CaretCharacterIndex + 1, text);
                }  
                CaretCharacterIndex += text.Length;
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
                if ((cursorLoc.Y + _caret.Height) > (Height - (UIDefaults.YTextPadding + BorderWidth)))
                {
                    double over = (cursorLoc.Y + _caret.Height) - (Height - (UIDefaults.YTextPadding + BorderWidth));
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
                    _xOffset += under + _caret.Width;
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
            PlaceHolderTextColor, CanvasTextFormat);
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
        /// Clear all the text from the textbox
        /// </summary>
        public void ClearText()
        {
            CaretCharacterIndex = -1;
            Text = string.Empty;
            OnTextChanged(Text);
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
            ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, result, 5, 0, inputLocaleIdentifier);

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
