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
        private ImageLibraryElementController _controller;
        private Rect _regionBounds;
        private Rect _imageContainerBounds;

        public delegate void RegionUpdatedHandler(ImageDetailRegionRenderItem regionRegion);
        public event RegionUpdatedHandler RegionUpdated;
        private ImageDetailRegionResizerRenderItem _resizer;

        private Rect _bmpRect;
        public ImageLibraryElementModel LibraryElementModel { get; set; }
        public bool IsModifiable { get; set; }
        
        public ImageDetailRegionRenderItem(ImageLibraryElementModel libraryElementModel, Rect bitmapRect, Rect imageContainerBounds, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, bool isModifiable = true) : base(parent, resourceCreator)
        {
            IsModifiable = isModifiable;
            _bmpRect = bitmapRect;
            _controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModel.LibraryElementId) as ImageLibraryElementController;
            _controller.SizeChanged += ControllerOnSizeChanged;
            _controller.LocationChanged += ControllerOnLocationChanged;
            LibraryElementModel = libraryElementModel;
            _imageContainerBounds = imageContainerBounds;

            UpdateImageBound(_imageContainerBounds);
      
            if (IsModifiable) { 
                _resizer = new ImageDetailRegionResizerRenderItem(this, resourceCreator);
                _resizer.ResizerDragged += ResizerOnResizerDragged;            
                Children.Add(_resizer);
            }
        }


        public void UpdateImageBound(Rect imageBounds)
        {
            _imageContainerBounds = imageBounds;
            _regionBounds = new Rect(0, 0, LibraryElementModel.NormalizedWidth * _bmpRect.Width, LibraryElementModel.NormalizedHeight * _bmpRect.Height);
            T = Matrix3x2.CreateTranslation((float)(_imageContainerBounds.X + LibraryElementModel.NormalizedX *  _bmpRect.Width), (float)(_imageContainerBounds.Y + LibraryElementModel.NormalizedY *  _bmpRect.Height));
        }

        private void ControllerOnLocationChanged(object sender, Point topLeft)
        {
            UpdateImageBound(_imageContainerBounds);
         }

        private void ControllerOnSizeChanged(object sender, double width, double height)
        {
            UpdateImageBound( new Rect(0, 0, LibraryElementModel.NormalizedWidth * _bmpRect.Width, LibraryElementModel.NormalizedHeight * _imageContainerBounds.Height));
        }

        public override void Dispose()
        {
            LibraryElementModel = null;
            if (_resizer != null)
                _resizer.ResizerDragged -= ResizerOnResizerDragged;
            _controller.SizeChanged -= ControllerOnSizeChanged;
            _controller.LocationChanged -= ControllerOnLocationChanged;
            _resizer?.Dispose();
            _resizer = null;
        }

        private void ResizerOnResizerDragged(Vector2 delta)
        {
            _controller.SizeChanged -= ControllerOnSizeChanged;
            _controller.LocationChanged -= ControllerOnLocationChanged;
            _regionBounds.Width += delta.X;
            _regionBounds.Height += delta.Y;
            RegionUpdated?.Invoke(this);
            _controller.SizeChanged += ControllerOnSizeChanged;
            _controller.LocationChanged += ControllerOnLocationChanged;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
    
            var orgTransform = ds.Transform;

            ds.Transform = GetTransform()*orgTransform;
   
            ds.DrawRectangle(new Rect(0,0,_regionBounds.Width, _regionBounds.Height), Color.FromArgb(0x33,0,0x55,0), 3);

            ds.Transform = orgTransform;

            if (_resizer != null)
                _resizer.T = Matrix3x2.CreateTranslation((float)(_regionBounds.X +_regionBounds.Width), (float)(_regionBounds.Y + _regionBounds.Height));
            base.Draw(ds);
        }

        public override void OnDragged(CanvasPointer pointer)
        {
            if (!IsModifiable)
                return;

            _controller.SizeChanged -= ControllerOnSizeChanged;
            _controller.LocationChanged -= ControllerOnLocationChanged;
            base.OnDragged(pointer);

            var nx = (float)Math.Max(_imageContainerBounds.X, Math.Min(_imageContainerBounds.X + _imageContainerBounds.Width - _regionBounds.Width, T.M31 + pointer.DeltaSinceLastUpdate.X));
            var ny = (float)Math.Max(_imageContainerBounds.Y, Math.Min(_imageContainerBounds.Y + _imageContainerBounds.Height - _regionBounds.Height, T.M32 + pointer.DeltaSinceLastUpdate.Y));
            T = Matrix3x2.CreateTranslation(nx, ny);
            RegionUpdated?.Invoke(this);

            _controller.SizeChanged += ControllerOnSizeChanged;
            _controller.LocationChanged += ControllerOnLocationChanged;
        }

        public override Rect GetMeasure()
        {
            return _regionBounds;
        }
    }
}
