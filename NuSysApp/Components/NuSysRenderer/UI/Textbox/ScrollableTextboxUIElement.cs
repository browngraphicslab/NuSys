using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
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
        /// The text to be displayed in the textbox.
        /// </summary>
        public override string Text
        {
            get { return base.Text; }
            set
            {
                // replace all new lines in input with the constant new line character
                base.Text = value;
                if (_loaded)
                {
                    EditableTextboxUIElement_TextChanged(this, value);

                    // if text is being programatically changed bound the caret character index
                    if (!HasFocus)
                    {
                        CaretCharacterIndex = Math.Min(-1, Math.Max(CaretCharacterIndex, Text.Length - 1));
                    }
                }
                OnTextChanged(Text);
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
        private bool _hasSelection;

        /// <summary>
        /// True if the control key is pressed, used for keyboard shortcuts
        /// </summary>
        private bool _isCtrlPressed;

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
        /// Regex for finding all instances of new line in text
        /// </summary>

        private static Regex _newLineRegex = new Regex(@"\r\n?|\n", RegexOptions.Compiled);

        private const string Newline = "\n";

        /// <summary>
        /// private helper for public variable character index
        /// </summary>
        private int _caretCharacterIndex { get; set; }

        /// <summary>
        /// The caret character index is the zero based index of the character the caret is to the right of
        /// thus the caret can be at location -1 if it is to the left of the first character in the textbox
        /// </summary>
        public int CaretCharacterIndex
        {
            get { return _caretCharacterIndex; }
            // bound the CaretCharacterIndex from -1 to Text.Length -1 inclusive
            set { _caretCharacterIndex = Math.Max(-1, Math.Min(Text.Length - 1, value)); }
        }

        /// <summary>
        /// The x position we try to preserve when moving up or down in the textbox using the arrow keys,
        /// Set to float min value if considered not set
        /// </summary>
        private float _upDownCaretNavHorizonalOffset;

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
        /// True if the load call has been made and all the proper resources have been created
        /// </summary>
        private bool _loaded;

        /// <summary>
        /// List of rectangles which combined make up the currently selected text
        /// </summary>
        private List<Rect> _selectionRects;

        /// <summary>
        /// True if we want to update the selection rects false otherwise
        /// </summary>
        private bool _updateSelectionRects;

        /// <summary>
        /// True if we want to update the transform of the cursor
        /// </summary>
        private bool _updateCaretTransform;

        /// <summary>
        /// Brush used to draw the selectionRects
        /// </summary>
        private CanvasSolidColorBrush _selectionBrush;

        /// <summary>
        /// The color of the placeholder text
        /// </summary>
        public Color PlaceHolderTextColor { get; set; } = UIDefaults.PlaceHolderTextColor;

        /// <summary>
        /// The placeholder text to display on the Scrollable textbox
        /// </summary>
        public string PlaceHolderText { get; set; } = string.Empty;

        /// <summary>
        ///  the vertical scrollbar
        /// </summary>
        private ScrollBarUIElement _verticalScrollbar;

        /// <summary>
        /// the initial _yOffset when drag scrolling
        /// </summary>
        private double _initialDragYOffset;

        /// <summary>
        /// the initial _xOffset when drag scrolling
        /// </summary>
        private double _initialDragXOffset;

        /// <summary>
        /// true if we want to scroll to keep the caret on the screen
        /// </summary>
        private bool _keepCaretOnScreen;

        /// <summary>
        /// True if we want to update the canvas text layout, when this is true we also update the caret transform
        /// and the selection rects
        /// </summary>
        private bool _updateCanvasTextLayout;

        /// <summary>
        ///  private helper for public boolean IsEditable
        /// </summary>
        private bool _isEditable { get; set; }

        /// <summary>
        /// True if the scrollable textbox ui element is editable
        /// </summary>
        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                _isEditable = value;
                if (!IsEditable)
                {
                    _caret.IsVisible = false;
                }
            }
        }


        /// <summary>
        /// true if the shit button is currently pressed false otherwise, use this for keyboard shortcuts not capitalization
        /// </summary>
        private bool _isShiftPressed;

        /// <summary>
        /// The most recently set width of the canvas text layout
        /// </summary>
        private float _textLayoutWidth;

        /// <summary>
        /// The most recently set height of the canvas text layout
        /// </summary>
        private float _textLayoutHeight;

        /// <summary>
        /// Handler for the input submitted event, fired whenever the user hits the enter button on a non vertically scrolling textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="input"></param>
        public delegate void InputSubmittedHandler(ScrollableTextboxUIElement sender, string input);

        /// <summary>
        /// The input submitted event, fired when the user hits the enter button on a non vertically scrolling textbox
        /// </summary>
        public event InputSubmittedHandler InputSubmitted;


        /// <summary>
        /// Position is a float from 0 to 1 representing the start of the scroll bar, fired whenever the scrollbar position changes
        /// </summary>
        public event ScrollBarUIElement.ScrollBarPositionChangedHandler ScrollBarPositionChanged;

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
            TextVerticalAlignment = CanvasVerticalAlignment.Top;

            // Don't wrap text if it is a horizontally scrolling textbox
            Wrapping = scrollVert ? CanvasWordWrapping.EmergencyBreak : CanvasWordWrapping.NoWrap;
            TrimmingSign = CanvasTrimmingSign.None;

            // the initial caret character index is to the left of the first character
            CaretCharacterIndex = -1;

            // initialize the list of selection rects
            _selectionRects = new List<Rect>();

            if (scrollVert)
            {
                _verticalScrollbar = new ScrollBarUIElement(this, resourceCreator,
                    ScrollBarUIElement.Orientation.Vertical)
                {
                    Width = 15
                };

                if (showScrollBar)
                {
                    AddChild(_verticalScrollbar);
                }

                _verticalScrollbar.ScrollBarPositionChanged += _verticalScrollbar_ScrollBarPositionChanged;
            }

            IsEditable = true;

        }

        /// <summary>
        /// Create the resources for the textbox, shifted to the load method
        /// </summary>
        /// <param name="resourceCreator"></param>
        private void CreateResources(ICanvasResourceCreatorWithDpi resourceCreator)
        {
            // initialize a new canvas text format
            UpdateCanvasTextFormat();

            // the initial offset of the text is 0,0
            _xOffset = 0;
            _yOffset = 0;

            // initialize a new canvas text layout
            UpdateCanvasTextLayout();

            // initialize a new caret and as a child to the textbox
            InitializeNewCaret();

            // set the caret height to reflect the line height
            UpdateCaretHeight();

            // create the brush used to draw selections
            _selectionBrush = new CanvasSolidColorBrush(resourceCreator, Colors.Black)
            {
                Opacity = .25f
            };

            // Set up events
            Pressed += EditableTextboxUIElement_Pressed;
            OnFocusGained += EditableTextboxUIElement_OnFocusGained;
            OnFocusLost += EditableTextboxUIElement_OnFocusLost;
            KeyPressed += EditableTextboxUIElement_KeyPressed;
            KeyReleased += EditableTextboxUIElement_KeyReleased;
            DragStarted += ScrollableTextboxUIElement_DragStarted;
            Dragged += ScrollableTextboxUIElement_Dragged;
            DoubleTapped += ScrollableTextboxUIElement_DoubleTapped;
            PointerWheelChanged += ScrollableTextboxUIElement_PointerWheelChanged;
        }

        /// <summary>
        /// Initializes a new caret and adds it as a child to the textbox
        /// </summary>
        private void InitializeNewCaret()
        {
            // Initialize cursor don't add it as a child because we want to draw it
            // outside of the usual hierarchy
            _caret = new RectangleUIElement(this, ResourceCreator)
            {
                Width = 2,
                Background = Colors.Black,
                IsVisible = false
            };

            // instead of adding it as a child we just set the parent of the child's 
            // transform to the textbox itself
            _caret.Transform.SetParent(Transform);

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

        #region mouse-input

        /// <summary>
        /// Called whenever the pointer wheel changes on the scrolling textbox
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        /// <param name="delta"></param>
        private void ScrollableTextboxUIElement_PointerWheelChanged(InteractiveBaseRenderItem item, CanvasPointer pointer, float delta)
        {
            // if we scroll vertically
            if (_scrollVert)
            {
                // change the position of the scrollbar
                _yOffset -= TextLayout.LayoutBoundsIncludingTrailingWhitespace.Height * (delta > 0 ? -.05 : .05);
                BoundYOffset();
                _updateCaretTransform = true;
                _updateSelectionRects = true;
            }
        }

        /// <summary>
        /// Used to select a whole word when the box is double tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ScrollableTextboxUIElement_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // clear the current selection
            ClearSelection(false);

            // we want to update the cursor transform
            _updateCaretTransform = true;

            // get the index of the character we clicked on this could be negative 1
            // make this the selection start index
            _selectionStartIndex = GetCaretCharacterIndexFromPoint(pointer.CurrentPoint);
            _selectionEndIndex = _selectionStartIndex + 1;

            // decrement the selection start index until we get a character that is punctuation or whitespace
            while (_selectionStartIndex >= 0)
            {
                if (char.IsWhiteSpace(Text[_selectionStartIndex]) || char.IsPunctuation(Text[_selectionStartIndex]))
                {
                    break;
                }
                _selectionStartIndex--;
            }


            while (_selectionEndIndex <= Text.Length - 2)
            {
                if (char.IsWhiteSpace(Text[_selectionEndIndex]) || char.IsPunctuation(Text[_selectionEndIndex]))
                {
                    _selectionEndIndex--;
                    break;
                }
                _selectionEndIndex++;
            }

            _hasSelection = true;
            CaretCharacterIndex = _selectionEndIndex;
            _updateSelectionRects = true;
        }

        /// <summary>
        /// Fired whenever a drag is started on this element
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ScrollableTextboxUIElement_DragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // for touch pointer's drag only changes the transform so don't do any selection code
            if (pointer.DeviceType == PointerDeviceType.Touch)
            {
                _initialDragXOffset = _xOffset;
                _initialDragYOffset = _yOffset;
                return; // just return
            }

            if (IsEditable)
            {
                // if either shift is not pressed or we don't have selection, otherwise we'll be extending the current selection
                if (!_isShiftPressed || !_hasSelection)
                {
                    // set the _selectionStartIndex to the current caret position
                    _selectionStartIndex = GetCaretCharacterIndexFromPoint(pointer.CurrentPoint);
                }
            }


        }

        /// <summary>
        /// Fired whenever the pointer is dragged on the scrollable textbox, if the pointer is a touch pointer
        /// then we scroll otherwise we make a selection
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ScrollableTextboxUIElement_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // when we are using touch scroll on drag
            if (pointer.DeviceType == PointerDeviceType.Touch)
            {
                if (_scrollVert)
                {
                    _yOffset = _initialDragYOffset + pointer.Delta.Y;
                    BoundYOffset();
                    _updateCaretTransform = true;
                    _updateSelectionRects = true;
                }
                else
                {
                    _xOffset = _initialDragXOffset + pointer.Delta.X;
                    BoundXOffset();
                    _updateCaretTransform = true;
                    _updateSelectionRects = true;
                }

                return;
            }

            if (IsEditable)
            {
                // update the caret character index to reflect the current point
                CaretCharacterIndex = GetCaretCharacterIndexFromPoint(pointer.CurrentPoint);

                // set the selection end index to the current caret position
                _selectionEndIndex = CaretCharacterIndex;

                // if we have selected at least one character then we have selection
                if (_selectionEndIndex != _selectionStartIndex)
                {
                    _hasSelection = true;
                    _updateSelectionRects = true;
                }
                else // otherwise we do not have selection
                {
                    _hasSelection = false;
                }

                // update the transform of the cursor
                _updateCaretTransform = true;

                // keep the caret on the screen
                _keepCaretOnScreen = true;
            }
        }


        /// <summary>
        /// Fired when text box is pressed on - used to move the cursor to the location of the press
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void EditableTextboxUIElement_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (IsEditable)
            {


                // we are no longer moving using up down arrows so reset the x offset used for that navigation
                _upDownCaretNavHorizonalOffset = float.MinValue;
                // we want to update the cursor transform to its new position
                _updateCaretTransform = true;

                // if shift is pressed we want to extend the current selection or create a selection between
                // the caret's current location and the 
                if (_isShiftPressed)
                {
                    if (!_hasSelection)
                    {
                        _selectionStartIndex = CaretCharacterIndex;
                    }
                    // update the CaretCharacterIndex to reflect the new caret character location
                    CaretCharacterIndex = GetCaretCharacterIndexFromPoint(pointer.CurrentPoint);
                    _selectionEndIndex = CaretCharacterIndex;
                    _hasSelection = _selectionStartIndex != _selectionEndIndex;
                    _updateSelectionRects = true;

                }
                else
                {
                    // clear the current selection and set the caret character index based on the pointer input
                    ClearSelection(false);
                    // update the CaretCharacterIndex
                    CaretCharacterIndex = GetCaretCharacterIndexFromPoint(pointer.CurrentPoint);
                }
            }

        }

        /// <summary>
        /// Fired whenever the text is changed in this textbox. Updates the current text layout
        /// </summary>
        /// <param name="item"></param>
        /// <param name="text"></param>
        private void EditableTextboxUIElement_TextChanged(InteractiveBaseRenderItem item, string text)
        {
            UpdateCanvasTextLayout();
            TextChanged?.Invoke(this, text);
        }
        #endregion mouse-input


        #region keyboard-input
        /// <summary>
        /// Handle all things to do with keyboard key being released
        /// </summary>
        /// <param name="args"></param>
        private void EditableTextboxUIElement_KeyReleased(KeyArgs args)
        {
            if (args.Key == VirtualKey.Control)
            {
                _isCtrlPressed = false;
            }
            else if (args.Key == VirtualKey.Shift)
            {
                _isShiftPressed = false;
            }

        }

        /// <summary>
        /// Handle all thing to do with keyboard key being pressed
        /// </summary>
        /// <param name="args"></param>
        private void EditableTextboxUIElement_KeyPressed(KeyArgs args)
        {
            if (!IsEditable)
            {
                return;
            }

            // set the caret visibility to true
            _caret.IsVisible = true;

            // we want to update the cursor transform
            _updateCaretTransform = true;

            // we want to keep the caret on the screen
            _keepCaretOnScreen = true;

            // set the caret offset to 0 if we are not
            // navigating up or down
            if (args.Key != VirtualKey.Up && args.Key != VirtualKey.Down)
            {
                _upDownCaretNavHorizonalOffset = float.MinValue;
            }



            //Backspace Key
            if (args.Key == VirtualKey.Back)
            {
                if (_hasSelection)
                {
                    ClearSelection();
                }
                // as long as there is at least one character to delete
                else if (CaretCharacterIndex >= 0)
                {
                    // remove the character from the text
                    Text = Text.Remove(CaretCharacterIndex--, 1);
                    // decrement the current character so its on the previous character
                }
            }
            // Delete Key
            else if (args.Key == VirtualKey.Delete)
            {
                if (_hasSelection)
                {
                    ClearSelection();
                }
                // as long as we are not the last character in the document
                // remove the next character, 
                else if (CaretCharacterIndex < Text.Length - 1)
                {
                    Text = Text.Remove(CaretCharacterIndex + 1, 1);
                }
            }
            // Move cursor left
            else if (args.Key == VirtualKey.Left)
            {

                if (_hasSelection)
                {
                    ClearSelection(false);
                }
                else
                {
                    // decrement the CaretCharacterIndex
                    CaretCharacterIndex--;
                }
            }
            // Move cursor right
            else if (args.Key == VirtualKey.Right)
            {
                if (_hasSelection)
                {
                    var newCaretCharacterIndex = Math.Max(_selectionStartIndex, _selectionEndIndex);
                    ClearSelection(false);
                    CaretCharacterIndex = newCaretCharacterIndex;
                }
                else
                {
                    // try to incremenet the CaretCharacterIndex, but do not let the CaretCharacter
                    // index increment to beyond the length of the text - 1. 
                    // ('a' is text with length 1, CaretCharacterIndex 0 is to the right of 'a') thats why we subtract one from the length
                    CaretCharacterIndex++;
                }
            }
            // Move cursor up
            else if (args.Key == VirtualKey.Up)
            {
                // get the current local position of the caret, used for finding the position on the line above
                // we store it here because ClearSelection changes the caret position to the start
                // of the selection that was cleared
                var currentCaretPosition = _caret.Transform.LocalPosition;

                // clear the current selection if it exists
                if (_hasSelection)
                {
                    ClearSelection(false);
                }

                // if we aren't currently saving our x offset save it
                if (_upDownCaretNavHorizonalOffset == float.MinValue)
                {
                    _upDownCaretNavHorizonalOffset = currentCaretPosition.X;
                }

                // get the hit index of the character above the current one
                var charIndex = GetCaretCharacterIndexFromPoint(new Vector2(_upDownCaretNavHorizonalOffset, _caret.Transform.LocalY - _caret.Height), false);
                CaretCharacterIndex = charIndex;
            }
            // Move cursor down
            else if (args.Key == VirtualKey.Down)
            {
                // get the current local position of the caret, used for finding the position on the line above
                // we store it here because ClearSelection changes the caret position to the start
                // of the selection that was cleared
                var currentCaretPosition = _caret.Transform.LocalPosition;

                // clear the current selection if it exists
                if (_hasSelection)
                {
                    ClearSelection(false);
                }

                // if we aren't currently saving our x offset save it
                if (_upDownCaretNavHorizonalOffset == float.MinValue)
                {
                    _upDownCaretNavHorizonalOffset = currentCaretPosition.X;
                }

                // get the index of the character at the pointer directly below the current line
                var charIndex = GetCaretCharacterIndexFromPoint(new Vector2(_upDownCaretNavHorizonalOffset, _caret.Transform.LocalY + _caret.Height), false);

                // set the Caret Character Index to the new character index
                CaretCharacterIndex = charIndex;
            }
            // Control button pressed
            else if (args.Key == VirtualKey.Control)
            {
                _isCtrlPressed = true;
            }
            // Special case z,x,c,a keys while control is pressed
            else if (args.Key == VirtualKey.C && _isCtrlPressed)
            {
                Copy();
            }
            else if (args.Key == VirtualKey.X && _isCtrlPressed)
            {
                Cut();
            }
            else if (args.Key == VirtualKey.V && _isCtrlPressed)
            {
                Paste();
            }
            else if (args.Key == VirtualKey.A && _isCtrlPressed)
            {
                SelectAll();
            }

            else if (args.Key == VirtualKey.Tab)
            {
                if (_hasSelection)
                {
                    ClearSelection();
                }
                Text = Text.Insert(CaretCharacterIndex + 1, "    ");
                CaretCharacterIndex += 4;
            }
            else if (args.Key == VirtualKey.Enter)
            {
                if (_hasSelection)
                {
                    ClearSelection();
                }

                if (_scrollVert)
                {
                    Text = Text.Insert(CaretCharacterIndex + 1, Newline);
                    CaretCharacterIndex++;
                    OnInputSubmitted();
                }
                else
                {
                    OnInputSubmitted();
                }

            }
            // Type the letter into the box
            else
            {
                var s = KeyCodeToUnicode(args.Key);

                if (s.Length == 0)
                {
                    s = FinalKeyCodeToUnicode(args.Key);
                }

                //Regardless of the keyboard state, se
                if (s.Length > 0)
                {
                    if (_hasSelection)
                    {
                        ClearSelection();
                    }
                    Text = Text.Insert(CaretCharacterIndex + 1, s);
                    CaretCharacterIndex++;
                }
            }
        }

        /// <summary>
        /// Fires the input submitted event, only fired on non vertically scrolling textboxes when the enter key is pressed
        /// </summary>
        private void OnInputSubmitted()
        {
            InputSubmitted?.Invoke(this, Text);
        }

        #endregion keyboard-input


        #region focus
        /// <summary>
        /// Triggered when this textbox loses focus
        /// </summary>
        /// <param name="item"></param>
        private void EditableTextboxUIElement_OnFocusLost(BaseRenderItem item)
        {
            _caret.IsVisible = false;
            ClearSelection(false);

            //SessionController.Instance.SessionView.FreeFormViewer.Keyboard.LosePseudoFocus();
        }

        /// <summary>
        /// Triggered when this textbox gains focus
        /// </summary>
        /// <param name="item"></param>
        private void EditableTextboxUIElement_OnFocusGained(BaseRenderItem item)
        {
            if (IsEditable)
            {
                _caret.IsVisible = true;
                UITask.Run(delegate
                {
                    if (SessionController.Instance.SessionSettings.TouchKeyboardVisible)
                    {
                        if (
                            SessionController.Instance.SessionView.FreeFormViewer.CanvasInteractionManager
                                .LastInteractionType == CanvasInteractionManager.InteractionType.Touch)
                        {
                            SessionController.Instance.SessionView.FreeFormViewer.Keyboard.GainPseudoFocus();
                        }
                    }
                });

            }
            else
            {
                _caret.IsVisible = false;

            }
        }
        #endregion focus

        #region draw

        /// <summary>
        /// Draw the cursor at its appropriate position
        /// </summary>
        /// <param name="ds"></param>
        private void DrawCaret(CanvasDrawingSession ds)
        {
            _caret.Draw(ds);
        }

        /// <summary>
        /// Draws the highlight on top of the selected text
        /// </summary>
        /// <param name="ds"></param>
        private void DrawSelection(CanvasDrawingSession ds)
        {
            // Highlight selected characters
            if (_hasSelection)
            {
                // draw all the selection rects
                foreach (var rect in _selectionRects.ToArray())
                {
                    ds.FillRectangle(rect, _selectionBrush);
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
                ds.DrawText(PlaceHolderText, new Rect(BorderWidth + UIDefaults.XTextPadding,
            BorderWidth + UIDefaults.YTextPadding,
            Width - 2 * (BorderWidth + UIDefaults.XTextPadding) - (_verticalScrollbar?.IsVisible ?? false ? _verticalScrollbar.Width : 0), double.MaxValue),
            PlaceHolderTextColor, CanvasTextFormat);
            }
        }

        /// <summary>
        /// Draws the text the selection, and the cursor, we combine all those calls into one
        /// so that we can order the draw stack correctly, otherwise we would have to call base.Draw
        /// then draw the text, cursor, and selection
        /// </summary>
        /// <param name="ds"></param>
        protected override void DrawText(CanvasDrawingSession ds)
        {
            if (!_loaded || Text == null)
            {
                return;
            }

            // save the current transform of the drawing session
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            // Only draw text in bounds of textbox
            using (
                ds.CreateLayer(1,
                    CanvasGeometry.CreateRectangle(Canvas, BorderWidth + UIDefaults.XTextPadding,
                        BorderWidth + UIDefaults.YTextPadding,
                        Width - (BorderWidth + UIDefaults.XTextPadding),
                        Height - (BorderWidth + UIDefaults.YTextPadding))))
            {
                Debug.Assert(Width - 2 * BorderWidth > 0 && Height - 2 * BorderWidth > 0,
                        "these must be greater than zero or drawText crashes below");

                // DrawPlaceHolderText
                DrawPlaceHolderText(ds);

                // Draw Regular Text
                if (_scrollVert)
                {
                    ds.DrawText(Text, new Rect(BorderWidth + UIDefaults.XTextPadding + _xOffset,
                        BorderWidth + UIDefaults.YTextPadding + _yOffset,
                        Width - 2 * (BorderWidth + UIDefaults.XTextPadding) -
                        (_verticalScrollbar.IsVisible ? _verticalScrollbar.Width : 0), double.MaxValue),
                        TextColor, CanvasTextFormat);
                }
                else
                {
                    ds.DrawText(Text, new Rect(BorderWidth + UIDefaults.XTextPadding + _xOffset,
                        BorderWidth + UIDefaults.YTextPadding + _yOffset, double.MaxValue,
                        Height - 2 * (BorderWidth + UIDefaults.YTextPadding)),
                        TextColor, CanvasTextFormat);
                }

                // Draw Selection
                DrawSelection(ds);

                // Draw the cursor
                DrawCaret(ds);
            }


            ds.Transform = orgTransform;
        }
        #endregion draw

        #region update
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
            var bounds = textLayoutRegion.LayoutBounds;

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
            if (_scrollVert)
            {
                _textLayoutWidth = Math.Max(0, Width - 2 * (BorderWidth + UIDefaults.XTextPadding) -
                                   (_verticalScrollbar.IsVisible ? _verticalScrollbar.Width : 0));
                TextLayout = new CanvasTextLayout(ResourceCreator, Text, CanvasTextFormat, _textLayoutWidth, float.MaxValue);
            }
            else
            {
                _textLayoutHeight = Height - 2 * (BorderWidth + UIDefaults.YTextPadding);
                TextLayout = new CanvasTextLayout(ResourceCreator, Text, CanvasTextFormat, float.MaxValue, _textLayoutHeight);
            }

            //_updateCanvasTextLayout = false;                        

        }

        /// <summary>
        /// Checks to make sure that the text layout bounds are equivalent to the draw bounds
        /// </summary>
        /// <returns></returns>
        private bool? CheckTextLayoutBoundsEqualDrawBounds()
        {
            if (!_loaded)
            {
                return null;
            }

            if (_scrollVert)
            {
                return Math.Abs(_textLayoutWidth - (Width - 2 * (BorderWidth + UIDefaults.XTextPadding) -
                                                    (_verticalScrollbar.IsVisible ? _verticalScrollbar.Width : 0))) < .005;
            }
            else
            {
                return Math.Abs(_textLayoutHeight - (Height - 2 * (BorderWidth + UIDefaults.YTextPadding))) < .005;
            }
        }

        /// <summary>
        /// Call this to update the list of selection rects drawn in the DrawSelection method
        /// </summary>
        private void UpdateSelectionRects()
        {
            // Highlight selected characters
            if (_hasSelection && _loaded)
            {
                // clear the current selection rects
                _selectionRects.Clear();

                var firstIndex = Math.Min(_selectionStartIndex, _selectionEndIndex) == -1 ? 0 : Math.Min(_selectionStartIndex, _selectionEndIndex) + 1;
                var length = Math.Abs(_selectionEndIndex - _selectionStartIndex);


                var selectedCharacterBounds = TextLayout.GetCharacterRegions(firstIndex, length);
                foreach (var characterBounds in selectedCharacterBounds)
                {
                    var boundRect = characterBounds.LayoutBounds;
                    boundRect.X += _xOffset + UIDefaults.XTextPadding + BorderWidth;
                    boundRect.Y += _yOffset + UIDefaults.YTextPadding + BorderWidth;

                    _selectionRects.Add(boundRect);
                }
            }

            _updateSelectionRects = false;
        }


        /// <summary>
        /// Update the blinking cursor
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // update the canvas text layout
            if (CheckTextLayoutBoundsEqualDrawBounds() == false)
            {
                UpdateCanvasTextLayout();
                _updateCaretTransform = true;
                _updateSelectionRects = true;
            }

            // update the transform for the cursor
            if (_updateCaretTransform)
            {
                UpdateCaretTransform();
                if (_keepCaretOnScreen)
                {
                    ScrollTextToContainCaret();
                }
            }

            // update the locations of all the selection rects
            if (_updateSelectionRects)
            {
                UpdateSelectionRects();
            }

            if (HasFocus && _isEditable)
            {
                // update the blink counter
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

            if (_scrollVert && _loaded)
            {
                _verticalScrollbar.Height = Height - 2 * BorderWidth;
                _verticalScrollbar.Transform.LocalPosition = new Vector2(Width - _verticalScrollbar.Width - BorderWidth, BorderWidth);

                // set the position and rang eof the vertical scroll bar
                SetVerticalScrollBarPositionAndRange();

                // shift the text so it fills the textbox if it can
                if (Math.Abs(_verticalScrollbar.Range - 1) < .001)
                {
                    _yOffset = 0;
                }
            }

            // shfit the text so it fills the whole textbox if it can
            if (!_scrollVert && _loaded)
            {
                if ((Width - 2 * BorderWidth - UIDefaults.XTextPadding) /
                    TextLayout.LayoutBoundsIncludingTrailingWhitespace.Width >= 1)
                {
                    _xOffset = 0;
                }
            }

            if (_loaded)
            {
                _caret.Update(Transform.LocalToScreenMatrix);
            }

            base.Update(parentLocalToScreenTransform);
        }

        #endregion update

        public override void Dispose()
        {
            if (_loaded)
            {
                Pressed -= EditableTextboxUIElement_Pressed;
                OnFocusGained -= EditableTextboxUIElement_OnFocusGained;
                OnFocusLost -= EditableTextboxUIElement_OnFocusLost;
                KeyPressed -= EditableTextboxUIElement_KeyPressed;
                KeyReleased -= EditableTextboxUIElement_KeyReleased;
                DragStarted -= ScrollableTextboxUIElement_DragStarted;
                Dragged -= ScrollableTextboxUIElement_Dragged;
                DoubleTapped -= ScrollableTextboxUIElement_DoubleTapped;
                PointerWheelChanged -= ScrollableTextboxUIElement_PointerWheelChanged;
                if (_scrollVert)
                {
                    _verticalScrollbar.ScrollBarPositionChanged -= _verticalScrollbar_ScrollBarPositionChanged;

                }
            }


            base.Dispose();
        }

        #region caretPositioning

        /// <summary>
        /// Gets the index of the CaretCharacterIndex from a point on the screen
        /// </summary>
        /// <param name="mouseOverPt"></param>
        /// <param name="convertPointFromScreenToLocal"></param>
        /// <returns></returns>
        public virtual int GetCaretCharacterIndexFromPoint(Vector2 mouseOverPt, bool convertPointFromScreenToLocal = true)
        {
            // the index we are going to returrn
            int hitIndex;

            // convert the poitn from screen coordinates to local coordinates if necessary
            Vector2 localPoint;
            if (convertPointFromScreenToLocal)
            {
                localPoint = Vector2.Transform(mouseOverPt, Transform.ScreenToLocalMatrix);
            }
            else
            {
                localPoint = mouseOverPt;
            }

            // get the point we are hit testing
            var pointToCheck = new Vector2((float)(localPoint.X - UIDefaults.XTextPadding - BorderWidth - _xOffset),
                                      (float)(localPoint.Y - UIDefaults.YTextPadding - BorderWidth - _yOffset));


            // get the index of the character we clicked on
            CanvasTextLayoutRegion textLayoutRegion;
            TextLayout.HitTest(
                pointToCheck.X,
                pointToCheck.Y,
                out textLayoutRegion);
            var characterIndex = textLayoutRegion.CharacterIndex;

            // get the bounds of the character we clicked on
            TextLayout.GetCaretPosition(characterIndex, true, out textLayoutRegion);
            var characterBounds = textLayoutRegion.LayoutBounds;

            // if we clicked on the left side of the character, decrement the character index by 1, want the cursor
            // to be on the left of the character, so the CharacterIndex should be for the character before the one
            // that was returned
            if (Math.Abs(characterBounds.Left - pointToCheck.X) < Math.Abs(characterBounds.Right - pointToCheck.X))
            {
                hitIndex = characterIndex - 1;
            }
            else
            {
                hitIndex = characterIndex;
            }

            // bound the character index to the lenth of the text
            characterIndex = Math.Min(Text.Length - 1, characterIndex);

            // if the character we clicked on is a newline, determine if we clicked on the next line
            // or on the previous line
            if (characterIndex != -1 && Text.Substring(characterIndex, 1) == Newline)
            {
                // if the point we are checking is above the bottom of the character then we clicked on the same line
                // so the cursor should be before the new line
                if (characterBounds.Bottom > pointToCheck.Y)
                {
                    // decrement the character index by 1 so the cursor is before the new line
                    hitIndex = textLayoutRegion.CharacterIndex - 1;
                }
                else
                {
                    // otherwise set the hit index to the character we clicked on
                    hitIndex = characterIndex;
                }
            }

            // character index is always -1 on empty text
            if (string.IsNullOrEmpty(Text))
            {
                hitIndex = -1;
            }

            return hitIndex;

        }


        /// <summary>
        /// Update the cursor's transform based on the current CaretCharacterIndex
        /// </summary>
        private void UpdateCaretTransform()
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

                var bounds = textLayoutRegion.LayoutBounds;
                bounds.X += _xOffset;
                bounds.Y += _yOffset;

                try
                {
                    if (Text.Substring(CaretCharacterIndex, 1) == "\r" || Text.Substring(CaretCharacterIndex, 1) == "\n")
                    {
                        // move the cursor to the next line
                        bounds.Y += bounds.Height;
                        bounds.X = _xOffset;
                    }
                }
                catch (ArgumentOutOfRangeException exception)
                {

                }


                newCursorLoc = new Vector2((float)bounds.Right + UIDefaults.XTextPadding + BorderWidth,
                                           (float)bounds.Top + UIDefaults.YTextPadding + BorderWidth);
            }
            else
            {
                TextLayout.GetCaretPosition(0, false, out textLayoutRegion);

                var bounds = textLayoutRegion.LayoutBounds;
                bounds.X += _xOffset;
                bounds.Y += _yOffset;

                newCursorLoc = new Vector2((float)bounds.Left + UIDefaults.XTextPadding + BorderWidth,
                                           (float)bounds.Top + UIDefaults.YTextPadding + BorderWidth);
            }

            _caret.Transform.LocalPosition = newCursorLoc;

            _updateCaretTransform = false;
        }

        #endregion caretPositiong


        #region scrolling


        /// <summary>
        /// call this to set the position and range of the vertical scrollbar
        /// </summary>
        private void SetVerticalScrollBarPositionAndRange()
        {
            if (!_loaded || !_scrollVert)
            {
                return;
            }

            var _vertScrollPrevPosition = _verticalScrollbar.Position;

            _verticalScrollbar.Position = (float)(-_yOffset / TextLayout.LayoutBoundsIncludingTrailingWhitespace.Height);
            _verticalScrollbar.Range =
                (float)
                    ((Height - 2 * (BorderWidth + UIDefaults.YTextPadding)) /
                     TextLayout.LayoutBoundsIncludingTrailingWhitespace.Height);

            BoundVerticalScrollBarPosition();


            if (Math.Abs(_vertScrollPrevPosition - _verticalScrollbar.Position) > .005)
            {
                ScrollBarPositionChanged?.Invoke(this, _verticalScrollbar.Position);
            }
        }

        /// <summary>
        /// Call this to bound the vertical scroll bar
        /// </summary>
        private void BoundVerticalScrollBarPosition()
        {
            // bound the vertical scroll bar postiion
            if (_verticalScrollbar.Position + _verticalScrollbar.Range > 1)
            {
                _verticalScrollbar.Position = 1 - _verticalScrollbar.Range;
            }
            if (_verticalScrollbar.Position < 0)
            {
                _verticalScrollbar.Position = 0;
            }
        }

        /// <summary>
        /// Fired whenever the vertical scroll bar's position changes
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position"></param>
        private void _verticalScrollbar_ScrollBarPositionChanged(object source, float position)
        {
            _yOffset = -position * TextLayout.LayoutBoundsIncludingTrailingWhitespace.Height;
            BoundYOffset();
            ScrollBarPositionChanged?.Invoke(this, position);
            _updateCaretTransform = true;
            _updateSelectionRects = true;
        }

        /// <summary>
        /// Set the vertical scroll bar position publicly
        /// </summary>
        /// <param name="newPosition"></param>
        public void SetVerticalScrollBarPosition(float newPosition)
        {
            _yOffset = (float)(-newPosition * TextLayout.LayoutBoundsIncludingTrailingWhitespace.Height);
            BoundYOffset();
            _updateCaretTransform = true;
            _updateSelectionRects = true;
        }

        #endregion scrolling

        #region selection
        /// <summary>
        /// Clear the current selection safely, returns empty string if the current selection
        /// had an invalid index or if there is no selection, also set the CaretCharacterIndex to the start of the
        /// selection that was cleared
        /// </summary>
        public string ClearSelection(bool deleteSelection = true)
        {
            // dont return anything if we don't actually have selection
            if (!_hasSelection)
            {
                _selectionStartIndex = 0;
                _selectionEndIndex = 0;
                return string.Empty;
            }

            string selection;

            // get the starting point of the selection
            var firstIndex = Math.Min(_selectionStartIndex, _selectionEndIndex) == -1 ? 0 : Math.Min(_selectionStartIndex, _selectionEndIndex) + 1;

            // get the legnth of the selection
            var length = Math.Abs(_selectionEndIndex - _selectionStartIndex);

            // set the CaretCharacterIndex to the minimum of the selection start and end indexes
            CaretCharacterIndex = Math.Min(_selectionStartIndex, _selectionEndIndex);
            _updateCaretTransform = true;


            // try to remove the selection
            try
            {
                selection = Text.Substring(firstIndex, length);
                if (deleteSelection)
                {
                    Text = Text.Remove(firstIndex, length);
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                selection = string.Empty;
            }

            // we no longer have selection
            _hasSelection = false;
            _selectionStartIndex = 0;
            _selectionEndIndex = 0;
            _selectionRects.Clear();


            // return the string we removed
            return selection;
        }

        /// <summary>
        /// Returns the currently selected text, if the selection is invalid or
        /// no text is selected return empty string
        /// </summary>
        /// <returns></returns>
        public string GetSelection()
        {
            var firstIndex = Math.Min(_selectionStartIndex, _selectionEndIndex) == -1
                ? 0
                : Math.Min(_selectionStartIndex, _selectionEndIndex) + 1;

            // get the legnth of the selection
            var length = Math.Abs(_selectionEndIndex - _selectionStartIndex);

            try
            {
                return Text.Substring(firstIndex, length);
            }
            catch (ArgumentOutOfRangeException e)
            {
                return string.Empty;
            }
        }
        #endregion selection

        #region data-operations-copy-cut-paste
        /// <summary>
        /// Copies the current selection in the text box into the clipboard
        /// </summary>
        private void Copy()
        {
            var selection = GetSelection();

            SessionController.Instance.DataPackage.SetText(selection);

            Clipboard.SetContent(SessionController.Instance.DataPackage);
            OnTextCopied(selection);
        }

        /// <summary>
        /// Cuts current selection from the textbox and copies it into the clipboard
        /// </summary>
        private void Cut()
        {
            // get the selected text and clear the selection
            var selectedText = ClearSelection();

            // set the text of the clipboard to the selected text
            SessionController.Instance.DataPackage.SetText(selectedText);
            Clipboard.SetContent(SessionController.Instance.DataPackage);

            // fired the text cut event with the selected event
            OnTextCut(selectedText);
        }

        /// <summary>
        /// Pastes text from clipboard into current location in textbox indicated by the cursor
        /// </summary>
        private async void Paste()
        {
            var dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                // If text is selected, paste over it
                if (_hasSelection)
                {
                    ClearSelection();
                }
                var text = await dataPackageView.GetTextAsync();

                // make sure we are only using "\n" new lines instead of carriage returns
                text = NormalizeNewLines(text);

                OnTextPasted(text);
                // Paste text from clipboard into the text
                if (CaretCharacterIndex != -1)
                {
                    Text = Text.Insert(CaretCharacterIndex + 1, text);
                }
                else
                {
                    Text = Text.Insert(CaretCharacterIndex + 1, text);
                }
                CaretCharacterIndex += text.Length;
                _updateCaretTransform = true;
                _keepCaretOnScreen = true;
                OnTextChanged(Text);
            }
        }

        /// <summary>
        /// Selects all the text in the textbox
        /// </summary>
        private void SelectAll()
        {
            // clear the current selection
            ClearSelection(false);

            // set the selection indices to encompass the entirety of the text
            _selectionStartIndex = -1;
            _selectionEndIndex = Text.Length - 1;

            // we have selection if start index is not end index, i.e. if text is not empty
            _hasSelection = _selectionStartIndex != _selectionEndIndex;
            // if we have selection keep caret on screen, and update caret transform
            if (_hasSelection)
            {
                _keepCaretOnScreen = true;
                _updateCaretTransform = true;
                CaretCharacterIndex = _selectionEndIndex;
                _updateSelectionRects = true;
            }


        }

        /// <summary>
        /// Returns the input text with newlines replaced by the proper new lines
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string NormalizeNewLines(string text)
        {
            if (_scrollVert)
            {
                return _newLineRegex.Replace(text, Newline);
            }
            else
            {
                return _newLineRegex.Replace(text, " ");
            }
        }

        #endregion data-operations-copy-cut-paste

        /// <summary>
        /// If the caret is not within the bounds of the textbox, scrolls the textbox so the caret is back within the 
        /// bounds of the textbox
        /// </summary>
        private void ScrollTextToContainCaret()
        {
            var caretLocation = _caret.Transform.LocalPosition;

            // we are scrolling vertically check y position of the caret
            if (_scrollVert)
            {
                // if the caret's y position is below the bottom of the textbox
                if (caretLocation.Y + _caret.Height > Height - (UIDefaults.YTextPadding + BorderWidth))
                {
                    // decrement the offset of the textbox by the amount the caret is over
                    double over = caretLocation.Y + _caret.Height - (Height - (UIDefaults.YTextPadding + BorderWidth));
                    _yOffset -= over;

                    // then update the caret's transform
                    _caret.Transform.LocalPosition = new Vector2(_caret.Transform.LocalX, (float)(_caret.Transform.LocalY - over));
                }
                // otherwise if the caret's y position is above the top of the textbox
                else if (caretLocation.Y < UIDefaults.YTextPadding + BorderWidth)
                {
                    // increment the offest of the textbox by the amount the caret is under
                    double under = UIDefaults.YTextPadding + BorderWidth - caretLocation.Y;
                    _yOffset += under;

                    // then update the caret's transform
                    _caret.Transform.LocalPosition = new Vector2(_caret.Transform.LocalX, (float)(_caret.Transform.LocalY + under));
                }
            }
            else
            {
                // otherwise if the caret is greater than the width of the textbox
                if (caretLocation.X > Width - (UIDefaults.XTextPadding + BorderWidth))
                {
                    // decrement the xoffset of the textbox by the amoutn the caret is over
                    double over = caretLocation.X - (Width - UIDefaults.XTextPadding - BorderWidth);
                    _xOffset -= over;

                    // decrement the caret's x location
                    _caret.Transform.LocalPosition = new Vector2((float)(_caret.Transform.LocalX - over), _caret.Transform.LocalY);
                }
                else if (caretLocation.X < UIDefaults.XTextPadding)
                {
                    double under = UIDefaults.XTextPadding - caretLocation.X;
                    _xOffset += under + _caret.Width;
                    _caret.Transform.LocalPosition = new Vector2((float)(_caret.Transform.LocalX + under), _caret.Transform.LocalY);
                }
            }

            _keepCaretOnScreen = false;

        }

        /// <summary>
        /// Bound the y offset
        /// </summary>
        public void BoundYOffset()
        {
            if (!_loaded)
            {
                return;
            }

            _yOffset = Math.Min(0, _yOffset);

            _yOffset = Math.Max(-(TextLayout.LayoutBounds.Height - Height + 2 * (UIDefaults.YTextPadding + BorderWidth)), _yOffset);


            // shift the text so it fills the textbox if it can
            if (Math.Abs(_verticalScrollbar.Range - 1) < .001)
            {
                _yOffset = 0;
            }
        }

        /// <summary>
        /// Bound the X offset
        /// </summary>
        public void BoundXOffset()
        {
            if (!_loaded)
            {
                return;
            }

            // x offset is how far we are to the left, so it should never be positive


            _xOffset = Math.Max(-(TextLayout.LayoutBounds.Width - Width + 2 * (UIDefaults.XTextPadding + BorderWidth)), _xOffset);
            _xOffset = Math.Min(0, _xOffset);
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
            _updateCaretTransform = true;
            OnTextChanged(Text);
        }

        /// <summary>
        /// Convert key code to its ascii character/string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string KeyCodeToUnicode(VirtualKey key)
        {
            var shift = SessionController.Instance.ShiftHeld;

            //For characters that are unique to a virtual key

            if (key == VirtualKey.Space)
            {
                return " ";
            }

            if (key == VirtualKey.Multiply)
            {
                return "*";
            }

            //For characters that share the same virtualkey (depending on shift)
            if (shift)
            {
                return KeyCodeToUnicodeShift(key);
            }
            else
            {
                return KeyCodeToUnicodeUnshift(key);
            }
        }

        private string KeyCodeToUnicodeUnshift(VirtualKey key)
        {

            var virtualKeyCode = (uint)key;
            var capslock = SessionController.Instance.CapitalLock;

            if (Keyboard.NoShiftKeyToChars.ContainsKey(key))
            {
                return Keyboard.NoShiftKeyToChars[key];
            }

            //Take care of letters
            if (virtualKeyCode >= 65 && virtualKeyCode <= 90)
            {
                var character = key.ToString();
                return capslock ? character : character.ToLower();


            }
            //Take care of numbers

            if (virtualKeyCode >= 48 && virtualKeyCode <= 57)
            {
                return (virtualKeyCode - 48).ToString();
            }

            //Take care of numpad numbers

            if (virtualKeyCode >= 96 && virtualKeyCode <= 105)
            {

                return (virtualKeyCode - 96).ToString();
            }


            return "";
        }

        private string KeyCodeToUnicodeShift(VirtualKey key)
        {
            var virtualKeyCode = (uint)key;
            var capslock = SessionController.Instance.CapitalLock;


            if (Keyboard.ShiftKeyToChars.ContainsKey(key))
            {
                return Keyboard.ShiftKeyToChars[key];
            }

            //Take care of letters
            if ((virtualKeyCode >= 65 && virtualKeyCode <= 90))
            {
                var character = key.ToString();
                return capslock ? character.ToLower() : character;

            }
            return "";
        }

        /// <summary>
        /// Convert key code to its ascii character/string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string FinalKeyCodeToUnicode(VirtualKey key)
        {
            var keyboardState = new byte[255];
            var keyboardStateStatus = GetKeyboardState(keyboardState);

            if (!keyboardStateStatus)
            {
                return "";
            }

            var virtualKeyCode = (uint)key;
            var scanCode = MapVirtualKey(virtualKeyCode, 0);
            var inputLocaleIdentifier = GetKeyboardLayout(0);

            var result = new StringBuilder();
            ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, result, 5, 0, inputLocaleIdentifier);

            return result.ToString();
        }



        public float GetTextHeight()
        {
            if (_loaded)
            {
                return (float)(TextLayout.LayoutBoundsIncludingTrailingWhitespace.Height + 2 * (BorderWidth + UIDefaults.YTextPadding));
            }
            return float.MinValue;
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