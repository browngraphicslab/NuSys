using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using SharpDX.DirectWrite;

namespace NuSysApp
{
    public class TextElementRenderItem : BaseRenderItem
    {
        private TextNodeViewModel _vm;
        private IRandomAccessStream _stream;
        private ICanvasResourceCreator _ds;

        public TextElementRenderItem(TextNodeViewModel vm, ICanvasResourceCreator ds)
        {
            _vm = vm;
            _ds = ds;
        }

        public async override void Draw(CanvasDrawingSession ds)
        {
            var to = Matrix3x2.CreateTranslation(new Vector2((float) _vm.X, (float) (_vm.Y - 30)));
            var top = Matrix3x2.Identity;
            Matrix3x2.Invert(to, out top);


            var old = ds.Transform;
            var x = old.M31;
            var y = old.M32;
            var sp = Matrix3x2.Identity;
            Matrix3x2.Invert(NuSysRenderer.S, out sp);
            var tt = Matrix3x2.CreateTranslation(0, -30);
            var newT = tt * top * sp * to *ds.Transform;

            ds.Transform = newT;

            var target = Vector2.Transform(new Vector2((float) _vm.X, (float) (_vm.Y)), newT);

            ds.DrawText(_vm.Title, new Vector2((float)_vm.X, (float)(_vm.Y - 30)), Colors.Black);
            ds.Transform = old;

            //ds.DrawText(_vm.Title, new Vector2((float)_vm.X, (float)(_vm.Y-30)), Colors.Black);

           ds.FillRectangle( new Rect {X=_vm.X, Y= _vm.Y, Width = _vm.Width, Height=_vm.Height}, Colors.White);
            
            var f = new CanvasTextFormat();
            f.WordWrapping = CanvasWordWrapping.Wrap;
            f.FontSize = 12;
            if (_vm.Text != null) { 
                var l = new CanvasTextLayout(_ds, _vm.Text, f, (float)_vm.Width, (float)_vm.Height);
                ds.DrawTextLayout(l, (float)_vm.X, (float)_vm.Y, Colors.Black);
            }

        }
    }
}
