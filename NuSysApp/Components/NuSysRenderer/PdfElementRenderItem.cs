using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using NusysIntermediate;

namespace NuSysApp
{
    public class PdfElementRenderItem : ElementRenderItem
    {
        private PdfNodeViewModel _vm;
        private CanvasBitmap _bmp;

        public PdfElementRenderItem(PdfNodeViewModel vm, CollectionRenderItem parent, CanvasAnimatedControl resourceCreator):base(vm, parent, resourceCreator)
        {
            _vm = vm;

        }

        public override void Dispose()
        {
            base.Dispose();
            _vm = null;
            _bmp.Dispose();
            _bmp = null;
        }

        public override async Task Load()
        {
            var content = _vm.Controller.LibraryElementController.ContentDataController.ContentDataModel as PdfContentDataModel;
            _bmp = await CanvasBitmap.LoadAsync(ResourceCreator, new Uri(content.PageUrls[0]), ResourceCreator.Dpi);
            _vm.ImageSize = _bmp.Size;
            var ratio = (double)_bmp.Size.Width / (double)_bmp.Size.Height;
            _vm.Controller.SetSize(_vm.Controller.Model.Width, _vm.Controller.Model.Width * ratio, false);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);
            var orgTransform = ds.Transform;
            ds.Transform = Win2dUtil.Invert(C) * S * C * T * ds.Transform;
            ds.FillRectangle(new Rect { X = 0, Y = 0, Width = _vm.Width, Height = _vm.Height }, Colors.Red);

            if (_bmp != null)
                ds.DrawImage(_bmp, new Rect { X = 0, Y = 0, Width = _vm.Width, Height = _vm.Height });

            ds.Transform = orgTransform;
        }
    }
}
