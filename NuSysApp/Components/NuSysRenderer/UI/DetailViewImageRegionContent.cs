using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using NusysIntermediate;

namespace NuSysApp
{
    public class DetailViewImageRegionContent : RectangleUIElement
    {
        /// <summary>
        /// The url of the image, this is just a helper for the public property ImageUrl
        /// </summary>
        private string _imageUrl;

        /// <summary>
        /// The url of the image we are going to display. used as the argument for a new URI()
        /// </summary>
        public string ImageUrl
        {
            get { return _imageUrl; }
            set
            {
                _imageUrl = value;
                LoadImageBitmap();
            }
        }

        /// <summary>
        /// The CanvasBitmap to hold the image
        /// </summary>
        protected CanvasBitmap _imageBitmap;

        /// <summary>
        /// The image library element controller for the RectangleImageUIElements
        /// </summary>
        protected ImageLibraryElementController _controller;

        /// <summary>
        /// Rect of the normalized coordinates which are cropped to display the image
        /// </summary>
        protected Rect _normalizedCroppedRect;

        /// <summary>
        /// Rect of the normalized DPI of the cropping used to display the image
        /// </summary>
        protected Rect _rectToCropFromContent;

        /// <summary>
        /// True if the mask needs to be refreshed for the regions
        /// </summary>
        protected bool _needsMaskRefresh;

        /// <summary>
        /// The maximum width that the image can be displayed
        /// </summary>
        protected float ImageMaxWidth;

        /// <summary>
        /// The maximum height that the image can be displayed
        /// </summary>
        protected float ImageMaxHeight;

        //todo say what this is
        protected Rect _croppedImageTarget;

        //todo say what this is
        protected float _scaleOrgToDisplay;

        //todo say what this is
        protected float _scaleDisplayToCrop;

        //todo say what this is
        protected CanvasGeometry _mask;

        /// <summary>
        /// Boolean which determines whether or not to show the crop gray background effect while moving regions
        /// </summary>
        protected bool _showCroppy;

        /// <summary>
        /// The rectangle used to show the croppy effect while moving regions
        /// </summary>
        protected CanvasGeometry _croppy;

        /// <summary>
        /// The region that is currently getting manipulated
        /// </summary>
        protected ImageDetailRegionRenderItem _activeRegion;

        /// <summary>
        /// True when the bitmap is being drawn
        /// </summary>
        private bool DrawingBitmap;

        /// <summary>
        /// Determines if the regions are able to be moved. True by default
        /// </summary>
        public bool IsRegionsModifiable { get; set; }

        /// <summary>
        /// Determines if the regions are visible. True by default
        /// </summary>
        public bool IsRegionsVisible { get; set; }


        public DetailViewImageRegionContent(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, ImageLibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;
            ImageUrl = controller.ContentDataController.ContentDataModel.Data;

            // set defaults
            IsRegionsModifiable = true;
            IsRegionsVisible = true;

            _controller.LocationChanged += ControllerOnLocationChanged;
            _controller.SizeChanged += ControllerOnSizeChanged;

            _controller.ContentDataController.ContentDataModel.OnRegionAdded += ContentDataModelOnOnRegionAdded;
            _controller.ContentDataController.ContentDataModel.OnRegionRemoved += ContentDataModelOnOnRegionRemoved;
        }

        public DetailViewImageRegionContent(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            PdfLibraryElementController controller) : base(parent, resourceCreator)
        {
            // do nothing handled in the extended class DetailViewPdfRegionRenderItem
        }


        /// <summary>
        /// Called when a region is removed, recomputes the regions for the entire image
        /// </summary>
        /// <param name="regionlibraryelementmodelid"></param>
        protected void ContentDataModelOnOnRegionRemoved(string regionlibraryelementmodelid)
        {
            ComputeRegions();
        }

        /// <summary>
        /// Called when a region is added, recomputes the regions for the entire image
        /// </summary>
        /// <param name="regionlibraryelementmodelid"></param>
        protected void ContentDataModelOnOnRegionAdded(string regionlibraryelementmodelid)
        {
            ComputeRegions();
        }

        /// <summary>
        /// Called when the Controller's size changes, ReRenders the image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected void ControllerOnSizeChanged(object sender, double width, double height)
        {
            ReRender();
        }

        /// <summary>
        /// Called when the Controller's location changes, ReRenders the image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="topleft"></param>
        protected void ControllerOnLocationChanged(object sender, Point topleft)
        {
            ReRender();
        }

        /// <summary>
        /// Loads the image bitmap
        /// </summary>
        protected async void LoadImageBitmap()
        {
            _imageBitmap?.Dispose();
            DrawingBitmap = true;
            if (ImageUrl == null)
            {
                return;
            }

            await Task.Run(async () =>
            {
                _imageBitmap =
                    await
                        CanvasBitmap.LoadAsync(ResourceCreator, new Uri(ImageUrl),
                            ResourceCreator.Dpi);
            });
            DrawingBitmap = false;

            ReRender();
        }



