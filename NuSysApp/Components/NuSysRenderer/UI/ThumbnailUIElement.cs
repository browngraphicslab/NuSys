using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using NusysIntermediate;

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

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            DrawImage(ds);
            
        }

        private void DrawImage(CanvasDrawingSession ds)
        {

            if (Image != null)
            {
                var orgTransform = ds.Transform;
                ds.Transform = Transform.LocalToScreenMatrix;
                var clip = GetClippedRegionBounds();
                using (ds.CreateLayer(1, CanvasGeometry.CreateRectangle(ResourceCreator, clip)))
                {
                    ds.DrawImage(Image, CalculateImageBounds() ?? GetLocalBounds(), Image.GetBounds(ResourceCreator));
                }

                ds.Transform = orgTransform;
            }
        
        }

        private Rect? CalculateImageBounds()
        {
            //if user has not set image bounds, return null so we can draw to local bounds
            if (ImageBounds == null)
            {
                return null;
            }
            //if user has set image bounds but not region bounds, simply return image bounds
            if (RegionBounds == null)
            {
                return GetImageBounds();
            }

            if (RegionBounds.Value.X == 0 && RegionBounds.Value.Y == 0 && RegionBounds.Value.Width ==
                    1 && RegionBounds.Value.Height == 1)
            {
                return GetImageBounds();
            }

            var total =  GetImageBounds();

            var actual = GetClippedRegionBounds();
            //inflate the image so that the clipped region rectangle essentially covers all of the image bound

            var widthScale = total.Value.Width/actual.Width;
            var heightScale = total.Value.Height/actual.Height;

            //finally, if user has set image bounds and region bounds, calculate proper region bounds
            return new Rect(0, 0, total.Value.Width/RegionBounds.Value.Width, total.Value.Height/RegionBounds.Value.Height);
        }
        public Rect GetClippedRegionBounds()
        {
            if (RegionBounds == null)
            {
                return new Rect(0, 0, Width, Height);
            }
            var region = RegionBounds.Value;


            var v = GetImageBounds() ?? GetLocalBounds();
            var imgWidth = Width;//v.Width; //Image.GetBounds(ResourceCreator).Width;
            var imgHeight = Height;// v.Height; //Image.GetBounds(ResourceCreator).Height;
            return new Rect(region.X * imgWidth, region.Y * imgHeight, region.Width * imgWidth, region.Height * imgHeight);
        }


    }
}
