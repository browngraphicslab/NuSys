using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
            Mask = new Rect(0, 0, Width, Height);
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

        /// <summary>
        /// Override hit testing so that mask is applied
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public override BaseRenderItem HitTest(Vector2 screenPoint)
        {
            if (!IsVisible)
                return null;

            if (IsChildrenHitTestVisible)
            {
                var children = _children.ToList();
                children.Reverse();

                foreach (var child in children)
                {
                    var hit = child.HitTest(screenPoint);

                    // here is where the override occurs, everything else is identical to base, 
                    // we just check to make sure that if a hit was found on a child, the ScreenBounds()
                    // which are just the localBounds transformed into screen coordinates, actually contains
                    // the point that was pressed. We have override local bounds so that it returns the mask
                    // so only hit events that occur within the mask will be returned as true
                    if (hit != null && GetScreenBounds().Contains(screenPoint.ToPoint()))
                        return hit;
                }
            }

            if (!IsHitTestVisible)
            {
                return null;
            }

            var a = GetScreenBounds();

            return GetScreenBounds().Contains(screenPoint.ToPoint()) ? this : null;
        }

        /// <summary>
        /// the local bounds that we can hit test are the actual mask itself, everything outside the mask
        /// is invisible and shouldn't be able to be clicked on
        /// </summary>
        /// <returns></returns>
        public override Rect GetLocalBounds()
        {
            return Mask;
        }
    }
}
