using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class DetailViewRenderItem : InteractiveBaseRenderItem
    {

        private CanvasAnimatedControl _canvas;

        public DetailViewRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _canvas = resourceCreator as CanvasAnimatedControl;

            Dragged += DetailViewRenderItem_Dragged;
            Tapped += DetailViewRenderItem_Tapped;
        }

        private void DetailViewRenderItem_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            throw new NotImplementedException();
        }

        private void DetailViewRenderItem_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            throw new NotImplementedException();
        }


        //public override void Draw(CanvasDrawingSession ds)
        //{
        //    base.Draw(ds);
        //    var screenWidth = SessionController.Instance.ScreenWidth;
        //    var screenHeight = SessionController.Instance.ScreenHeight;

        //}

        public override void Draw(CanvasDrawingSession ds)
        {
            // todo explain why we need this
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            var screenWidth = SessionController.Instance.ScreenWidth;
            var screenHeight = SessionController.Instance.ScreenHeight;
            ds.FillRectangle(new Rect(screenWidth/4, screenHeight/4, screenWidth/2, screenHeight/2), Colors.Red );

            ds.Transform = orgTransform;

            base.Draw(ds);
        }
    }
}
