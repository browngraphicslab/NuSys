using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class NuSessionViewer : RectangleUIElement
    {
        private FloatingMenu _floatingMenu;

        public NuSessionViewer(BaseRenderItem parent, CanvasAnimatedControl canvas) : base(parent, canvas)
        {
            Background = Colors.Transparent;

            _floatingMenu = new FloatingMenu(this, canvas);

            AddChild(_floatingMenu);

            Canvas.SizeChanged += OnMainCanvasSizeChanged;
        }

        private void OnMainCanvasSizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            Width = (float) e.NewSize.Width;
            Height = (float) e.NewSize.Height;

            _floatingMenu.Transform.LocalPosition = new Vector2(Width / 4 - _floatingMenu.Width/2, Height / 4 - _floatingMenu.Height/2);

        }

        public override void Dispose()
        {
            Canvas.SizeChanged -= OnMainCanvasSizeChanged;

            base.Dispose();
        }
    }
}
