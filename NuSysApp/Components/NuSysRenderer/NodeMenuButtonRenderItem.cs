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
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using SharpDX.DirectWrite;

namespace NuSysApp
{
    public class NodeMenuButtonRenderItem : InteractiveBaseRenderItem
    {
        private CanvasBitmap _bmp;
        private string _iconUrl;

        public string Label
        {
            get { return _label.Text; }
            set { _label.Text = value; }
        }

        private TextboxUIElement _label;

        public NodeMenuButtonRenderItem( string iconUrl, BaseRenderItem parent, CanvasAnimatedControl resourceCreator) :base(parent, resourceCreator)
        {
            _iconUrl = iconUrl;
            _label = new TextboxUIElement(this, resourceCreator)
            {
                TextColor = Constants.RED,
                Text = "",
                FontFamily = UIDefaults.TextFont,
                FontSize = 10,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                TextVerticalAlignment = CanvasVerticalAlignment.Top,
                Background = Colors.Transparent
            };

            AddChild(_label);
            _label.Transform.LocalPosition = new Vector2(-50, 21);
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
         
            _bmp = null;
            base.Dispose();
        }

        public override async Task Load()
        {
            _bmp = await MediaUtil.LoadCanvasBitmapAsync(ResourceCreator, new Uri(_iconUrl));
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
            ds.FillCircle(new Vector2(0,0), 25, Constants.RED);
            var scaleFactor = 15/_bmp.Size.Width;
            // ds.FillCircle(new Rect { X = Postion.X, Y = 0, Width = _vm.Width, Height = _vm.Height }, Colors.Red);
            if (_bmp != null)
                //ds.DrawImage(_bmp, new Rect(-10 + (30 - _bmp.Size.Width * scaleFactor) / 2f, -10 + (30 - _bmp.Size.Height * scaleFactor) / 2f, _bmp.Size.Width * scaleFactor, _bmp.Size.Height * scaleFactor)); 
                ds.DrawImage(_bmp, new Rect(-15,-15,30,30));
            ds.Transform = orgTransform;
        }


        public override Rect GetLocalBounds()
        {
            return new Rect(-25, -25, 50, 50);
        }

    }
}
