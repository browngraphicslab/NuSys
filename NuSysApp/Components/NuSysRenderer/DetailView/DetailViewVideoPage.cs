using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class DetailViewVideoPage : DetailViewPage
    {

        private DetailViewVideoContent _content;

        public DetailViewVideoPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, VideoLibraryElementController controller, bool showRegions) : base(parent, resourceCreator, controller, false, false)
        {
            // initialize the image rectangle and the _imageLayoutManager
            _content = new DetailViewVideoContent(this, Canvas, controller, showRegions);

            SetContent(_content);

        }
    }

}
