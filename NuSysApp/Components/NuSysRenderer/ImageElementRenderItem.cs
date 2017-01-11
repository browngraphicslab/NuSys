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
        private ImageDetailRenderItem _image;
        private InkableUIElement _inkable;

        public ImageElementRenderItem(ImageElementViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) :base(vm, parent, resourceCreator)
        {
            _vm = vm;
            _vm.Controller.SizeChanged += ControllerOnSizeChanged;
            var imageController = _vm.Controller.LibraryElementController as ImageLibraryElementController;
            _image = new ImageDetailRenderItem(imageController, new Size(_vm.Width, _vm.Height), this, resourceCreator);
            _image.IsRegionsVisible = true;
            _image.IsRegionsModifiable = false;
            _image.IsHitTestVisible = false;

            AddChild(_image);

            _inkable = new InkableUIElement(imageController.ContentDataController, this, resourceCreator);
            _inkable.Background = Colors.Transparent;
            _inkable.BorderWidth = 2.0f;
            _inkable.Bordercolor = Colors.Green;
            AddChild(_inkable);
            _inkable.Transform.SetParent(_image.Transform);
        }

        public async override Task Load()
        {
            await _image.Load();
            _vm.Controller.SetSize(_vm.Controller.Model.Width, _vm.Controller.Model.Height, false);
            _image.CanvasSize = new Size(_vm.Controller.Model.Width, _vm.Controller.Model.Height);
        }

        private void ControllerOnSizeChanged(object source, double width, double height)
        {
           _image.CanvasSize = new Size(width,height);
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

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _inkable.Width = (float) _image.CroppedImageTarget.Width;
            _inkable.Height = (float)_image.CroppedImageTarget.Height;
            _inkable.Transform.LocalPosition = _image.Transform.LocalPosition;
            base.Update(parentLocalToScreenTransform);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;

            if (_vm == null )
                return;

           

            ds.Transform = Transform.LocalToScreenMatrix;
            base.Draw(ds);
            ds.Transform = orgTransform;

        }
    }
}
