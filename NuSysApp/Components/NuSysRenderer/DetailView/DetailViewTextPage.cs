using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class DetailViewTextPage : DetailViewPage
    {
        private DetailViewTextContent _content;

        public DetailViewTextPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) : base(parent, resourceCreator, controller, false, false)
        {
            // initialize the image rectangle and the _imageLayoutManager
            _content = new DetailViewTextContent(this, Canvas, controller);

            SetContent(_content);
        }
    }
}
