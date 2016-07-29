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
        private CanvasBitmap _bmp;
        private ICanvasResourceCreator _ds;
        private CanvasGeometry _geometry;

        public TextElementRenderItem(TextNodeViewModel vm, ICanvasResourceCreator ds)
        {
            _vm = vm;
            _ds = ds;
            _geometry = CanvasGeometry.CreateRectangle(_ds, new Rect { X = _vm.X, Y = _vm.Y, Width = _vm.Width, Height = _vm.Height });
            
        }

        public async override void Draw(CanvasDrawingSession ds)
        {
            ds.DrawText(_vm.Title, new Vector2((float)_vm.X, (float)(_vm.Y-30)), Colors.Black);
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

    sealed class WebviewRenderer
    {
        private static volatile WebviewRenderer instance;
        private static object syncRoot = new Object();

        private WebviewRenderer() { }

        private BlockingQueue<TextElementRenderItem> _queue = new BlockingQueue<TextElementRenderItem>(100); 

        public static WebviewRenderer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new WebviewRenderer();
                    }
                }

                return instance;
            }
        }
    }
}
