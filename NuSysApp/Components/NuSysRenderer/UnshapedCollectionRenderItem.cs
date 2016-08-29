using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class UnshapedCollectionRenderItem : CollectionRenderItem
    {

        public UnshapedCollectionRenderItem(ElementCollectionViewModel vm, CollectionRenderItem parent, CanvasAnimatedControl canvas, bool interactionEnabled = false) : base(vm, parent, canvas, interactionEnabled)
        {
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = GetTransform() * ds.Transform;
            var boundaries = new Rect(0, 0, ViewModel.Width, ViewModel.Height);
           
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

            if (this != SessionController.Instance.SessionView.FreeFormViewer.InitialCollection)
                ds.DrawRectangle(boundaries, borderColor, borderWidth);

            var boundariesGeom = CanvasGeometry.CreateRectangle(ds, boundaries);
            using (ds.CreateLayer(1, boundariesGeom))
            {
                ds.Transform = GetCameraTransform() * ds.Transform;

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
            base.Draw(ds);
        }


    }
}
