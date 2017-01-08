using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class GridLayoutTester : ResizeableWindowUIElement
    {

        private StackLayoutManager slm;

        public GridLayoutTester(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            var GridLayoutManager = new GridLayoutManager(this, ResourceCreator)
            {
                Width = 800,
                Height = 900
            };
            GridLayoutManager.AddColumns(new List<float> { 1, 2, 3, 4 });
            GridLayoutManager.AddRows(new List<float> { 1, 2, 3, 4 });

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    GridLayoutManager.AddElement(new RectangleUIElement(this, ResourceCreator)
                    {
                        BorderWidth = 2,
                        Bordercolor = Colors.Green
                    }, row, col, relativeWidth: .8f, relativeHeight: .8f);
                }
            }
            AddChild(GridLayoutManager);

            slm = new StackLayoutManager()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                TopMargin = TopBarHeight
            };
            slm.AddElement(GridLayoutManager);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            slm.SetSize(Width, Height);
            slm.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
