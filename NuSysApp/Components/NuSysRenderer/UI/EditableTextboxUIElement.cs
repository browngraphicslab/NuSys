using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI;


namespace NuSysApp
{
    public class EditableTextboxUIElement : TextboxUIElement
    {
        // Delegate for the TextChanged event
        // Takes in the new string of text
        public delegate void TextHandler(InteractiveBaseRenderItem item, String text);

        // Text events
        public event TextHandler TextChanged;
        public event TextHandler TextCopied;
        public event TextHandler TextCut;
        public event TextHandler TextPasted;

        // Keeps track of whether the user has highlighted
        // any text
        private bool _hasSelection;
        // Boolean for when this textbox has focus
        private bool _hasFocus;
        // Boolean for when the control key is held down in order to handle
        // cut/copy/paste events
        private bool _isCtrlPressed = false;

        // The current text layout
        public CanvasTextLayout TextLayout { get; set; }

        // The rectangle representing the cursor
        private RectangleUIElement _cursor;
        // Holds the index in the text string that the cursor is
        // currently located at
        public int CursorCharacterIndex { get; set; }
        // Preserves the x position when moving the cursor up and down
        // in the text box
        private float _currCursorX = 0;

        // Beginning and ending indices of the current highlighted text
        private int _selectionStartIndex = 0;
        private int _selectionEndIndex = 0;

        // Incremented at every update call and used to control how often
        // the cursor blinks
        private int _blinkCounter = 0;
        
        /// <summary>
        /// Models a text box which the user can type into and edit
        /// Inherits from TextboxUIElement
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public EditableTextboxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _hasSelection = false;
            TextVerticalAlignment = CanvasVerticalAlignment.Top;
            Wrapping = CanvasWordWrapping.NoWrap;
            TrimmingSign = CanvasTrimmingSign.None;

            TextLayout = CreateTextLayout(resourceCreator);
            //_textLayout = TextLayout;

            // Get the size the cursor should be
            CursorCharacterIndex = 0;
            CanvasTextLayoutRegion textLayoutRegion;
            TextLayout.GetCaretPosition(CursorCharacterIndex, false, out textLayoutRegion);          
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
            Vector2 pos = new Vector2(pointer.CurrentPoint.X - UIDefaults.XTextPadding - (float)this.Transform.LocalPosition.X,
                                      pointer.CurrentPoint.Y - UIDefaults.YTextPadding - (float)this.Transform.LocalPosition.Y);
            _selectionEndIndex = GetHitIndex(pos);
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
                    if (CursorCharacterIndex == Text.Length)
                    {
                        Text = Text.Remove(CursorCharacterIndex - 1, 1);
                        OnTextChanged(Text);
                    }
                    else
                    {
                        Text = Text.Remove(CursorCharacterIndex, 1);
                        OnTextChanged(Text);
                    }
                    CursorCharacterIndex--;
                } else if ((CursorCharacterIndex == 0) && (Text.Length != 0))
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
                else if (CursorCharacterIndex == Text.Length)
                {
                    CursorCharacterIndex -= 2;
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
                if (CursorCharacterIndex < (Text.Length))
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
                    if (CursorCharacterIndex != Text.Length)
                    {
                        Text = Text.Insert(CursorCharacterIndex + 1, s);
                        OnTextChanged(Text);
                        CursorCharacterIndex++;
                    }
                    else
                    {
                        Text = Text.Insert(CursorCharacterIndex, s);
                        OnTextChanged(Text);
                        CursorCharacterIndex++;
                    }

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
        public void DrawCursor(CanvasDrawingSession ds)
        {
            CanvasTextLayoutRegion textLayoutRegion;
            if (CursorCharacterIndex > -1)
            {
                TextLayout.GetCaretPosition(CursorCharacterIndex, false, out textLayoutRegion);

                Rect bounds = textLayoutRegion.LayoutBounds;

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

        // Update draw method to draw the cursor and highlighted selection
        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            DrawCursor(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            var t1 = Matrix3x2.CreateTranslation(10, 5);
            ds.Transform = ds.Transform * t1;

            // Highlight selected characters
            if (_hasSelection)
            {
                _cursor.IsVisible = false;
                int firstIndex = Math.Min(_selectionStartIndex, _selectionEndIndex);
                int length = Math.Abs(_selectionEndIndex - _selectionStartIndex) + 1;
                CanvasTextLayoutRegion[] descriptions = TextLayout.GetCharacterRegions(firstIndex, length);
                foreach (CanvasTextLayoutRegion description in descriptions)
                {
                    ICanvasBrush b = new CanvasSolidColorBrush(ds, Colors.Black);
                    b.Opacity = 0.25f;
                    ds.FillRectangle(description.LayoutBounds, b);
                }
            }

            ds.Transform = orgTransform;

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

        /// <summary>
        /// Get the character index of a mouse press point 
        /// Used to move cursor
        /// </summary>
        /// <param name="mouseOverPt"></param>
        /// <returns></returns>
        int GetHitIndex(Vector2 mouseOverPt)
        {
            CanvasTextLayoutRegion textLayoutRegion;
            TextLayout.HitTest(
                mouseOverPt.X,
                mouseOverPt.Y,
                out textLayoutRegion);
            return textLayoutRegion.CharacterIndex;
        }

        /// <summary>
        /// Update text layout to the current text
        /// </summary>
        /// <param name="resourceCreator"></param>
        /// <returns></returns>
        private CanvasTextLayout CreateTextLayout(ICanvasResourceCreator resourceCreator)
        {

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

            var textLayout = new CanvasTextLayout(resourceCreator, Text, textFormat, 
                Width - 2 * (BorderWidth + UIDefaults.XTextPadding), 
                Height - 2 * (BorderWidth + UIDefaults.YTextPadding));

            return textLayout;
        }

        /// <summary>
        /// Clear the current selection
        /// </summary>
        private void ClearSelection()
        {
            _hasSelection = false;
            _selectionStartIndex = 0;
            _selectionEndIndex = 0;
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
                CursorCharacterIndex -= (length-1);
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
                Text = Text.Insert(CursorCharacterIndex, text);
                OnTextChanged(Text);
                CursorCharacterIndex += text.Length;
            }
        }

    }
}
