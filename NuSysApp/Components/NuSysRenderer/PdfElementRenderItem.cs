using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class PdfElementRenderItem : BaseRenderItem
    {
        private PdfNodeViewModel _vm;
        private ICanvasResourceCreator _ds;
        private CanvasBitmap _bmp;

        public PdfElementRenderItem(PdfNodeViewModel vm, ICanvasResourceCreator ds)
        {
            _vm = vm;
            _ds = ds;

            _vm.PropertyChanged += async delegate(object sender, PropertyChangedEventArgs args)
            {
                //_bmp = CanvasBitmap.CreateFromSoftwareBitmap(_vm.SwBitmap)
                if (_bmp == null && args.PropertyName == "ImageSource") { 
                 //   _bmp = await CanvasBitmap.LoadAsync(_ds, _vm.RandomAccessStream);
                    _bmp = CanvasBitmap.CreateFromBytes(_ds, _vm.Buffer, (int)_vm.Width, (int)_vm.Height, DirectXPixelFormat.B8G8R8A8UIntNormalized);
                }
            };
        }

        public override async Task Load()
        {
        //    await _vm.Init();
            
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            ds.DrawText(_vm.Title, new Vector2((float)_vm.X, (float)(_vm.Y-30)), Colors.Black);
            if (_bmp != null)
            {
                ds.DrawImage(_bmp, new Rect {X = _vm.X, Y = _vm.Y, Width = 200, Height = 300});
            }
        }
    }
}
