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
    public class TextElementRenderItem : ElementRenderItem
    {
        private TextNodeViewModel _vm;

        public TextElementRenderItem(TextNodeViewModel vm, ICanvasResourceCreator resourceCreator):base(vm, resourceCreator)
        {
            _vm = vm;
        }

        public override void Dispose()
        {
            base.Dispose();
            _vm = null;
        }

        public async override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            ds.FillRectangle( new Rect {X=_vm.X, Y= _vm.Y, Width = _vm.Width, Height=_vm.Height}, Colors.White);
            
            var f = new CanvasTextFormat();
            f.WordWrapping = CanvasWordWrapping.Wrap;
            f.FontSize = 10;
            if (_vm.Text != null) { 
                var l = new CanvasTextLayout(ResourceCreator, _vm.Text, f, (float)_vm.Width, (float)_vm.Height);
                l.HorizontalAlignment = CanvasHorizontalAlignment.Center;
                ds.DrawTextLayout(l, (float)_vm.X, (float)_vm.Y, Colors.Black);
            }

        }
    }
}
