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
        public int CurrentPage;
        private bool _isUpdating;
        private ImageDetailRenderItem _image;
        private PdfLibraryElementController _pdfLibraryElementController;

        public PdfElementRenderItem(PdfNodeViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator):base(vm, parent, resourceCreator)
        {
            _vm = vm;
            _vm.Controller.SizeChanged += ControllerOnSizeChanged;
            _pdfLibraryElementController = _vm.Controller.LibraryElementController as PdfLibraryElementController;
            _image = new ImageDetailRenderItem(_pdfLibraryElementController, new Size(_vm.Width, _vm.Height), this, resourceCreator);
            _image.IsRegionsVisible = true;
            _image.IsRegionsModifiable = false;

        }

        private void ControllerOnSizeChanged(object source, double width, double height)
        {
            _image.CanvasSize = new Size(width, height);
        }

        public override void Dispose()
        {
            _vm = null;
            base.Dispose();
        }

        public async void GotoPage(int page)
        {
            _isUpdating = true;
            var content = _vm.Controller.LibraryElementController.ContentDataController.ContentDataModel as PdfContentDataModel;
            if (page < 0)
            {
                CurrentPage = 0;
            } else if (page > content.PageUrls.Count - 1)
            {
                CurrentPage = content.PageUrls.Count - 1;
            }
            else
            {
                CurrentPage = page;
            }

            _image.ImageUrl = content.PageUrls[CurrentPage];
            await _image.Load();
            
            _bmp = await CanvasBitmap.LoadAsync(ResourceCreator, new Uri(content.PageUrls[CurrentPage]), ResourceCreator.Dpi);
            _vm.ImageSize = _bmp.Size;

            var ratio = (double)_bmp.Size.Width / (double)_bmp.Size.Height;
            if (Math.Abs(_vm.Width/_vm.Height) -1 > 0.001)
                _vm.Controller.SetSize(_vm.Width, _vm.Height* ratio, false);

            _isUpdating = false;
        }

        public override async Task Load()
        {
            await _image.Load();
            _vm.Controller.SetSize(_vm.Controller.Model.Width, _vm.Controller.Model.Height, false);
            _image.CanvasSize = new Size(_vm.Controller.Model.Width, _vm.Controller.Model.Height);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            if (_isUpdating || _vm == null)
                return;

            ds.Transform = GetTransform() * ds.Transform;
            _image.Draw(ds);

            ds.Transform = orgTransform;
            base.Draw(ds);
            ds.Transform = orgTransform;

        }
    }
}
