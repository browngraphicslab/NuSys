using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class ElementRenderItem : BaseRenderItem
    {
        private ElementViewModel _vm;
        private static bool _bla;
        private IRandomAccessStream _stream;
        private CanvasBitmap _bmp;
        private ICanvasResourceCreator _ds;

        public ElementRenderItem(ElementViewModel vm, ICanvasResourceCreator ds)
        {
            _vm = vm;
            _ds = ds;
        }     
        
        public override void Draw(CanvasDrawingSession ds)
        {
            var old = ds.Transform;
            ds.FillRectangle(new Rect { X = _vm.X, Y = _vm.Y, Width = _vm.Width, Height = _vm.Height }, Colors.Black);

            var x = old.M31;
            var y = old.M32;
            var newT = Matrix3x2.Identity;
            newT.M31 = x;
            newT.M32 = y;
            ds.Transform = newT;
            ds.DrawText(_vm.Title, new Vector2((float)_vm.X, (float)(_vm.Y-30)), Colors.Black);
            ds.Transform = old;
        }
    }
}
