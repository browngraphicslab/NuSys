using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;

using Windows.Foundation;
using Windows.UI.Input.Inking;
using NusysIntermediate;

namespace NuSysApp
{
    public class ShapedCollectionRenderItem : CollectionRenderItem
    {
        private CanvasGeometry _shape;
        private CanvasGeometry _rect;
        private ElementCollectionViewModel _vm;
        private bool _recomputeShape = true;
        private Rect _shapeBounds;

        private CanvasStrokeStyle _strokeStyle = new CanvasStrokeStyle
        {
            TransformBehavior = CanvasStrokeTransformBehavior.Fixed
        };
        
        public ShapedCollectionRenderItem(ElementCollectionViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi canvas, bool interactionEnabled = false) : base(vm, parent, canvas, interactionEnabled)
        {
            _vm = vm;
            _vm.Controller.SizeChanged += ControllerOnSizeChanged;
        }

        public override void Dispose()
        {
            if (_vm?.Controller != null)
            {
                _vm.Controller.SizeChanged -= ControllerOnSizeChanged;
            }
            _shape?.Dispose();
            _shape = null;
            _vm = null;
            _strokeStyle = null;
            base.Dispose();
        }

        private void ControllerOnSizeChanged(object source, double width, double height)
        {
            IsDirty = true;
        }

        public override void Update()
        {
            base.Update();

            if (!IsDirty)
                return;
          
            if ((_vm.IsFinite || _vm.IsShaped) && _recomputeShape) { 
                var model = (CollectionLibraryElementModel)ViewModel.Controller.LibraryElementModel;
                var pts = model.ShapePoints.Select(p => new Vector2((float)p.X, (float)p.Y)).ToArray();
                if (pts.Count() == 5)
                {
                    _shape = CanvasGeometry.CreateRectangle(ResourceCreator, pts[0].X, pts[0].Y, pts[2].X - pts[0].X,
                        pts[2].Y - pts[0].Y);
                }
                else
                {
                    _shape = CanvasGeometry.CreatePolygon(ResourceCreator, pts);
                }
                
                _shapeBounds = _shape.ComputeBounds();
                _recomputeShape = false;
            }

            var boundaries = new Rect(0, 0, ViewModel.Width, ViewModel.Height);
            _rect = CanvasGeometry.CreateRectangle(ResourceCreator, boundaries);
            IsDirty = false;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (_rect == null)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = GetTransform() * ds.Transform;
            // ds.Transform = GetCameraTransform() * GetTransform() * ds.Transform;
            var boundaries = new Rect(0, 0, ViewModel.Width, ViewModel.Height);


            Color borderColor;
            Color bgColor;
            float borderWidth = 4f;

            var isActive = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection == this;

            if (isActive)
            {
                borderColor = Color.FromArgb(0x66, 0, 102, 255);
                borderWidth = 8f;
                bgColor = Colors.White;
            }
            else
            {
                borderColor = Colors.Black;
                borderWidth = 4f;
                bgColor = Colors.LightGray;
            }


            // infinite unshaped
            if (!_vm.IsShaped && !_vm.IsFinite)
            {
                ds.Transform = GetTransform() * orgTransform;
                ds.FillGeometry(_rect, bgColor);
                ds.DrawRectangle(boundaries, borderColor, borderWidth);
            }
            // finite unshaped 
            else if (!_vm.IsShaped && _vm.IsFinite)
            {
                ds.Transform = GetCameraTransform() * GetTransform() * orgTransform;
                ds.FillGeometry(_shape, bgColor);
                ds.DrawRectangle(_shapeBounds, borderColor, borderWidth);
                ds.Transform = GetTransform() * orgTransform;
            }
            // finite shaped
            else if (_vm.IsFinite && _vm.IsShaped && isActive)
            {
              
                ds.Transform = GetCameraTransform() * GetTransform() * orgTransform;
                ds.FillGeometry(_shape, bgColor);
                ds.DrawGeometry(_shape, bgColor, 12f, _strokeStyle);
               // ds.DrawRectangle(_shapeBounds, borderColor, borderWidth);
                ds.Transform = GetTransform() * orgTransform;
            }
            //inifite shaped
            else if(!_vm.IsFinite && _vm.IsShaped)
            {
                ds.Transform = GetTransform() * orgTransform;
                ds.FillGeometry(_rect, bgColor);
                ds.DrawGeometry(_rect, borderColor, borderWidth);
                ds.Transform = GetTransform() * orgTransform;
            }
            /*
            // infinite collections
                if (_shape != null && !_vm.IsFinite )
            {
                ds.Transform = GetCameraTransform() * GetTransform() * orgTransform;
                ds.FillGeometry(_shape, Colors.White);
                ds.DrawRectangle(_shapeBounds, borderColor, borderWidth);
                ds.Transform = GetTransform() * orgTransform;
            }
            else if (!_vm.IsFinite)
            {
                ds.Transform = GetTransform() * orgTransform;
                ds.FillGeometry(_rect, Colors.White);
                ds.DrawRectangle(boundaries, borderColor, borderWidth);
            } */



            CanvasGeometry mask;
            if (_vm.IsFinite && _shape != null)
            {
                var scaleFactor = (float)boundaries.Width / (float)_shapeBounds.Width;;
                S = Matrix3x2.CreateScale(scaleFactor);
                ds.Transform = GetCameraTransform()*GetTransform()*orgTransform;
                mask = _shape;
            }
            else
            {
                ds.Transform = GetTransform() * orgTransform;
                mask = _rect;
            }     
            
            using (ds.CreateLayer(1, mask))
            {
                ds.Transform = GetCameraTransform() * GetTransform() * orgTransform;
                if (_vm.IsShaped)
                    ds.FillGeometry(_shape, Colors.DarkSeaGreen);
                foreach (var item in _renderItems0.ToArray())
                    item.Draw(ds);

                foreach (var item in _renderItems1.ToArray())
                    item.Draw(ds);

                foreach (var item in _renderItems2.ToArray())
                    item.Draw(ds);

                foreach (var item in _renderItems3.ToArray())
                    item.Draw(ds);

                ds.Transform = orgTransform;
            }

            ds.Transform = orgTransform;
            base.Draw(ds);
        }
    }
}
