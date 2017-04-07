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
        public int CurrentPage;

        private PdfDetailRenderItem _image;
        private PdfLibraryElementController _pdfLibraryElementController;
        private InkableUIElement _inkable;

        public PdfElementRenderItem(PdfNodeViewModel vm, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator):base(vm, parent, resourceCreator)
        {
            _vm = vm;
            _vm.Controller.SizeChanged += ControllerOnSizeChanged;
            _pdfLibraryElementController = _vm.Controller.LibraryElementController as PdfLibraryElementController;
            _image = new PdfDetailRenderItem(_pdfLibraryElementController, new Size(_vm.Width, _vm.Height), this, resourceCreator);
            _image.IsRegionsVisible = true;
            _image.IsRegionsModifiable = false;
            _image.IsHitTestVisible = false;

            AddChild(_image);

            _inkable = new InkableUIElement(_pdfLibraryElementController, this, resourceCreator);
            _inkable.Background = Colors.Transparent;
            AddChild(_inkable);
            _inkable.Transform.SetParent(_image.Transform);
        }

        private void ControllerOnSizeChanged(object source, double width, double height)
        {
            _image.CanvasSize = new Size(width, height);
        }

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            _vm.Controller.SizeChanged -= ControllerOnSizeChanged;
            _image.Dispose();
            _image = null;
            _vm.Dispose();
            _vm = null;
            _pdfLibraryElementController = null;

            base.Dispose();
        }

        public async Task GotoPage(int page)
        {
            (ResourceCreator as CanvasAnimatedControl).RunOnGameLoopThreadAsync(async () => {
                var content = _vm.Controller.LibraryElementController.ContentDataController.ContentDataModel as PdfContentDataModel;
                if (page < 0)
                {
                    CurrentPage = 0;
                }
                else if (page > content.PageUrls.Count - 1)
                {
                    CurrentPage = content.PageUrls.Count - 1;
                }
                else
                {
                    CurrentPage = page;
                }

                _image.CurrentPage = CurrentPage;
                _image.ImageUrl = content.PageUrls[CurrentPage];
                await _image.Load();
        });
    }

        public override async Task Load()
        {
            await GotoPage(_pdfLibraryElementController.PdfLibraryElementModel.PageStart);
            return;
            await _image.Load();
            _vm.Controller.SetSize(_vm.Controller.Model.Width, _vm.Controller.Model.Height, false);
            _image.CanvasSize = new Size(_vm.Controller.Model.Width, _vm.Controller.Model.Height);

            
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_image.CroppedImageTarget.Width >= 0.0)
            {
                _inkable.Width = (float)_image.CroppedImageTarget.Width;
            }
            if (_image.CroppedImageTarget.Height >= 0.0)
            {
                _inkable.Height = (float)_image.CroppedImageTarget.Height;
            }
            _inkable.Transform.LocalPosition = _image.Transform.LocalPosition;
            base.Update(parentLocalToScreenTransform);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            base.Draw(ds);
            ds.Transform = orgTransform;

        }
    }
}
