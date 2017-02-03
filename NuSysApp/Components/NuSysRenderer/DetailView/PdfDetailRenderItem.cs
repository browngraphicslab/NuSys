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
    public class PdfDetailRenderItem : ImageDetailRenderItem
    {

        public int CurrentPage { get; set; }
       
        public PdfDetailRenderItem(PdfLibraryElementController controller, Size maxSize, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(controller, maxSize, parent, resourceCreator)
        {
            var content = controller.ContentDataController.ContentDataModel as PdfContentDataModel;
            ImageUrl = content.PageUrls[0];
            controller.ContentDataController.ContentDataUpdated += ContentDataControllerOnContentDataUpdated;
            _controller = controller;
            _canvasSize = maxSize;
        }

        /// <summary>
        /// event hander called whenever the pdf changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="s"></param>
        private void ContentDataControllerOnContentDataUpdated(object sender, string s)
        {
            Debug.Assert(_controller.ContentDataController.ContentDataModel is PdfContentDataModel);
            ImageUrl = (_controller.ContentDataController.ContentDataModel as PdfContentDataModel).PageUrls[CurrentPage];
            Load();
        }


        protected override void ComputeRegions()
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

            var others = SessionController.Instance.ContentController.AllLibraryElementModels.Where(l => l.ContentDataModelId == _controller.ContentDataController.ContentDataModel.ContentId).Cast<PdfLibraryElementModel>();
            others = others.Where(l => l.LibraryElementId != _controller.LibraryElementModel.LibraryElementId && l.PageStart >= CurrentPage && l.PageEnd <= CurrentPage);
            foreach (var l in others)
            {
                var region = new ImageDetailRegionRenderItem(l, _normalizedCroppedRect, _bmp.Bitmap.Bounds, _scaleDisplayToCrop * _scaleOrgToDisplay, this, ResourceCreator, IsRegionsModifiable);
                region.RegionMoved += RegionOnRegionMoved;
                region.RegionResized += RegionOnRegionResized;
                region.RegionPressed += RegionOnRegionPressed;
                region.RegionReleased += RegionOnRegionReleased;

                AddChild(region);
            }

            SortChildren((a, b) => {
                var areaA = a.GetLocalBounds(); var areaB = b.GetLocalBounds();
                return areaA.Width * areaA.Height > areaB.Width * areaB.Height ? 1 : -1;
            });

            FireRedraw();
        }


        /// <summary>
        /// removes region event handlers 
        /// </summary>
        public override void Dispose()
        {
            _controller.ContentDataController.ContentDataUpdated -= ContentDataControllerOnContentDataUpdated;
            Debug.Assert(_bmp != null);
            _bmp?.Dispose();
            base.Dispose();
        }
    }
}
