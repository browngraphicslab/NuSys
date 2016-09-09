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

        public ImageElementRenderItem(ImageElementViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) :base(vm, parent, resourceCreator)
        {
            _vm = vm;
            _vm.Controller.SizeChanged += ControllerOnSizeChanged;
            var imageController = _vm.Controller.LibraryElementController as ImageLibraryElementController;
            _image = new ImageDetailRenderItem(imageController, new Size(_vm.Width, _vm.Height), this, resourceCreator);
            _image.IsRegionsVisible = true;
            _image.IsRegionsModifiable = false;
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
            _vm.Controller.SizeChanged -= ControllerOnSizeChanged;
            _vm = null;
            _image.Dispose();
            _image = null;
            base.Dispose();
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;

            if (_vm == null )
                return;
          
            ds.Transform = GetTransform() * ds.Transform;
            _image.Draw(ds);

            ds.Transform = orgTransform;
            base.Draw(ds);
            ds.Transform = orgTransform;


        }
    }
}
