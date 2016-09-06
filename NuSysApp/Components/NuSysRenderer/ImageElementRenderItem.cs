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
using NusysIntermediate;

namespace NuSysApp
{
    public class ImageElementRenderItem : ElementRenderItem
    {
        private ImageElementViewModel _vm;
        private CanvasBitmap _bmp;
        private CanvasRenderTarget _renderTarget;

        public ImageElementRenderItem(ImageElementViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) :base(vm, parent, resourceCreator)
        {
            _vm = vm;
        }

        public override void Dispose()
        {
            _vm = null;
            _bmp.Dispose();
            _bmp = null;
            base.Dispose();
        }

        public override async Task Load()
        {
            var url = _vm.Controller.LibraryElementController.ContentDataController.ContentDataModel.Data;
            _bmp = await CanvasBitmap.LoadAsync(ResourceCreator, new Uri(url), ResourceCreator.Dpi);
            _vm.ImageSize = _bmp.Size;

            

            var ratio = (double)_bmp.Size.Width / (double)_bmp.Size.Height;
            _vm.Controller.SetSize(_vm.Controller.Model.Width, _vm.Controller.Model.Width * ratio, false);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (_bmp != null && _renderTarget == null)
            {


                var lib = (_vm.Controller.LibraryElementModel as ImageLibraryElementModel);
                var newRatio = lib.Ratio*(lib.NormalizedWidth/lib.NormalizedHeight);
                var nx = lib.NormalizedX*_bmp.Size.Width;
                var ny = lib.NormalizedY*_bmp.Size.Height;
                var nw = lib.NormalizedWidth*_bmp.Size.Width;
                var nh = lib.NormalizedHeight*_bmp.Size.Height;
                var dstRect = new Rect(nx, ny, nw, nh);
                _renderTarget = new CanvasRenderTarget(ResourceCreator, (float)nw, (float)nh);
                using (var dss = _renderTarget.CreateDrawingSession())
                {
                    dss.DrawImage(_bmp,0,0, dstRect);
                }
                var ratio = (double)nw / (double)nh;
                _vm.Controller.SetSize(_vm.Controller.Model.Height * ratio, _vm.Controller.Model.Height, false);
            }

            base.Draw(ds);
            var orgTransform = ds.Transform;
            ds.Transform = Win2dUtil.Invert(C) * S * C * T * ds.Transform;
            ds.FillRectangle(new Rect { X = 0, Y = 0, Width = _vm.Width, Height = _vm.Height }, Colors.Red);

            if (_renderTarget != null)
                ds.DrawImage(_renderTarget, new Rect { X = 0, Y = 0, Width = _vm.Width, Height = _vm.Height});

            ds.Transform = orgTransform;
        }
    }
}
