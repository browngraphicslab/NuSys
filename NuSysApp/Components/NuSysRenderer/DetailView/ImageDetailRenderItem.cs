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
using Microsoft.Graphics.Canvas.Geometry;
using NusysIntermediate;

namespace NuSysApp
{
    public class ImageDetailRenderItem : BaseRenderItem
    {
        private ImageLibraryElementController _controller;
        private CanvasBitmap _bmp;
        private Rect _croppedImageTarget;
        private Rect _rectToCropFromContent;
        private Rect _normalizedCroppedRect;
        private double _scaleOrgToDisplay;
        private double _scaleDisplayToCrop;
        private CanvasGeometry _mask;
        private Size _canvasSize;
        private bool _needsMaskRefresh;

        public bool IsRegionsVisible { get; set; }
        public bool IsRegionsModifiable { get; set; }

        public delegate void RegionUpdatedHandler();
        public event RegionUpdatedHandler NeedsRedraw;

        public ImageDetailRenderItem(ImageLibraryElementController controller, Size maxSize, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _controller = controller;
            _canvasSize = maxSize;

            controller.ContentDataController.ContentDataModel.OnRegionAdded += ContentDataModelOnOnRegionAdded;
            controller.ContentDataController.ContentDataModel.OnRegionRemoved += ContentDataModelOnOnRegionRemoved;
            
        }

        public override void Dispose()
        {
            _controller.ContentDataController.ContentDataModel.OnRegionAdded -= ContentDataModelOnOnRegionAdded;
            _controller.ContentDataController.ContentDataModel.OnRegionRemoved -= ContentDataModelOnOnRegionRemoved;
            foreach (var child in Children)
            {
                var region = child as ImageDetailRegionRenderItem;
                region.RegionMoved -= RegionOnRegionMoved;
                region.RegionResized -= RegionOnRegionResized;
                region?.Dispose();
            }

            base.Dispose();
        }

        private async void ContentDataModelOnOnRegionAdded(string regionLibraryElementModelId)
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

            _normalizedCroppedRect = new Rect(lib.NormalizedX, lib.NormalizedY, lib.NormalizedWidth, lib.NormalizedHeight);
            _rectToCropFromContent = new Rect(nx, ny, nw, nh);

            RecomputeSize();

            

            ComputeRegions();
        }

        private void RecomputeSize()
        {
            if (_bmp == null)
                return;

            var lib = (_controller.LibraryElementModel as ImageLibraryElementModel);
            var croppedRectRatio = _rectToCropFromContent.Width / _rectToCropFromContent.Height;
            if (_rectToCropFromContent.Width > _rectToCropFromContent.Height && CanvasSize.Width * 1 / croppedRectRatio <= CanvasSize.Height)
            {
                _croppedImageTarget.Width = CanvasSize.Width;
                _croppedImageTarget.Height = _croppedImageTarget.Width * 1 / croppedRectRatio;
                _scaleOrgToDisplay = CanvasSize.Width / _bmp.Size.Width;
                _scaleDisplayToCrop = 1 / lib.NormalizedWidth;
            }
            else
            {
                _croppedImageTarget.Height = CanvasSize.Height;
                _croppedImageTarget.Width = _croppedImageTarget.Height * croppedRectRatio;
                _scaleOrgToDisplay = CanvasSize.Height / _bmp.Size.Height;
                _scaleDisplayToCrop = 1 / lib.NormalizedHeight;
            }

            _needsMaskRefresh = true;
        }

