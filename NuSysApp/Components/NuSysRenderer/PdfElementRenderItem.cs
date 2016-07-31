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

        public PdfElementRenderItem(PdfNodeViewModel vm, CanvasAnimatedControl resourceCreator):base(vm, resourceCreator)
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

            if (_bmp == null && args.PropertyName == "ImageSource")
            {
                _bmp?.Dispose();
                _bmp = CanvasBitmap.CreateFromBytes(ResourceCreator, _vm.Buffer, (int)_vm.PdfSize.Width, (int)_vm.PdfSize.Height, DirectXPixelFormat.B8G8R8A8UIntNormalized);
                _vm.Controller.SetSize(_bmp.Size.Width, _bmp.Size.Height);
                _vm.DisposeData();
            }
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            if (_bmp != null)
            {
                ds.DrawImage(_bmp, new Rect {X = _vm.X, Y = _vm.Y, Width = _vm.Width, Height = _vm.Height});
            }
        }
    }
}
