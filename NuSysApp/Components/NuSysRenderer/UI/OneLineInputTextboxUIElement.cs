using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI;

namespace NuSysApp
{
    class OneLineInputTextboxUIElement : EditableTextboxUIElement
    {
        public string LeftOver { get; set; }

        public OneLineInputTextboxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Wrapping = CanvasWordWrapping.NoWrap;

            this.KeyPressed += OneLine_KeyPressed;
        }

        private void ShiftLeft()
        {

        }

        private void ShiftRight()
        {

        }

        private void OneLine_KeyPressed(Windows.UI.Core.KeyEventArgs args)
        {
            float minw = TextLayout.GetMinimumLineLength();

            float w = Width - 2 * (BorderWidth + UIDefaults.XTextPadding);
            //Debug.WriteLine(minw);
            //if (minw > w)
            //{
            //    Debug.WriteLine("OVER");
            //}

            // if cursor is at left most position and left key is pressed, shift left
            // if cursor is at right most position and right key is pressed, shift right
            // if key is typed and cursor is far right, shift right
            // DON'T LET CURSOR MOVE OUSIDE INDEX RANGE

            // Move cursor left
            if (args.VirtualKey == VirtualKey.Left)
            {
                
            }
            // Move cursor right
            else if (args.VirtualKey == VirtualKey.Right)
            {

            }
        }

        public override void Dispose()
        {
            this.KeyPressed -= OneLine_KeyPressed;

            base.Dispose();
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            //var orgTransform = ds.Transform;
            //ds.Transform = Transform.LocalToScreenMatrix;
            //var t1 = Matrix3x2.CreateTranslation(10, 5);
            //ds.Transform = ds.Transform * t1;

            //ds.DrawRectangle(TextLayout.LayoutBounds, Colors.Red, 2);

            //ds.Transform = orgTransform;
        }
    }
}
