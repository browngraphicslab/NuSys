﻿using System;
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
    public class ImageElementRenderItem : ElementRenderItem
    {
        private ImageElementViewModel _vm;
        private CanvasBitmap _bmp;

        public ImageElementRenderItem(ImageElementViewModel vm, CanvasAnimatedControl resourceCreator) :base(vm, resourceCreator)
        {
            _vm = vm;
        }

        public override void Dispose()
        {
            base.Dispose();
            _vm = null;
            _bmp.Dispose();
            _bmp = null;
        }

        public override async Task Load()
        {
            var url = _vm.Controller.LibraryElementController.GetSource();
            _bmp = await CanvasBitmap.LoadAsync(ResourceCreator, url, ResourceCreator.Dpi);
            _vm.Controller.SetSize(_bmp.Size.Width, _bmp.Size.Height);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);
            if (_bmp != null)
                ds.DrawImage(_bmp, new Rect { X = _vm.X, Y = _vm.Y, Width = _vm.Width, Height = _vm.Height});
        }
    }
}
