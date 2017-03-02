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
                var clip = GetClippedImageBounds();
                using (ds.CreateLayer(1, CanvasGeometry.CreateRectangle(ResourceCreator, clip)))
                {
                    ds.DrawImage(Image, GetImageBounds() ?? GetLocalBounds(), Image.GetBounds(ResourceCreator));
                }

                ds.Transform = orgTransform;
            }
        
        }

        private Rect GetClippedImageBounds()
        {
            if (RegionBounds == null)
            {
                return new Rect(0, 0, Width, Height);

            }
            var region = RegionBounds.Value;

            var v = GetImageBounds() ?? GetLocalBounds();
            var imgWidth = v.Width; //Image.GetBounds(ResourceCreator).Width;
            var imgHeight = v.Height; //Image.GetBounds(ResourceCreator).Height;
            return new Rect(region.X * imgWidth, region.Y * imgHeight, region.Width * imgWidth, region.Height * imgHeight);
        }


    }
}
