using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class DetailViewPdfRegionPage : DetailViewRegionPage
    {

        private DetailViewPdfRegionContent _content;


        public DetailViewPdfRegionPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, PdfLibraryElementController controller) : base(parent, resourceCreator, controller, true)
        {
            _content = new DetailViewPdfRegionContent(this, resourceCreator, controller);
            SetContent(_content);

        }
    }
}
