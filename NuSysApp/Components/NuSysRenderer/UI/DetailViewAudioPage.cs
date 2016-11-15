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
