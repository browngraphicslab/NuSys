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
        private Rect _regionBounds;
        private Rect _imageBounds;

        public delegate void RegionUpdatedHandler(ImageDetailRegionRenderItem regionRegion);
        public event RegionUpdatedHandler RegionUpdated;
        private ImageDetailRegionResizerRenderItem _resizer;

        public ImageLibraryElementModel LibraryElementModel { get; set; }
        
        public ImageDetailRegionRenderItem(ImageLibraryElementModel libraryElementModel, Rect regionBounds, Rect imageBounds, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            LibraryElementModel = libraryElementModel;
            _regionBounds = regionBounds;
            _imageBounds = imageBounds;
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
            _regionBounds.Width += delta.X;
            _regionBounds.Height += delta.Y;
            RegionUpdated?.Invoke(this);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
    
            var orgTransform = ds.Transform;

            ds.Transform = GetTransform()*orgTransform;
   
            ds.DrawRectangle(new Rect(0,0,_regionBounds.Width, _regionBounds.Height), Color.FromArgb(0x33,0,0x55,0), 3);

            ds.Transform = orgTransform;

            _resizer.T = Matrix3x2.CreateTranslation((float)(_regionBounds.X +_regionBounds.Width), (float)(_regionBounds.Y + _regionBounds.Height));
            base.Draw(ds);
        }

        public override void OnDragged(CanvasPointer pointer)
        {
            base.OnDragged(pointer);
            var nx = (float)Math.Max(_imageBounds.X, Math.Min(_imageBounds.X + _imageBounds.Width - _regionBounds.Width, T.M31 + pointer.DeltaSinceLastUpdate.X));
            var ny = (float)Math.Max(_imageBounds.Y, Math.Min(_imageBounds.Y + _imageBounds.Height - _regionBounds.Height, T.M32 + pointer.DeltaSinceLastUpdate.Y));
            T = Matrix3x2.CreateTranslation(nx, ny);
            RegionUpdated?.Invoke(this);
        }

        public override Rect GetMeasure()
        {
            return _regionBounds;
        }
    }
}
