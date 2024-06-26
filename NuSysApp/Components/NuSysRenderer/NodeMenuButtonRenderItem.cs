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
    public class NodeMenuButtonRenderItem : InteractiveBaseRenderItem
    {
        private CanvasBitmap _bmp;
        private string _iconUrl;

        public NodeMenuButtonRenderItem( string iconUrl, BaseRenderItem parent, CanvasAnimatedControl resourceCreator) :base(parent, resourceCreator)
        {
            _iconUrl = iconUrl;
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
         
            _bmp.Dispose();
            _bmp = null;
            base.Dispose();
        }

        public override async Task Load()
        {
            _bmp = await CanvasBitmap.LoadAsync(ResourceCreator, new Uri(_iconUrl));
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            if (!IsVisible)
                return;

            base.Draw(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            ds.FillCircle(new Vector2(0,0), 15, Color.FromArgb(0xFF, 0x6B,0x93,0x97));
            var scaleFactor = 15/_bmp.Size.Width;
            // ds.FillCircle(new Rect { X = Postion.X, Y = 0, Width = _vm.Width, Height = _vm.Height }, Colors.Red);
            if (_bmp != null)
                ds.DrawImage(_bmp, new Rect(-15 + (30 - _bmp.Size.Width * scaleFactor) / 2f, -15 + (30 - _bmp.Size.Height * scaleFactor) / 2f, _bmp.Size.Width * scaleFactor, _bmp.Size.Height * scaleFactor)); 

            ds.Transform = orgTransform;
        }


        public override Rect GetLocalBounds()
        {
            return new Rect(-15, -15, 30, 30);
        }

    }
}
