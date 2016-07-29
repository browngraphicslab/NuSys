using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class ImageElementRenderItem : BaseRenderItem
    {
        private ImageElementViewModel _vm;
        private ICanvasResourceCreator _ds;
        private CanvasBitmap _bmp;

        public ImageElementRenderItem(ImageElementViewModel vm, ICanvasResourceCreator ds)
        {
            _vm = vm;
            _ds = ds;
        }

        public override async Task Load()
        {
            var url = _vm.Controller.LibraryElementController.GetSource();
            _bmp = await CanvasBitmap.LoadAsync(_ds, url);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            ds.DrawText(_vm.Title, new Vector2((float)_vm.X, (float)(_vm.Y-30)), Colors.Black);
            if (_bmp != null)
                ds.DrawImage(_bmp, new Rect { X = _vm.X, Y = _vm.Y, Width = _vm.Width, Height = _vm.Height});
        }
    }
}
