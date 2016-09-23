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
using Microsoft.Graphics.Canvas.Geometry;
using NetTopologySuite.Geometries;
using NusysIntermediate;
using Point = Windows.Foundation.Point;

namespace NuSysApp
{
    public class ImageDetailRegionRenderItem : InteractiveBaseRenderItem
    {
        private ImageLibraryElementController _controller;
        public Size Size;
        public delegate void RegionUpdatedHandler(ImageDetailRegionRenderItem regionRegion, Vector2 regionBounds);
        public delegate void RegionInteractionHandler(ImageDetailRegionRenderItem regionRegion);
        public event RegionInteractionHandler RegionPressed;
        public event RegionInteractionHandler RegionReleased;

        public event RegionUpdatedHandler RegionMoved;
        public event RegionUpdatedHandler RegionResized;

        private ImageDetailRegionResizerRenderItem _resizer;
        private Rect _cropAreaNormalized;
        private Rect _bitmap;
        private double _totalScale;
        public ImageLibraryElementModel LibraryElementModel { get; set; }

        private CanvasStrokeStyle _strokeStyle = new CanvasStrokeStyle
        {
            TransformBehavior = CanvasStrokeTransformBehavior.Fixed
        };

        private bool _isModifiable;

        public bool IsModifiable { get; set; }

        public ImageDetailRegionRenderItem(ImageLibraryElementModel libraryElementModel, Rect cropAreaNormalized, Rect bitmap, double totalScale, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, bool isModifiable = true) : base(parent, resourceCreator)
        {
            _totalScale = totalScale;
            _isModifiable = isModifiable;
            _bitmap = bitmap;
            _cropAreaNormalized = cropAreaNormalized;
            LibraryElementModel = libraryElementModel;


            var rect = new Rect(LibraryElementModel.NormalizedX,
                                LibraryElementModel.NormalizedY,
                                LibraryElementModel.NormalizedWidth,
                                LibraryElementModel.NormalizedHeight);

            _isModifiable = _isModifiable && IsFullyContained(rect);
            if (_isModifiable) { 
                _resizer = new ImageDetailRegionResizerRenderItem(this, ResourceCreator);
                _resizer.ResizerDragged += ResizerOnResizerDragged;
                _resizer.ResizerDragStarted += ResizerOnResizerDragStarted;
                _resizer.ResizerDragEnded += ResizerOnResizerDragEnded;
                AddChild(_resizer);
            }

            _controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModel.LibraryElementId) as ImageLibraryElementController;
            _controller.SizeChanged += ControllerOnSizeChanged;
            _controller.LocationChanged += ControllerOnLocationChanged;
            
            UpdateImageBound(_totalScale);
        }

        private void ResizerOnResizerDragEnded()
        {
            RegionReleased?.Invoke(this);
        }

        private void ResizerOnResizerDragStarted()
        {
            RegionPressed?.Invoke(this);
        }

        public void UpdateImageBound(double scale)
        {
            _totalScale = scale;

            var rect = new Rect(LibraryElementModel.NormalizedX - _cropAreaNormalized.X,
                                LibraryElementModel.NormalizedY - _cropAreaNormalized.Y, 
                                LibraryElementModel.NormalizedWidth, 
                                LibraryElementModel.NormalizedHeight);

            var tx = rect.X * scale * _bitmap.Width;
            var ty = rect.Y * scale * _bitmap.Height;

            var tw = rect.Width * scale * _bitmap.Width;
            var th = rect.Height * scale * _bitmap.Height;

            if (_resizer != null) { 
                th = Math.Max(th, _resizer.GetLocalBounds().Height);
                tw = Math.Max(tw, _resizer.GetLocalBounds().Width);
            }

            Size = new Size(tw, th);
            Transform.LocalPosition = new Vector2((float)(tx), (float)(ty));
        }

        private void ControllerOnLocationChanged(object sender, Point topLeft)
        {
            UpdateImageBound(_totalScale);
         }

        private void ControllerOnSizeChanged(object sender, double width, double height)
        {
            UpdateImageBound(_totalScale);
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
            if (!_isModifiable)
                return;
            RegionResized?.Invoke(this, delta);
        }

        private bool IsFullyContained(Rect rect)
        {
            if (rect.X >= _cropAreaNormalized.X
                && rect.Y >= _cropAreaNormalized.Y
                && rect.X + rect.Width <= _cropAreaNormalized.X + _cropAreaNormalized.Width
                && rect.Y + rect.Height <= _cropAreaNormalized.Y + _cropAreaNormalized.Height)
            {
                return true;
            }
            return false;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_resizer != null)
                _resizer.Transform.LocalPosition = new Vector2((float)(Size.Width), (float)(Size.Height));
            base.Update(parentLocalToScreenTransform);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            base.Draw(ds);
            ds.DrawRectangle(new Rect(0, 0, Size.Width, Size.Height), Color.FromArgb(255, 200, 200, 200), 2, _strokeStyle);
            ds.Transform = orgTransform;
        }

        public override void OnDragged(CanvasPointer pointer)
        {
            if (!_isModifiable)
                return;         
            RegionMoved?.Invoke(this, pointer.DeltaSinceLastUpdate);
        }

        public override void OnPressed(CanvasPointer pointer)
        {
            RegionPressed?.Invoke(this);
        }

        public override void OnReleased(CanvasPointer pointer)
        {
            RegionReleased?.Invoke(this);
        }

        public override void OnDoubleTapped(CanvasPointer pointer)
        {
            SessionController.Instance.SessionView.ShowDetailView(_controller);
        }

        public override Rect GetLocalBounds()
        {
            return new Rect(0,0, Size.Width, Size.Height);
        }
    }
}
