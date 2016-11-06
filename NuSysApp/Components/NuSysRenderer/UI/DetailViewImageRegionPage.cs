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
    class DetailViewImageRegionPage : DetailViewRegionPage
    {
        private DetailViewImageRegionContent _content;

        public DetailViewImageRegionPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            ImageLibraryElementController controller) : base(parent, resourceCreator, controller, true)
        {
            // initialize the image rectangle and the _imageLayoutManager
            _content = new DetailViewImageRegionContent(this, Canvas, controller);

            SetContent(_content);
            
        }

    }
}
