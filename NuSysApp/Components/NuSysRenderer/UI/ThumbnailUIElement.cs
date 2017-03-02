using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{

    public class ThumbnailUIElement : RectangleUIElement
    {
        /// <summary>
        /// The image to be displayed on the rectangle
        /// </summary>
        public ICanvasImage Image { get; set; }

        /// <summary>
        /// The bounds to draw the image in the rectangle, these are normalized coordinates
        /// </summary>
        public override Rect? ImageBounds { get; set; }

        public Rect? RegionBounds { get; set; }

        public ThumbnailUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

        }
    }
}
