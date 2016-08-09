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
        private HTMLParser _htmlParser;

        public TextElementRenderItem(TextNodeViewModel vm, CollectionRenderItem parent, CanvasAnimatedControl resourceCreator):base(vm, parent, resourceCreator)
        {
            _vm = vm;
            _htmlParser = new HTMLParser(resourceCreator);
        }

        public override void Dispose()
        {
            base.Dispose();
            _vm = null;
        }

        public async override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Win2dUtil.Invert(C) * S * C * T * ds.Transform;

            ds.FillRectangle( new Rect {X = 0, Y = 0, Width = _vm.Width, Height=_vm.Height}, Colors.White);

            var textLayout = _htmlParser.GetParsedText(_vm.Text, _vm.Height, _vm.Width);
            textLayout.HorizontalAlignment = CanvasHorizontalAlignment.Center;
            ds.DrawTextLayout(textLayout, (float)_vm.X, (float)_vm.Y, Colors.Black);
            /*
            var f = new CanvasTextFormat();
            f.WordWrapping = CanvasWordWrapping.Wrap;
            f.FontSize = 10;
            if (_vm.Text != null) { 
                var l = new CanvasTextLayout(ResourceCreator, _vm.Text, f, (float)_vm.Width, (float)_vm.Height);
                l.HorizontalAlignment = CanvasHorizontalAlignment.Center;
                ds.DrawTextLayout(l, (float)_vm.X, (float)_vm.Y, Colors.Black);
            }
            */

            ds.Transform = orgTransform;

        }
    }
}
