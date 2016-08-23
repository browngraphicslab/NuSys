using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class PdfElementRenderItem : ElementRenderItem
    {
        private PdfNodeViewModel _vm;
        private CanvasBitmap _bmp;

        public PdfElementRenderItem(PdfNodeViewModel vm, CollectionRenderItem parent, CanvasAnimatedControl resourceCreator):base(vm, parent, resourceCreator)
        {
            _vm = vm;
            _vm.PropertyChanged += OnPropertyChanged;
        }

        public override void Dispose()
        {
            base.Dispose();
            _vm = null;
            _bmp.Dispose();
            _bmp = null;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            // TODO: fix!   
            /*
            if (_bmp == null && args.PropertyName == "ImageSource")
            {
                _bmp?.Dispose();    
                _bmp = CanvasBitmap.CreateFromBytes(ResourceCreator, _vm.Buffer, (int)_vm.PdfSize.Width, (int)_vm.PdfSize.Height, DirectXPixelFormat.B8G8R8A8UIntNormalized);
                _vm.Controller.SetSize(_bmp.Size.Width, _bmp.Size.Height);
                _vm.DisposeData();
            }
            */
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);
            var orgTransform = ds.Transform;
            ds.Transform = Win2dUtil.Invert(C) * S * C * T * ds.Transform;
            if (_bmp != null)
            {
                ds.DrawImage(_bmp, new Rect {X = 0, Y = 0, Width = _vm.Width, Height = _vm.Height});
            }
            ds.Transform = orgTransform;
        }
    }
}
