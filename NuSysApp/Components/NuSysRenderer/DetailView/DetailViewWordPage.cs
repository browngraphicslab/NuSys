using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    /// <summary>
    /// The detail view page for word library elements
    /// </summary>
    public class DetailViewWordPage : DetailViewPage
    {
        /// <summary>
        /// constructor takes in the usual parameters and a wordNodeLibraryElementController
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="controller"></param>
        public DetailViewWordPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, WordNodeLibraryElementController controller) : base(parent, resourceCreator, controller, false, false)
        {            
            // initialize the image rectangle and the _imageLayoutManager
            var content = new DetailViewPdfRegionContent(this, Canvas, controller,false);
            SetContent(content);
        }
    }
}
