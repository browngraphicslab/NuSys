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
    public class NodeMenuButtonRenderItem : BaseRenderItem
    {
        private CanvasBitmap _bmp;
        private string _iconUrl;


        public bool IsVisible { get; set; } = true;

        public NodeMenuButtonRenderItem( string iconUrl, CollectionRenderItem parent, CanvasAnimatedControl resourceCreator) :base(parent, resourceCreator)
        {
            _iconUrl = iconUrl;
        }

        public override void Dispose()
        {
            base.Dispose();
            _bmp.Dispose();
            _bmp = null;
        }

        public override async Task Load()
        {
            _bmp = await CanvasBitmap.LoadAsync(ResourceCreator, new Uri(_iconUrl), ResourceCreator.Dpi);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Win2dUtil.Invert(C) * S * C * T * ds.Transform;
            ds.FillCircle(new Vector2(0,0), 15, Colors.Chartreuse);

            // ds.FillCircle(new Rect { X = Postion.X, Y = 0, Width = _vm.Width, Height = _vm.Height }, Colors.Red);
            if (_bmp != null)
                ds.DrawImage(_bmp, new Rect(-15 + (30 - _bmp.Size.Width) / 2f, -15 + (30 - _bmp.Size.Height) / 2f, _bmp.Size.Width, _bmp.Size.Height)); 

            ds.Transform = orgTransform;
        }

        public override bool HitTest(Vector2 point)
        {
            var rect = new Rect(T.M31-15, T.M32 - 15, 30,30);
            return rect.Contains(point.ToPoint());
        }
    }
}
