using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class DetailViewAudioRegionPage : DetailViewRegionPage
    {

        public DetailViewAudioRegionPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, AudioLibraryElementController controller) : base(parent, resourceCreator, controller, false)
        {
            // initialize the audio media player
            var content = new BaseMediaPlayerUIElement(this, Canvas, controller);

            SetContent(content);
        }
    }
}
