using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    class DetailViewPdfRegionRenderItem : DetailViewImageRegionContent
    {
        /// <summary>
        /// Helper for the current page of the pdf
        /// </summary>
        private int _currentPage;

        /// <summary>
        /// The current page of the pdf
        /// </summary>
        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                Debug.Assert(_pdfContentDataModel != null, "The pdf content data model has to be set in the constructor, before a page is displayed");
                // set value to zero if we are negative, no negative pages
                value = value < 0 ? 0 : value;
                // set value to last page if we are greater than last page
                value = value >= _pdfContentDataModel.PageCount ? _pdfContentDataModel.PageCount - 1 : value;

                // set the current page correctly
                _currentPage = value;

                // change the image url to the current page
                ImageUrl = _pdfContentDataModel.PageUrls[_currentPage];
            }
        }

        /// <summary>
        /// The pdf content data model associated with the pdf
        /// </summary>
        private PdfContentDataModel _pdfContentDataModel;

        public DetailViewPdfRegionRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, PdfLibraryElementController controller) : base(parent, resourceCreator, controller)
        {

            _controller = controller;

            // initialzie the image url to the first page
            _pdfContentDataModel = controller.ContentDataController.ContentDataModel as PdfContentDataModel;
            Debug.Assert(_pdfContentDataModel != null);
            CurrentPage = 0;

            // set defaults
            IsRegionsModifiable = true;
            IsRegionsVisible = true;

            // add events
            _controller.LocationChanged += ControllerOnLocationChanged;
            _controller.SizeChanged += ControllerOnSizeChanged;

            _controller.ContentDataController.ContentDataModel.OnRegionAdded += ContentDataModelOnOnRegionAdded;
            _controller.ContentDataController.ContentDataModel.OnRegionRemoved += ContentDataModelOnOnRegionRemoved;          
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
                var region = new ImageDetailRegionRenderItem(l, _normalizedCroppedRect, _imageBitmap.Bounds, _scaleDisplayToCrop * _scaleOrgToDisplay, this, ResourceCreator, IsRegionsModifiable);
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
        }
    }
}
