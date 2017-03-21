using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class DetailViewUnknownFilePage : DetailViewPage
    {
        public DetailViewUnknownFilePage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller, bool showsImageAnalysis = false, bool showRegions = false) : base(parent, resourceCreator, controller, showsImageAnalysis, showRegions)
        {
            var content = new DetailViewUnknownFileContent(this, Canvas, controller);
            SetContent(content);
        }
    }
}
