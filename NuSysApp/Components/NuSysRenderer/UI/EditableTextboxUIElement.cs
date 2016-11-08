using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;

namespace NuSysApp
{
    public class EditableTextboxUIElement : TextboxUIElement
    {     

        private RectangleUIElement _cursor;

        public EditableTextboxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            //// set default values
            //TextHorizontalAlignment = UIDefaults.TextHorizontalAlignment;
            TextVerticalAlignment = CanvasVerticalAlignment.Top;
            Wrapping = CanvasWordWrapping.WholeWord;
            //TextColor = UIDefaults.TextColor;
            //FontStyle = UIDefaults.FontStyle;
            //FontSize = UIDefaults.FontSize;
            //FontFamily = UIDefaults.FontFamily;
            //Wrapping = UIDefaults.Wrapping;
            //TrimmingSign = UIDefaults.TrimmingSign;
            //TrimmingGranularity = UIDefaults.TrimmingGranularity;
            //BorderWidth = 0;

            //_cursor.Width = 3;
            //_cursor.Height = FontSize;

            //this.Tapped += EditableTextboxUIElement_Tapped;
            this.OnFocusGained += EditableTextboxUIElement_OnFocusGained;
            this.OnFocusLost += EditableTextboxUIElement_OnFocusLost;
            this.KeyPressed += EditableTextboxUIElement_KeyPressed;


        }

        private void EditableTextboxUIElement_KeyPressed(Windows.UI.Core.KeyEventArgs args)
        {
            // if is in focus, add typed key to the current cursor location in string
            // otherwise check for arrow keys to move cursor, enter to make newline, backspace/delete
            // to remove characters (tab functionality?)

            //Text += GetCharsFromKeys(args.VirtualKey, false, false);
            if (args.VirtualKey == VirtualKey.Back)
            {
                Text = Text.Remove(Text.Length - 1);
            } else
            {
                Text += KeyCodeToUnicode(args.VirtualKey);
            }
        }

        private void EditableTextboxUIElement_OnFocusLost(BaseRenderItem item)
        {
            // hide blinking cursor
            BorderWidth = 0;
        }

        private void EditableTextboxUIElement_OnFocusGained(BaseRenderItem item)
        {
            // show blinking cursor at end of text
            BorderWidth = 2;

        }

        private void EditableTextboxUIElement_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // if it is in focus, move cursor to location that is tapped
            
        }

        /// <summary>
        /// Draws the text within the textbox
        /// </summary>
        /// <param name="ds"></param>
        public void DrawText(CanvasDrawingSession ds)
        {
            // save the current transform of the drawing session
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            base.DrawText(ds);

            _cursor.Height = FontSize;

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

            ds.Transform = orgTransform;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {

        }

        public override void Dispose()
        {
            this.Tapped -= EditableTextboxUIElement_Tapped;
            this.OnFocusGained -= EditableTextboxUIElement_OnFocusGained;
            this.OnFocusLost -= EditableTextboxUIElement_OnFocusLost;

            base.Dispose();
        }

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
