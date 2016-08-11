using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public enum ButtonType
    {
        Delete, Presentation
    }
    public class NodeMenuButtonRenderItem : BaseRenderItem
    {
        private CanvasBitmap _bmp;
        public Vector2 Postion;
        private ButtonType _type;

        public NodeMenuButtonRenderItem( CollectionRenderItem parent, CanvasAnimatedControl resourceCreator, ButtonType type) :base(parent, resourceCreator)
        {
            _type = type;
        }

        public override void Dispose()
        {
            base.Dispose();
            _bmp.Dispose();
            _bmp = null;
        }

        public override async Task Load()
        {
            return;
            /*
            var url = _vm.Controller.LibraryElementController.GetSource();
            _bmp = await CanvasBitmap.LoadAsync(ResourceCreator, url, ResourceCreator.Dpi);
            _vm.Controller.SetSize(_bmp.Size.Width, _bmp.Size.Height);
            */
        }

        public override async void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Win2dUtil.Invert(C) * S * C * T * ds.Transform;
            ds.FillCircle(Postion, 15, Colors.AntiqueWhite);
            if (_type == ButtonType.Delete)
            {
                var url = new Uri("ms-appx:///Assets/icon_delete_color.png");
                var icon = await CanvasBitmap.LoadAsync(ResourceCreator, url);
                ds.DrawImage(icon, new Rect { X = 0, Y = 0, Width = 25, Height = 25 });
            }
            if (_type == ButtonType.Presentation)
            {
                var url = new Uri("ms-appx:///Assets/present icon.png");
                var icon = await CanvasBitmap.LoadAsync(ResourceCreator, url);
                ds.DrawImage(icon, new Rect { X = 0, Y = 0, Width = 25, Height = 25 });
            }
            // ds.FillCircle(new Rect { X = Postion.X, Y = 0, Width = _vm.Width, Height = _vm.Height }, Colors.Red);

          //  if (_bmp != null)
          //      ds.DrawImage(_bmp, new Rect { X = 0, Y = 0, Width = _vm.Width, Height = _vm.Height});

            ds.Transform = orgTransform;
        }

        public override bool HitTest(Vector2 point)
        {
            var rect = new Rect(Postion.X-15, Postion.Y-15, 30,30);
            return rect.Contains(point.ToPoint());
        }
    }
}
