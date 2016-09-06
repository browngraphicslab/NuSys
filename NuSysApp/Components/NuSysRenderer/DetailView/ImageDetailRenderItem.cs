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
        private List<ImageDetailRegionRenderItem> _regions = new List<ImageDetailRegionRenderItem>();

        public delegate void RegionUpdatedHandler();
        public event RegionUpdatedHandler NeedsRedraw;

        public ImageDetailRenderItem(ImageLibraryElementController controller, Size maxSize, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _controller = controller;
            _maxSize = maxSize;

            controller.ContentDataController.ContentDataModel.OnRegionAdded += ContentDataModelOnOnRegionAdded;
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
            var model = SessionController.Instance.ContentController.GetLibraryElementModel(regionLibraryElementModelId);
            ComputeRegions();
        }

        public override async Task Load()
        {
            var url = _controller.ContentDataController.ContentDataModel.Data;
            _bmp = await CanvasBitmap.LoadAsync(ResourceCreator, new Uri(url), ResourceCreator.Dpi);
            if (_bmp.Size.Width > _bmp.Size.Height)
            {
                _targetRect.Width = _maxSize.Width;
                _targetRect.Height = _targetRect.Width*1/_controller.ImageLibraryElementModel.Ratio;
            }
            else
            {
                _targetRect.Height = _maxSize.Height;
                _targetRect.Width = _targetRect.Height * _controller.ImageLibraryElementModel.Ratio;
            }

            _targetRect.X = (_maxSize.Width - _targetRect.Width)/2;
            _targetRect.Y = (_maxSize.Height - _targetRect.Height)/2;

            ComputeRegions();
            
        }

        private void ComputeRegions()
        {
            var others = SessionController.Instance.ContentController.AllLibraryElementModels.Where(l => l.ContentDataModelId == _controller.ContentDataController.ContentDataModel.ContentId && l.LibraryElementId != _controller.LibraryElementModel.LibraryElementId).Cast<ImageLibraryElementModel>();
            foreach (var l in others)
            {
                var rect = new Rect(0,0, l.NormalizedWidth * _targetRect.Width, l.NormalizedHeight*_targetRect.Height );
                var region = new ImageDetailRegionRenderItem(l, rect, _targetRect, this, ResourceCreator);
                region.T = Matrix3x2.CreateTranslation((float)(_targetRect.X + l.NormalizedX * _targetRect.Width), (float)(_targetRect.Y + l.NormalizedY * _targetRect.Height));
                region.RegionUpdated += RegionOnRegionUpdated;
                
                Children.Add(region);
            }
        }

        private void RegionOnRegionUpdated(ImageDetailRegionRenderItem region)
        {
            NeedsRedraw?.Invoke();
            var screenRect = new Rect(region.T.M31 - _targetRect.X, region.T.M32 - _targetRect.Y,region.GetMeasure().Width, region.GetMeasure().Height);
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(region.LibraryElementModel.LibraryElementId) as ImageLibraryElementController;
            controller.SetXLocation(screenRect.X / _targetRect.Width);
            controller.SetYLocation(screenRect.Y / _targetRect.Height);
            controller.SetWidth(screenRect.Width / _targetRect.Width);
            controller.SetHeight(screenRect.Height / _targetRect.Height);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
           
            var orgTransform = ds.Transform;
            ds.Transform = GetTransform()*orgTransform;
            if (_bmp != null)
                ds.DrawImage(_bmp, _targetRect);

            base.Draw(ds);

            ds.Transform = orgTransform;


        }
    }
}
