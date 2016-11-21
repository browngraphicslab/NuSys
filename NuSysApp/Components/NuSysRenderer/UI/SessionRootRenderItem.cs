using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class SessionRootRenderItem : BaseRenderItem
    {
        public NuSessionViewer NuSessionViewer;

        public SessionRootRenderItem(BaseRenderItem parent, CanvasAnimatedControl canvas) : base(parent, canvas)
        {
            NuSessionViewer = new NuSessionViewer(this, canvas);
            

        }

        public override BaseRenderItem HitTest(Vector2 screenPoint)
        {
            // check to see if the session view was hit
            var hitItem = NuSessionViewer.HitTest(screenPoint);
            if (hitItem == NuSessionViewer)
                return base.HitTest(screenPoint);

            // return the session view item if it was hit
            // otherwise return the item from the rest of the children
            return hitItem ?? base.HitTest(screenPoint);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            // draw all the children
            base.Draw(ds);

            // then draw the session view on top of the children
            NuSessionViewer.Draw(ds);
        }

        public override Task Load()
        {
            NuSessionViewer.Load();

            return base.Load();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            NuSessionViewer.Update(parentLocalToScreenTransform);

            base.Update(parentLocalToScreenTransform);
        }

        public override void Dispose()
        {
            NuSessionViewer.Dispose();

            base.Dispose();
        }
    }
}
