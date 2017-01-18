using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Text;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

namespace NuSysApp
{
    public class TagRenderItem : BaseRenderItem
    {
        private CanvasTextLayout _textLayout;
        private Rect _measurement;
        public string Text { get; private set; }

        public TagRenderItem(string text, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
            : base(parent, resourceCreator)
        {
            Text = text;
            var format = new CanvasTextFormat
            {
                FontSize = 15f,
                FontFamily = UIDefaults.TextFont,
            };

            _textLayout = new CanvasTextLayout(resourceCreator, text, format, 500, 500);

            _measurement = _textLayout.LayoutBounds;
            _measurement.Union(_textLayout.DrawBounds);
            _measurement = Win2dUtil.AddPadding(_measurement, 5);
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            _textLayout.Dispose();
            _textLayout = null;
            Text = null;
            base.Dispose();
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            base.Draw(ds);
            ds.Transform = Transform.LocalToScreenMatrix;
            ds.FillRectangle(_measurement, Constants.RED);
            ds.DrawTextLayout(_textLayout, 5, 5, Colors.White);
        }

        public override Rect GetLocalBounds()
        {
            return _measurement;
        }
    }
}
