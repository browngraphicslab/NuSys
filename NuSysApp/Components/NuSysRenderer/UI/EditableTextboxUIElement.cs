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

            Text += GetCharsFromKeys(args.VirtualKey, false, false);

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

        [DllImport("user32.dll")]
        private static extern int ToUnicode(uint virtualKeyCode, uint scanCode,
            byte[] keyboardState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)]
            StringBuilder receivingBuffer,
            int bufferSize, uint flags);

        static string GetCharsFromKeys(VirtualKey keys, bool shift, bool altGr)
        {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            if (shift)
                keyboardState[(int)VirtualKey.Shift] = 0xff;
            if (altGr)
            {
                keyboardState[(int)VirtualKey.Control] = 0xff;
                keyboardState[(int)VirtualKey.Menu] = 0xff;
            }
            ToUnicode((uint)keys, 0, keyboardState, buf, 256, 0);
            return buf.ToString();
        }
    }
}
