using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace NuSysApp
{
    public class ToolResizerRenderItem :RectangleUIElement
    {
        private CanvasGeometry _triangle;

        public ToolResizerRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Background = Colors.Transparent;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            if (_triangle != null)
                ds.FillGeometry(_triangle, new Vector2(0, 0), Constants.MED_BLUE);

            ds.Transform = orgTransform;

            base.Draw(ds);
        }

        public override async Task Load()
        {
            _triangle = CanvasGeometry.CreatePolygon(ResourceCreator, new Vector2[4]{new Vector2(0, Height),
                new Vector2(Width, Height),
                new Vector2(Width, 0),
                new Vector2(0, Height)
            });
        }
        
        
    }
}