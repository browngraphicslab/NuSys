using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class ElementRenderItem : BaseRenderItem
    {
        private ElementViewModel _vm;
        private CanvasBitmap bmp;

        public ElementRenderItem(ElementViewModel vm)
        {
            _vm = vm;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            ds.DrawText(_vm.Title, new Vector2((float)_vm.X, (float)(_vm.Y-30)), Colors.Black);
            ds.FillRectangle( new Rect {X=_vm.X, Y= _vm.Y, Width = _vm.Width, Height=_vm.Height}, Colors.Black);
        }
    }
}
