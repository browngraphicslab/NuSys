using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class DraggableButtonUIElement : ButtonUIElement
    {
        public DraggableButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BaseInteractiveUIElement shapeElement) : base(parent, resourceCreator, shapeElement)
        {
        }


    }
}