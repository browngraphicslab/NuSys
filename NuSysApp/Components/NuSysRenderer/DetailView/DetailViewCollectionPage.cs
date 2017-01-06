using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class DetailViewCollectionPage : DetailViewPage
    {

        /// <summary>
        /// The content displayed on this page of the detail view
        /// </summary>
        private DetailViewCollectionContent _content;

        public DetailViewCollectionPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, CollectionLibraryElementController controller) : base(parent, resourceCreator, controller, false, false)
        {

            // initialize the image rectangle and the _imageLayoutManager
            _content = new DetailViewCollectionContent(this, Canvas, controller);

            SetContent(_content);
        }
    }
}
