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
        private Rect _cropRegion;
        private bool _isCropping;

        public ImageElementRenderItem(ImageElementViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) :base(vm, parent, resourceCreator)
        {
            _vm = vm;
            _vm.Controller.SizeChanged += ControllerOnSizeChanged;
            _controller = (_vm.Controller.LibraryElementController as ImageLibraryElementController);
            _controller.ContentDataController.ContentDataModel.OnRegionAdded += ContentDataModelOnOnRegionAdded;
            _controller.ContentDataController.ContentDataModel.OnRegionRemoved += ContentDataModelOnOnRegionRemoved;
            _controller.LocationChanged += LibOnLocationChanged;
            _controller.SizeChanged += LibOnSizeChanged;
        }

        private void ContentDataModelOnOnRegionRemoved(string regionLibraryElementModelId)
        {
            ComputeRegions();
        }

        private async Task ContentDataModelOnOnRegionAdded(string regionLibraryElementModelId)
        {
            ComputeRegions();
        }

        private void ControllerOnSizeChanged(object source, double width, double height)
        {
            foreach (var child in Children)
            {
                var region = child as ImageDetailRegionRenderItem;
                region?.UpdateImageBound();
            }
        }

        public override void Dispose()
        {
            _vm.Controller.SizeChanged -= ControllerOnSizeChanged;
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

            Crop();
            ComputeRegions();
        }

        private void Crop()
        {
            _isCropping = true;
            var lib = (_vm.Controller.LibraryElementModel as ImageLibraryElementModel);
            var nx = lib.NormalizedX * _bmp.Size.Width;
            var ny = lib.NormalizedY * _bmp.Size.Height;
            var nw = lib.NormalizedWidth * _bmp.Size.Width;
            var nh = lib.NormalizedHeight * _bmp.Size.Height;
            _cropRegion = new Rect(nx, ny, nw, nh);
            var ratio = nw/nh;
            _vm.Controller.SetSize(_vm.Height * ratio, _vm.Height, false);
            _isCropping = false;
        }

        private void ComputeRegions()
        {
            var children = Children.ToArray();
            Children.Clear();
            foreach (var child in children)
            {
                child.Dispose();
            }

      
            var others = SessionController.Instance.ContentController.AllLibraryElementModels.Where(l => l.ContentDataModelId == _controller.ContentDataController.ContentDataModel.ContentId && l.LibraryElementId != _controller.LibraryElementModel.LibraryElementId).Cast<ImageLibraryElementModel>();
            foreach (var regionLibraryElementModel in others)
            {
                var elementBounds = new Rect(0,0,_vm.Width, _vm.Height);
           //     var region = new ImageDetailRegionRenderItem(regionLibraryElementModel, _bmp.Bounds, _bmp.Bounds, _cropRegion.Width/_bmp.Size.Width, this, ResourceCreator, false);
            //    Children.Add(region);
            }
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;

            if (_vm == null )
                return;
          
            ds.Transform = GetTransform() * ds.Transform;
            ds.FillRectangle(new Rect { X = 0, Y = 0, Width = _vm.Width, Height = _vm.Height }, Colors.Red);

            if (_bmp != null)
                ds.DrawImage(_bmp, new Rect { X = 0, Y = 0, Width = _vm.Width, Height = _vm.Height}, _cropRegion);

            ds.Transform = orgTransform;
        //    base.Draw(ds);

            ds.Transform = orgTransform;


        }
    }
}