        /// <summary>
        /// Draw the image as the background
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed || DrawingBitmap)
                return;
            
            // if no _imageBitmap is set just treat this like a normal rectangle and draw the background
            if (_imageBitmap == null)
            {
                base.DrawBackground(ds);
                return;
            }

            if (_needsMaskRefresh)
            {
                _mask = CanvasGeometry.CreateRectangle(ResourceCreator, _croppedImageTarget);
                _needsMaskRefresh = false;
            }

            if (_showCroppy)
            {
                _croppy = CanvasGeometry.CreateRectangle(ResourceCreator, new Rect(_activeRegion.Transform.LocalPosition.X, _activeRegion.Transform.LocalPosition.Y, _activeRegion.GetLocalBounds().Width, _activeRegion.GetLocalBounds().Height)).CombineWith(CanvasGeometry.CreateRectangle(ResourceCreator, _croppedImageTarget), Matrix3x2.Identity, CanvasGeometryCombine.Xor);
            }


            var orgTransform = ds.Transform;

            ds.Transform = Transform.LocalToScreenMatrix;
            using (ds.CreateLayer(1, _mask))
            {
                ds.DrawImage(_imageBitmap, _croppedImageTarget, _rectToCropFromContent, 1, CanvasImageInterpolation.MultiSampleLinear);

                if (_activeRegion != null && _croppy != null)
                {
                    ds.FillGeometry(_croppy, Color.FromArgb(0x88, 0, 0, 0));
                }
                ds.Transform = orgTransform;

                if (IsRegionsVisible)
                    base.Draw(ds);

                ds.Transform = orgTransform;
            }
        }

        protected override void DrawBackground(CanvasDrawingSession ds)
        {
            // does nothing we dont want to draw a background
        }


        protected void ReRender()
        {
            var lib = (_controller.LibraryElementModel as ImageLibraryElementModel);
            Debug.Assert(lib != null);
            if (_imageBitmap == null)
            {
                return;
            }
            var nx = lib.NormalizedX * _imageBitmap.Size.Width;
            var ny = lib.NormalizedY * _imageBitmap.Size.Height;
            var nw = lib.NormalizedWidth * _imageBitmap.Size.Width;
            var nh = lib.NormalizedHeight * _imageBitmap.Size.Height;

            _normalizedCroppedRect = new Rect(lib.NormalizedX, lib.NormalizedY, lib.NormalizedWidth, lib.NormalizedHeight);
            _rectToCropFromContent = new Rect(nx, ny, nw, nh);

            _needsMaskRefresh = true;
            RecomputeSize();
            ComputeRegions();

        }


        protected virtual void ComputeRegions()
        {
            var children = GetChildren();
            ClearChildren();
            foreach (var child in children)
            {
                var region = child as ImageDetailRegionRenderItem;
                region.RegionMoved -= RegionOnRegionMoved;
                region.RegionResized -= RegionOnRegionResized;
                region.RegionPressed -= RegionOnRegionPressed;
                region.RegionReleased -= RegionOnRegionReleased;

                region?.Dispose();
            }

            var others = SessionController.Instance.ContentController.AllLibraryElementModels.Where(l => l.ContentDataModelId == _controller.ContentDataController.ContentDataModel.ContentId).Cast<ImageLibraryElementModel>();
            others = others.Where(l => l.LibraryElementId != _controller.LibraryElementModel.LibraryElementId);
            foreach (var l in others)
            {
                var region = new ImageDetailRegionRenderItem(l, _normalizedCroppedRect, _imageBitmap.Bounds, _scaleDisplayToCrop * _scaleOrgToDisplay, this, ResourceCreator, IsRegionsModifiable);
                region.RegionMoved += RegionOnRegionMoved;
                region.RegionResized += RegionOnRegionResized;
                region.RegionPressed += RegionOnRegionPressed;
                region.RegionReleased += RegionOnRegionReleased;

                AddChild(region);
            }

            SortChildren((a, b) => {
                var areaA = a.GetLocalBounds(); var areaB = b.GetLocalBounds(); return areaA.Width * areaA.Height >= areaB.Width * areaB.Height ? -1 : 1;
            });

        }


        /// <summary>
        /// Called when a region being manipulated is released
        /// </summary>
        /// <param name="region"></param>
        protected void RegionOnRegionReleased(ImageDetailRegionRenderItem region)
        {
            // dont show the gray background
            _showCroppy = false;
            _croppy = null;

            // set active region to null
            _activeRegion = null;
        }

        /// <summary>
        /// Called when a region is pressed
        /// </summary>
        /// <param name="region"></param>
        protected void RegionOnRegionPressed(ImageDetailRegionRenderItem region)
        {
            // show the gray background
            _showCroppy = true;

            // set the activeRegion to th ecurrent region
            _activeRegion = region;
        }

        /// <summary>
        /// Called when a region is resized
        /// </summary>
        /// <param name="region"></param>
        /// <param name="delta"></param>
        protected void RegionOnRegionResized(ImageDetailRegionRenderItem region, Vector2 delta)
        {
            var rx = region.LibraryElementModel.NormalizedWidth + delta.X / _imageBitmap.Size.Width / (_scaleDisplayToCrop * _scaleOrgToDisplay); ;
            var ry = region.LibraryElementModel.NormalizedHeight + delta.Y / _imageBitmap.Size.Height / (_scaleDisplayToCrop * _scaleOrgToDisplay); ;
            rx = Math.Max(0, Math.Min(_normalizedCroppedRect.Width - (region.LibraryElementModel.NormalizedX - _normalizedCroppedRect.X), rx));
            ry = Math.Max(0, Math.Min(_normalizedCroppedRect.Height - (region.LibraryElementModel.NormalizedY - _normalizedCroppedRect.Y), ry));
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(region.LibraryElementModel.LibraryElementId) as ImageLibraryElementController;
            controller.SetWidth(rx);
            controller.SetHeight(ry);
        }

        /// <summary>
        /// Called when a region is moved
        /// </summary>
        /// <param name="region"></param>
        /// <param name="delta"></param>
        protected void RegionOnRegionMoved(ImageDetailRegionRenderItem region, Vector2 delta)
        {
            var rx = region.LibraryElementModel.NormalizedX + delta.X / _imageBitmap.Size.Width / (_scaleDisplayToCrop * _scaleOrgToDisplay);
            var ry = region.LibraryElementModel.NormalizedY + delta.Y / _imageBitmap.Size.Height / (_scaleDisplayToCrop *_scaleOrgToDisplay);
            rx = Math.Max(_normalizedCroppedRect.X, Math.Min(_normalizedCroppedRect.X + _normalizedCroppedRect.Width - region.LibraryElementModel.NormalizedWidth, rx));
            ry = Math.Max(_normalizedCroppedRect.Y, Math.Min(_normalizedCroppedRect.Y + _normalizedCroppedRect.Height - region.LibraryElementModel.NormalizedHeight, ry));
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(region.LibraryElementModel.LibraryElementId) as ImageLibraryElementController;
            controller.SetXLocation(rx);
            controller.SetYLocation(ry);
        }

        protected void RecomputeSize()
        {
            if (_imageBitmap == null)
                return;

            var lib = (_controller.LibraryElementModel as ImageLibraryElementModel);
            Debug.Assert(lib != null);
            var croppedRectRatio = _rectToCropFromContent.Width / _rectToCropFromContent.Height;
            if (_rectToCropFromContent.Width > _rectToCropFromContent.Height && ImageMaxWidth * 1 / croppedRectRatio <= ImageMaxHeight)
            {
                _croppedImageTarget.Width = ImageMaxWidth;
                _croppedImageTarget.Height = _croppedImageTarget.Width * 1 / croppedRectRatio;
                _scaleOrgToDisplay =  ImageMaxWidth / (float) _imageBitmap.Size.Width;
                _scaleDisplayToCrop = 1 / (float) lib.NormalizedWidth;
            }
            else
            {
                _croppedImageTarget.Height = ImageMaxHeight;
                _croppedImageTarget.Width = _croppedImageTarget.Height * croppedRectRatio;
                _scaleOrgToDisplay = ImageMaxHeight / (float) _imageBitmap.Size.Height;
                _scaleDisplayToCrop = 1 / (float) lib.NormalizedHeight;
            }

            _needsMaskRefresh = true;
        }

        /// <summary>
        /// Update is used to arrange the item layout and set constraints
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // if the dimensions have changed, update the maximum dimensions and ReRender the image
            if (Math.Abs(ImageMaxWidth - Width) > .1 || Math.Abs(ImageMaxHeight - Height) > .1)
            {
                ImageMaxWidth = Width;
                ImageMaxHeight = Height;
                RecomputeSize();
                foreach (var child in GetChildren())
                {
                    var region = child as ImageDetailRegionRenderItem;
                    region.UpdateImageBound(_scaleDisplayToCrop * _scaleOrgToDisplay);
                }
            }

            var offsetX = Transform.LocalPosition.X + (float)(ImageMaxWidth - _croppedImageTarget.Width) / 2f;
            var offsetY = Transform.LocalPosition.Y + (float)(ImageMaxHeight - _croppedImageTarget.Height) / 2f;
            Transform.LocalPosition = new Vector2(offsetX, offsetY);
            base.Update(parentLocalToScreenTransform);
        }
    }
}
