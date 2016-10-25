using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class DetailViewImageRegionPage : RectangleUIElement
    {
        /// <summary>
        /// Rectangle holding the content of the image
        /// </summary>
        private RectangleImageUIElement _imageRect;

        /// <summary>
        /// the stack layout manager managing the layout of the image on the window
        /// </summary>
        private StackLayoutManager _imageLayoutManager;

        public DetailViewImageRegionPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            ImageLibraryElementController controller) : base(parent, resourceCreator)
        {
            // initialize the image rectangle and the _imageLayoutManager
            _imageRect = new RectangleImageUIElement(this, Canvas, controller);
            _imageLayoutManager = new StackLayoutManager();
            AddChild(_imageRect);
            _imageLayoutManager.AddElement(_imageRect);

        }

        /// <summary>
        /// The dispose method, remove events here, dispose of objects here
        /// </summary>
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            _imageLayoutManager.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Draws the image and stuff onto the 
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
            {
                return;
            }

            // save the old drawing session transform
            var orgTransform = ds.Transform;

            // set drawing session transform to draw using local coordinates on the screen
            ds.Transform = Transform.LocalToScreenMatrix;


            // resest to old drawing session transform
            ds.Transform = orgTransform;

            // draw all the children (regions etc)
            base.Draw(ds);
        }

        /// <summary>
        /// The update method, manage the layout here, update the transform here, called before draw
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _imageLayoutManager.SetSize(Width, Height);
            _imageLayoutManager.VerticalAlignment = VerticalAlignment.Center;
            _imageLayoutManager.HorizontalAlignment = HorizontalAlignment.Center;
            _imageLayoutManager.ItemWidth = Width - 20;
            _imageLayoutManager.ItemHeight = Height - 20;
            _imageLayoutManager.ArrangeItems();

            base.Update(parentLocalToScreenTransform);
        }


    }
}
