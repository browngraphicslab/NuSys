using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class InkableImageDetailRenderItemUIElement : RectangleUIElement
    {
        private InkableUIElement _inkable;
        private ImageDetailRenderItem _image;
        private ImageElementViewModel _vm;
        public InkableImageDetailRenderItemUIElement(IInkController inkController, ImageElementViewModel vm, BaseRenderItem parent,
            ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _vm = vm;
            _inkable = new InkableUIElement(inkController, this, resourceCreator);
            var imageController = _vm.Controller.LibraryElementController as ImageLibraryElementController;

            _image = new ImageDetailRenderItem(imageController, new Size(_vm.Width, _vm.Height), this, resourceCreator);
            _image.IsRegionsVisible = true;
            _image.IsRegionsModifiable = false;
            _image.IsHitTestVisible = false;
            AddChild(_image);

            _inkable = new InkableUIElement(inkController, this, resourceCreator);
            _inkable.Width = 100.0f;
            _inkable.Height = 100.0f;
            _inkable.Background = Colors.Green;
            AddChild(_inkable);
        }

        public async override Task Load()
        {
            await _image.Load();
            _vm.Controller.SetSize(_vm.Controller.Model.Width, _vm.Controller.Model.Height, false);
            _image.CanvasSize = new Size(_vm.Controller.Model.Width, _vm.Controller.Model.Height);
        }

        private void ControllerOnSizeChanged(object source, double width, double height)
        {
            _image.CanvasSize = new Size(width, height);
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            _image.Dispose();
            _image = null;
            _vm.Controller.SizeChanged -= ControllerOnSizeChanged;
            _vm = null;
            base.Dispose();
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;

            if (_vm == null)
            {
                return;
            }

            ds.Transform = Transform.LocalToScreenMatrix;
            base.Draw(ds);
            ds.Transform = orgTransform;

        }
    }
}
