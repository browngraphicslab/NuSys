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
    public class AudioElementRenderItem : ElementRenderItem
    {
        private AudioNodeViewModel _vm;
        private CanvasBitmap _bmp;

        public AudioElementRenderItem(AudioNodeViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) :base(vm, parent, resourceCreator)
        {
            _vm = vm;

        }

        public override async Task Load()
        {
            var url = _vm.Controller.LibraryElementController.LargeIconUri;
            _bmp = await CanvasBitmap.LoadAsync(ResourceCreator, url, ResourceCreator.Dpi);
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            base.Dispose();
            _vm = null;
            _bmp.Dispose();
            _bmp = null;
        }


        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;
            
            base.Draw(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Win2dUtil.Invert(C) * S * C * T * ds.Transform;
            //   ds.FillRectangle(new Rect { X = 0, Y = 0, Width = _vm.Width, Height = _vm.Height }, Colors.Black);
            if (_bmp != null)
                ds.DrawImage(_bmp, new Rect { X = 0, Y = 0, Width = _vm.Width, Height = _vm.Height });
            ds.Transform = orgTransform;
        }
    }
}
