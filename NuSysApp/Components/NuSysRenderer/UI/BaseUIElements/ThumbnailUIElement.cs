using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace NuSysApp
{
    public class ThumbnailUIElement : RectangleUIElement
    {

        public Rect? RegionBounds { get; set; }

        public ThumbnailUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {


        }


        public override void Draw(CanvasDrawingSession ds)
        {
            DrawBackground(ds);
            DrawThumbnail(ds);
        }

        private void DrawThumbnail(CanvasDrawingSession ds)
        {
            if (Image != null)
            {
                var orgTransform = ds.Transform;
                ds.Transform = Transform.LocalToScreenMatrix;

                using (ds.CreateLayer(1, CanvasGeometry.CreateRectangle(ResourceCreator, GetLocalBounds())))
                {
                    ds.DrawImage(Image, GetImageBounds() ?? GetLocalBounds(), GetDestinationRectangle() ?? Image.GetBounds(ResourceCreator));
                }

                ds.Transform = orgTransform;
            }
        }


        private Rect? GetDestinationRectangle()
        {
            if (RegionBounds == null)
            {
                return null;
            }

            return null;
        }

        private Rect? DenormalizeRect(Rect? normalizedRect)
        {
            var imgBounds = Image?.GetBounds(ResourceCreator);

            if (imgBounds == null || normalizedRect == null)
            {
                return null;
            }

            var imgWidth = imgBounds.Value.Width;
            var imgHeight = imgBounds.Value.Height;

            return new Rect(normalizedRect.Value.X * imgWidth, normalizedRect.Value.Y * imgHeight, normalizedRect.Value.Width * imgWidth, normalizedRect.Value.Height * imgHeight);
        }
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            base.Update(parentLocalToScreenTransform);
        }
    }
}
