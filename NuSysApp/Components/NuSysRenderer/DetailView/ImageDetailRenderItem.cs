using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class ImageDetailRenderItem : BaseRenderItem
    {
        private ImageLibraryElementController _controller;
        private CanvasBitmap _bmp;
        private Rect _targetRect = new Rect();
        private Size _maxSize;
        private Rect _srcRect;
        private List<ImageDetailRegionRenderItem> _regions = new List<ImageDetailRegionRenderItem>();

        public delegate void RegionUpdatedHandler();
        public event RegionUpdatedHandler NeedsRedraw;

        public ImageDetailRenderItem(ImageLibraryElementController controller, Size maxSize, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _controller = controller;
            _maxSize = maxSize;

            controller.ContentDataController.ContentDataModel.OnRegionAdded += ContentDataModelOnOnRegionAdded;
            controller.ContentDataController.ContentDataModel.OnRegionRemoved += ContentDataModelOnOnRegionRemoved;
        }

        public override void Dispose()
        {
            _controller.ContentDataController.ContentDataModel.OnRegionAdded -= ContentDataModelOnOnRegionAdded;
            foreach (var child in Children)
            {
                var region = child as ImageDetailRegionRenderItem;
                region.RegionUpdated -= RegionOnRegionUpdated;
                region?.Dispose();
            }
        }

        private async Task ContentDataModelOnOnRegionAdded(string regionLibraryElementModelId)
        {
            ComputeRegions();
        }
        private void ContentDataModelOnOnRegionRemoved(string regionLibraryElementModelId)
        {
            ComputeRegions();
        }

        public override async Task Load()
        {
            var url = _controller.ContentDataController.ContentDataModel.Data;
            _bmp = await CanvasBitmap.LoadAsync(ResourceCreator, new Uri(url), ResourceCreator.Dpi);

            var lib = (_controller.LibraryElementModel as ImageLibraryElementModel);
            var nx = lib.NormalizedX * _bmp.Size.Width;
            var ny = lib.NormalizedY * _bmp.Size.Height;
            var nw = lib.NormalizedWidth * _bmp.Size.Width;
            var nh = lib.NormalizedHeight * _bmp.Size.Height;
            _srcRect = new Rect(nx, ny, nw, nh);
            var ratio = nw/nh;


            if (_srcRect.Width > _srcRect.Height)
            {
                _targetRect.Width = _maxSize.Width;
                _targetRect.Height = _targetRect.Width*1/ ratio;
            }
            else
            {
                _targetRect.Height = _maxSize.Height;
                _targetRect.Width = _targetRect.Height * ratio;
            }

            _targetRect.X = (_maxSize.Width - _targetRect.Width)/2;
            _targetRect.Y = (_maxSize.Height - _targetRect.Height)/2;

            ComputeRegions();
            
        }

        private void ComputeRegions()
        {
            var children = Children.ToArray();
            Children.Clear();
            foreach (var child in children)
            {
                var region = child as ImageDetailRegionRenderItem;
                region.RegionUpdated -= RegionOnRegionUpdated;
                region?.Dispose();
            }

            var others = SessionController.Instance.ContentController.AllLibraryElementModels.Where(l => l.ContentDataModelId == _controller.ContentDataController.ContentDataModel.ContentId).Cast<ImageLibraryElementModel>();
            others = others.Where(l => l.LibraryElementId != _controller.LibraryElementModel.LibraryElementId);
            foreach (var l in others)
            {
                var region = new ImageDetailRegionRenderItem(l, _targetRect, _targetRect, this, ResourceCreator);
                region.RegionUpdated += RegionOnRegionUpdated;
                
                Children.Add(region);
            }

            NeedsRedraw?.Invoke();
        }

        private void RegionOnRegionUpdated(ImageDetailRegionRenderItem region)
        {
            
            var screenRect = new Rect(region.T.M31 - _targetRect.X, region.T.M32 - _targetRect.Y,region.GetMeasure().Width, region.GetMeasure().Height);
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(region.LibraryElementModel.LibraryElementId) as ImageLibraryElementController;
            controller.SetXLocation(screenRect.X / _targetRect.Width);
            controller.SetYLocation(screenRect.Y / _targetRect.Height);
            controller.SetWidth(screenRect.Width / _targetRect.Width);
            controller.SetHeight(screenRect.Height / _targetRect.Height);
            NeedsRedraw?.Invoke();
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = GetTransform()*orgTransform;
            if (_bmp != null)
                ds.DrawImage(_bmp, _targetRect, _srcRect);

            base.Draw(ds);

            ds.Transform = orgTransform;
        }
    }
}
