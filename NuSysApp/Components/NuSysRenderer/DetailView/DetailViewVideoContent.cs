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
            SessionController.Instance.SessionView.FreeFormViewer.PlayFullScreenVideo(_controller, true);
        }

        public override void Dispose()
        {
            DoubleTapped -= DetailViewVideoContent_DoubleTapped;

            base.Dispose();
        }

        public override async Task Load()
        {
            Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, _controller.LargeIconUri, ResourceCreator.Dpi);
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
                    // then the width fill's the rectangle so newImageWidth = width, and the height is scaled as a ratio to the newImageWidth
                    // so newImageHeight =  width * ratio
                    var ratio = bitmapHeight / bitmapWidth;
                    newImageWidth = Width;
                    newImageHeight = newImageWidth * ratio;
                }
                //otherwise if the image is longer than it is wider
                else
                {
                    // otherwise the height fill's the rectangle so newImageHeight = height, and the width is scaled as a ratio to the newImageHeight
                    var ratio = bitmapWidth / bitmapHeight;
                    newImageHeight = Height;
                    newImageWidth = newImageHeight * ratio;
                }

                // check to make sure that the new ratio fits into the height and width
                // for example if the height is 10 and the newImageHeight is 20 then we must
                // divide the new width and height by 2 = Height/newImageHeight in order for
                // both the width and height to safely fit in the screen
                var checkScale = Math.Min(Width / newImageWidth, Height / newImageHeight);
                newImageWidth *= checkScale;
                newImageHeight *= checkScale;

                var normalizedImageWidth = newImageWidth / Width;
                var normalizedImageHeight = newImageHeight / Height;

                // set the image bounds based on the new image
                // .5 brings us to the middle of the screen, then we move to the left by half the newImageWidth which is imageWidthRatio/2
                // these are normalized coordinates, the width is obviously the imageWidthRatio, imageHeightRatio
                ImageBounds = new Rect(.5 - normalizedImageWidth/2, .5 - normalizedImageHeight/2, normalizedImageWidth, normalizedImageHeight);
            }
            

            base.Update(parentLocalToScreenTransform);
        }

        public override Rect GetLocalBounds()
        {
            return GetImageBounds() ?? base.GetLocalBounds();
        }
    }
}
