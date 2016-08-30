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
        
        public ShapedCollectionRenderItem(ElementCollectionViewModel vm, CollectionRenderItem parent, CanvasAnimatedControl canvas, bool interactionEnabled = false) : base(vm, parent, canvas, interactionEnabled)
        {
            _vm = vm;
            _vm.Controller.SizeChanged += ControllerOnSizeChanged;
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


            if ((!_vm.IsFinite && this != SessionController.Instance.SessionView.FreeFormViewer.InitialCollection) ||
                (!_vm.IsShaped && _vm.IsFinite))
            {
                Color borderColor;
                float borderWidth = 4f;

                if (SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection == this)
                {
                    borderColor = Color.FromArgb(255, 0, 102, 255);
                    borderWidth = 6f;
                }
                else
                {
                    borderColor = Colors.Black;
                    borderWidth = 4f;
                }

                if ((!_vm.IsShaped && _vm.IsFinite))
                {
                    ds.Transform = GetCameraTransform() * GetTransform() * orgTransform;
                    ds.DrawRectangle(_shapeBounds,borderColor, borderWidth);
                    ds.Transform = GetTransform() * orgTransform;
                }
                else
                    ds.DrawRectangle(boundaries, borderColor, borderWidth);
            }

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
                if (_vm.IsShaped || (_vm.IsFinite && !_vm.IsShaped))
                    ds.FillGeometry(_shape, Colors.DarkRed);
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
