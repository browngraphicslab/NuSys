using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class ScrollingCanvasTester : ResizeableWindowUIElement
    {
        private ScrollingCanvas _sc;

        private StackLayoutManager _scLM;

        public ScrollingCanvasTester(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

            _sc = new ScrollingCanvas(this, ResourceCreator, ScrollingCanvas.ScrollOrientation.Both)
            {
                Width = 300,
                Height = 300,
                ScrollAreaSize = new Size(1000, 1000),
                Background = Colors.Blue
            };
            for (int i = 1; i < 10; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    _sc.AddElement(new ButtonUIElement(this, ResourceCreator)
                    {
                        Width = 50,
                        Height = 50,
                        BorderWidth = 1,
                        BorderColor = Colors.Red,
                        ButtonText = $"({i}, {j})",
                        ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center,
                        ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center
                    }, new Vector2(i * 100, j * 100));
                }
            }
            AddChild(_sc);

            _scLM = new StackLayoutManager
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            _scLM.AddElement(_sc);

            Width = 300;
            Height = 300;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _scLM.SetSize(Width, Height);
            _scLM.TopMargin = TopBarHeight;
            _scLM.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
