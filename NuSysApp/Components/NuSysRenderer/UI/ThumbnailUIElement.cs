using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace NuSysApp
{
    public class ThumbnailUIElement : RectangleUIElement
    {
        public ICanvasImage Image { get; set; }

        /// <summary>
        /// The bounds to draw the image in the rectangle, these are normalized coordinates
        /// </summary>
        public Rect? ImageBounds {
            get
            {
                return _imageBounds; 
            }
            set
            {
                base.ImageBounds = _imageBounds;
                _imageBounds = value;
                
            }
        }


        public Rect? RegionBounds
        {
            get
            {
                return _regionBounds; 
            }
            set
            {
                _regionBounds = value;
            }
        }

        private Rect? _imageBounds;
        private Rect? _regionBounds;


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

                using (ds.CreateLayer(1, CanvasGeometry.CreateRectangle(ResourceCreator, GetLocalBounds())))
                {
                    ds.DrawImage(Image, GetImageBounds() ?? GetLocalBounds(),  GetRegionBounds()?? Image.GetBounds(ResourceCreator));
                }

                ds.Transform = orgTransform;
            }
        }

        private Rect? GetRegionBounds()
        {
            if (RegionBounds == null)
            {
                return null;
            }
            var bounds=  Image.GetBounds(ResourceCreator);
            return new Rect(bounds.Width * RegionBounds.Value.Left, bounds.Height * RegionBounds.Value.Top, bounds.Width * RegionBounds.Value.Width, bounds.Height * RegionBounds.Value.Height);
        }
    }
}
