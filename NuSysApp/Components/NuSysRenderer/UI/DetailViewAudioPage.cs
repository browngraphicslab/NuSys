using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class DetailViewAudioPage : DetailViewPage
    {

        public DetailViewAudioPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, AudioLibraryElementController controller, bool showRegions) : base(parent, resourceCreator, controller, false, showRegions)
        {
            // initialize the audio media player
            var content = new BaseMediaPlayerUIElement(this, Canvas, controller, showRegions);

            SetContent(content);
        }
    }
}
