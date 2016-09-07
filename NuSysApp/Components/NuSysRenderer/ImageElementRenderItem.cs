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
        private ImageLibraryElementController _controller;
        private CanvasBitmap _bmp;
        private Rect _srcRect;
        private bool _isCropping;

        public ImageElementRenderItem(ImageElementViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) :base(vm, parent, resourceCreator)
        {
            _vm = vm;
           _controller = (_vm.Controller.LibraryElementController as ImageLibraryElementController);
            _controller.LocationChanged += LibOnLocationChanged;
            _controller.SizeChanged += LibOnSizeChanged;
        }
        public override void Dispose()
        {
            _vm = null;
            _bmp.Dispose();
            _bmp = null;
            _controller.LocationChanged -= LibOnLocationChanged;
            _controller.SizeChanged -= LibOnSizeChanged;
            _controller = null;
            base.Dispose();
        }

        private void LibOnSizeChanged(object sender, double width, double height)
        {
            Crop();
        }

        private void LibOnLocationChanged(object sender, Point topLeft)
        {
            Crop();
        }

        public override async Task Load()
        {
            var url = _vm.Controller.LibraryElementController.ContentDataController.ContentDataModel.Data;
            _bmp = await CanvasBitmap.LoadAsync(ResourceCreator, new Uri(url), ResourceCreator.Dpi);
            _vm.ImageSize = _bmp.Size;

          //  var ratio = (double)_bmp.Size.Width / (double)_bmp.Size.Height;
          //  _vm.Controller.SetSize(_vm.Controller.Model.Width, _vm.Controller.Model.Width * ratio, false);
            Crop();
        }

        private void Crop()
        {
            _isCropping = true;
               var lib = (_vm.Controller.LibraryElementModel as ImageLibraryElementModel);
            var nx = lib.NormalizedX * _bmp.Size.Width;
            var ny = lib.NormalizedY * _bmp.Size.Height;
            var nw = lib.NormalizedWidth * _bmp.Size.Width;
            var nh = lib.NormalizedHeight * _bmp.Size.Height;
            _srcRect = new Rect(nx, ny, nw, nh);
            var ratio = nw/nh;
            _vm.Controller.SetSize(_vm.Height * ratio, _vm.Height, false);
            _isCropping = false;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;

            base.Draw(ds);

            if (_vm == null )
                return;
          
            ds.Transform = Win2dUtil.Invert(C) * S * C * T * ds.Transform;
            ds.FillRectangle(new Rect { X = 0, Y = 0, Width = _vm.Width, Height = _vm.Height }, Colors.Red);

            if (_bmp != null)
                ds.DrawImage(_bmp, new Rect { X = 0, Y = 0, Width = _vm.Width, Height = _vm.Height}, _srcRect);

            ds.Transform = orgTransform;
        }
    }
}
