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
    public class HtmlElementRenderItem : ImageElementRenderItem
    {

        private ICanvasImage _htmlIcon;
        public HtmlElementRenderItem(ImageElementViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(vm, parent, resourceCreator)
        {
        }
        public override async Task Load()
        {
            _htmlIcon = await MediaUtil.LoadCanvasBitmapAsync(ResourceCreator, new Uri("ms-appx:///Assets/library_thumbnails/html.png"));
            await base.Load();
        }
        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);
            if (_htmlIcon != null)
            {
                var orgTransform = ds.Transform;
                ds.Transform = Transform.LocalToScreenMatrix;

                using (ds.CreateLayer(1, CanvasGeometry.CreateRectangle(Canvas, new Rect(0, 0, Width, Height))))
                {
                    ds.DrawImage(_htmlIcon, new Rect(0, 0, Constants.DefaultNodeSize * .125, Constants.DefaultNodeSize * .125), _htmlIcon.GetBounds(Canvas));
                }

                ds.Transform = orgTransform;
            }
        }
    }
}
