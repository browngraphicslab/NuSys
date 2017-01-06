using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    /// <summary>
    /// This is the home page for elements of the link type
    /// </summary>
    public class DetailViewLinkPage : DetailViewPage
    {
        private DetailViewLinkContent _content;

        public DetailViewLinkPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LinkLibraryElementController controller) : base(parent, resourceCreator, controller, false, false)
        {
            // initialize the image rectangle and the _imageLayoutManager
            _content = new DetailViewLinkContent(this, Canvas, controller);

            SetContent(_content);
        }
    }
}
