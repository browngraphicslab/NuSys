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
using NusysIntermediate;
using Windows.Foundation;

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
            if (!IsDirty)
                return;
            base.Update();
            if (_recomputeShape) { 
                var model = (CollectionLibraryElementModel)ViewModel.Controller.LibraryElementModel;
                var pts = model.ShapePoints.Select(p => new Vector2((float)p.X, (float)p.Y));
                _shape = CanvasGeometry.CreatePolygon(ResourceCreator, pts.ToArray());
                _shapeBounds = _shape.ComputeBounds();
                _recomputeShape = false;
            }

            var boundaries = new Rect(0, 0, ViewModel.Width, ViewModel.Height);
            _rect = CanvasGeometry.CreateRectangle(ResourceCreator, boundaries);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (_shape == null || _rect == null)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = GetTransform() * ds.Transform;
            // ds.Transform = GetCameraTransform() * GetTransform() * ds.Transform;
            var boundaries = new Rect(0, 0, ViewModel.Width, ViewModel.Height);

            if (this != SessionController.Instance.SessionView.FreeFormViewer.InitialCollection)
            {
                if (!_vm.IsFinite) {
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
                   // ds.Transform = Matrix3x2.Identity;
                    ds.DrawRectangle(boundaries, borderColor, borderWidth);
                }
            }

            CanvasGeometry mask;
            if (_vm.IsFinite)
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
