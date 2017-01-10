using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using WinRTXamlToolkit.Imaging;

namespace NuSysApp
{
    public class DetailViewVideoContent : RectangleUIElement
    {
        /// <summary>
        /// the video library element controller associated with this detail view page
        /// </summary>
        private VideoLibraryElementController _controller;

        /// <summary>
        /// true if the video will have the ability to add and remove regions, as well as display them
        /// </summary>
        private bool _showRegions;

        public DetailViewVideoContent(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, VideoLibraryElementController controller, bool showRegions) : base(parent, resourceCreator)
        {
            _controller = controller;

            _showRegions = showRegions;
            DoubleTapped += DetailViewVideoContent_DoubleTapped;
        }

        private void DetailViewVideoContent_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            UITask.Run(() =>
            {
                SessionController.Instance.SessionView.FreeFormViewer.PlayFullScreenVideo(_controller, true);
            });
        }

        public override void Dispose()
        {
            DoubleTapped -= DetailViewVideoContent_DoubleTapped;

            base.Dispose();
        }

        public override async Task Load()
        {
            Image = await CanvasBitmap.LoadAsync(Canvas, _controller.LargeIconUri, ResourceCreator.Dpi);
            base.Load();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // update the image rect only if the image is not null
            if (Image != null)
            {
                // get the imageRect which contains the image
                var imageRect = Image.GetBounds(Canvas, Transform.LocalToScreenMatrix);

                // get the width and height of the bitmap this has nothign to do with on screen widthand height
                var bitmapWidth = imageRect.Width;
                var bitmapHeight = imageRect.Height;

                // variables for storing the new width and new height
                double newImageWidth;
                double newImageHeight;


                // if the image is wider than it is longer
                if (bitmapHeight / bitmapWidth < bitmapWidth / bitmapHeight)
                {
                    var ratio = bitmapHeight / bitmapWidth;
                    newImageWidth = Width;
                    newImageHeight = Width * ratio;
                }
                //otherwise if the image is longer than it is wider
                else
                {
                    var ratio = bitmapWidth / bitmapHeight;
                    newImageWidth = Width * ratio;
                    newImageHeight = Width * ratio;
                }

                // check to make sure that the new ratio fits into the height and width
                var checkScale = Math.Min(Width / newImageWidth, Height / newImageHeight);
                newImageWidth *= checkScale;
                newImageHeight *= checkScale;

                // set the image bounds based on the new image
                ImageBounds = new Rect(Width / 2 - newImageWidth / 2, Height / 2 - newImageHeight / 2, newImageWidth, newImageHeight);
            }
            

            base.Update(parentLocalToScreenTransform);
        }

        public override Rect GetLocalBounds()
        {
            return ImageBounds ?? base.GetLocalBounds();
        }
    }
}
