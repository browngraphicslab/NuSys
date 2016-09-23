using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class CloseButtonRenderItem : InteractiveBaseRenderItem
    {
        private CanvasBitmap _bmp;
        private Rect _targetRect = new Rect(0, 0, 15, 15);

        public CloseButtonRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override async Task Load()
        {
            _bmp = await CanvasBitmap.LoadAsync(ResourceCreator, new Uri("ms-appx:///Assets/icon_node_x.png"));
            await base.Load();
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (_bmp == null)
                return;

            ds.Transform = Transform.LocalToScreenMatrix;
            ds.DrawImage(_bmp, _targetRect);

            base.Draw(ds);
        }

        public override Rect GetLocalBounds()
        {
            return _targetRect;
        }
    }
}
