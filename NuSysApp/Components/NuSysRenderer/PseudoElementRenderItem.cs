using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using WinRTXamlToolkit.Controls.Extensions;

namespace NuSysApp
{
    public class PseudoElementRenderItem : ElementRenderItem
    {
        public Rect Rect { get; set; } = new Rect();
        public ITool Tool;

        public PseudoElementRenderItem(ITool tool, CollectionRenderItem parent,
            CanvasAnimatedControl resourceCreator) : base(null, parent, resourceCreator)
        {
            Tool = tool;
            Tool.ToolAnchorChanged += OnToolAnchorChanged;

            var s = (FrameworkElement) tool;
            var t = (CompositeTransform) s.RenderTransform;
            Rect = new Rect(t.TranslateX+60, t.TranslateY, s.Width, s.Height);
        }

        private void OnToolAnchorChanged(object sender, Point2d point2D)
        {
            var s = (FrameworkElement) sender;
            var t = (CompositeTransform) s.RenderTransform;
            Rect = new Rect(t.TranslateX+60, t.TranslateY, s.ActualWidth, s.ActualHeight);
        }

        public override void Update()
        {
        }

        public override void Draw(CanvasDrawingSession ds)
        {

            ds.DrawRectangle(Rect, Colors.Red);
        }

        public override bool HitTest(Vector2 point)
        {
            return Rect.Contains(new Point(point.X, point.Y));
        }
    }
}
