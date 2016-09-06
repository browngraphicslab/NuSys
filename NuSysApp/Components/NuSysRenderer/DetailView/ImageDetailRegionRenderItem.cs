using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class ImageDetailRegionRenderItem : InteractiveBaseRenderItem
    {
        private Rect _rect = new Rect();

        public delegate void RegionUpdatedHandler(ImageDetailRegionRenderItem regionRegion);
        public event RegionUpdatedHandler RegionUpdated;
        private ImageDetailRegionResizerRenderItem _resizer;

        public ImageLibraryElementModel LibraryElementModel { get; set; }
        
        public ImageDetailRegionRenderItem(ImageLibraryElementModel libraryElementModel, Rect rect, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            LibraryElementModel = libraryElementModel;
            _rect = rect;
            _resizer = new ImageDetailRegionResizerRenderItem(this, resourceCreator);
            _resizer.ResizerDragged += ResizerOnResizerDragged;
            Children.Add(_resizer);
        }

        public override void Dispose()
        {
            LibraryElementModel = null;
            _resizer.ResizerDragged -= ResizerOnResizerDragged;
            _resizer.Dispose();
            _resizer = null;
        }

        private void ResizerOnResizerDragged(Vector2 delta)
        {
            _rect.Width += delta.X;
            _rect.Height += delta.Y;
            RegionUpdated?.Invoke(this);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
    
            var orgTransform = ds.Transform;

            ds.Transform = GetTransform()*orgTransform;
   
            ds.DrawRectangle(new Rect(0,0,_rect.Width, _rect.Height), Color.FromArgb(0x33,0,0x55,0), 3);

            ds.Transform = orgTransform;

            _resizer.T = Matrix3x2.CreateTranslation((float)(_rect.X +_rect.Width), (float)(_rect.Y + _rect.Height));
            base.Draw(ds);
        }

        public override void OnDragged(CanvasPointer pointer)
        {
            base.OnDragged(pointer);
            T = Matrix3x2.CreateTranslation(T.M31 + pointer.DeltaSinceLastUpdate.X, T.M32 + pointer.DeltaSinceLastUpdate.Y);
            RegionUpdated?.Invoke(this);
        }

        public override Rect GetMeasure()
        {
            return _rect;
        }
    }
}
