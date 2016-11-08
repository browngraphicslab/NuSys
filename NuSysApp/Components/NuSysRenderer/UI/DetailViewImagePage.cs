using System;
using System.Collections.Generic;
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
    class DetailViewImagePage : DetailViewPage
    {
        private DetailViewImageRegionContent _content;

        public DetailViewImagePage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, ImageLibraryElementController controller, bool showImageAnalysis, bool showRegions) : base(parent, resourceCreator, controller, showImageAnalysis, showRegions)
        {
            // initialize the image rectangle and the _imageLayoutManager
            _content = new DetailViewImageRegionContent(this, Canvas, controller, showRegions);

            SetContent(_content);
            
        }

    }
}