        private void ComputeRegions()
        {
            var children = Children.ToArray();
            Children.Clear();
            foreach (var child in children)
            {
                var region = child as ImageDetailRegionRenderItem;
                region.RegionMoved -= RegionOnRegionMoved;
                region.RegionResized -= RegionOnRegionResized;

                region?.Dispose();
            }

            var others = SessionController.Instance.ContentController.AllLibraryElementModels.Where(l => l.ContentDataModelId == _controller.ContentDataController.ContentDataModel.ContentId).Cast<ImageLibraryElementModel>();
            others = others.Where(l => l.LibraryElementId != _controller.LibraryElementModel.LibraryElementId);
            foreach (var l in others)
            {
                var region = new ImageDetailRegionRenderItem(l, _normalizedCroppedRect, _bmp.Bounds, _scaleDisplayToCrop * _scaleOrgToDisplay, this, ResourceCreator, IsRegionsModifiable);
                region.RegionMoved += RegionOnRegionMoved;
                region.RegionResized += RegionOnRegionResized;
                
                Children.Add(region);
            }

            Children.Sort( (a,b) => { var areaA = a.GetMeasure(); var areaB = b.GetMeasure();
                                      return areaA.Width*areaA.Height > areaB.Width*areaB.Height ?  1 : -1;
            });

            NeedsRedraw?.Invoke();
        }
        
        private void RegionOnRegionResized(ImageDetailRegionRenderItem region, Vector2 delta)
        {
            var rx = region.LibraryElementModel.NormalizedWidth + delta.X / _croppedImageTarget.Width / _scaleDisplayToCrop;
            var ry = region.LibraryElementModel.NormalizedHeight + delta.Y / _croppedImageTarget.Height / _scaleDisplayToCrop;
            rx = Math.Max(0, Math.Min(_normalizedCroppedRect.Width - (region.LibraryElementModel.NormalizedX - _normalizedCroppedRect.X), rx));
            ry = Math.Max(0, Math.Min( _normalizedCroppedRect.Height - (region.LibraryElementModel.NormalizedY - _normalizedCroppedRect.Y), ry));
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(region.LibraryElementModel.LibraryElementId) as ImageLibraryElementController;
            controller.SetWidth(rx);
            controller.SetHeight(ry);

            NeedsRedraw?.Invoke();
        }

        private void RegionOnRegionMoved(ImageDetailRegionRenderItem region, Vector2 delta)
        {
            var rx = region.LibraryElementModel.NormalizedX + delta.X/_croppedImageTarget.Width / _scaleDisplayToCrop;
            var ry = region.LibraryElementModel.NormalizedY + delta.Y/_croppedImageTarget.Height / _scaleDisplayToCrop;
            rx = Math.Max(_normalizedCroppedRect.X, Math.Min(_normalizedCroppedRect.X + _normalizedCroppedRect.Width - region.LibraryElementModel.NormalizedWidth , rx));
            ry = Math.Max(_normalizedCroppedRect.Y, Math.Min(_normalizedCroppedRect.Y + _normalizedCroppedRect.Height - region.LibraryElementModel.NormalizedHeight , ry));
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(region.LibraryElementModel.LibraryElementId) as ImageLibraryElementController;
            controller.SetXLocation(rx);
            controller.SetYLocation(ry);
            
            NeedsRedraw?.Invoke();
        }

        public override void Draw(CanvasDrawingSession ds)
        {

            if (_needsMaskRefresh)
            {
                _mask = CanvasGeometry.CreateRectangle(ResourceCreator, _croppedImageTarget);
                _needsMaskRefresh = false;
            }

            if (_mask == null)
                return;


            var orgTransform = ds.Transform;
            var offsetX = (float)(CanvasSize.Width - _croppedImageTarget.Width) / 2f;
            var offsetY = (float)(CanvasSize.Height - _croppedImageTarget.Height) / 2f;
            T = Matrix3x2.CreateTranslation(offsetX, offsetY);
            ds.Transform = GetTransform() * orgTransform;
            using (ds.CreateLayer(1, _mask))
            { 
                if (_bmp != null)

                    ds.DrawImage(_bmp, _croppedImageTarget, _rectToCropFromContent, 1, CanvasImageInterpolation.MultiSampleLinear);
                ds.Transform = orgTransform;

                if (IsRegionsVisible)
                    base.Draw(ds);

                ds.Transform = orgTransform;
            }
        }

        public Size CanvasSize
        {
            get
            {
                return _canvasSize;
            }
            set
            {
                _canvasSize = value;
                RecomputeSize();

                foreach (var child in Children.ToArray())
                {
                    var region = child as ImageDetailRegionRenderItem;
                    region.UpdateImageBound(_scaleDisplayToCrop * _scaleOrgToDisplay);
                }
            }
        }
    }
}
