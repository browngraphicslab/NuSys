using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class DetailViewPdfPage : DetailViewPage
    {

        private DetailViewPdfRegionContent _content;


        public DetailViewPdfPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, PdfLibraryElementController controller, bool showImageAnalysis, bool showRegions) : base(parent, resourceCreator, controller, showImageAnalysis, showRegions)
        {
            _content = new DetailViewPdfRegionContent(this, resourceCreator, controller, showRegions);
            SetContent(_content);

        }
    }
}
