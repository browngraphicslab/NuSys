using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class MaskedRectangleUIElement : RectangleUIElement
    {
        /// <summary>
        /// the mask to apply when drawing all the children, everything inside the mask will eb visible and everything outisde
        /// the mask will be invisible
        /// </summary>
        public Rect Mask { get; set; }

        public MaskedRectangleUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the default mask rect
            Mask = GetLocalBounds();
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
            {
                return;
            }
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            using (ds.CreateLayer(1, Mask))
            {
                base.Draw(ds);
            }

            ds.Transform = orgTransform;
        }
    }
}
