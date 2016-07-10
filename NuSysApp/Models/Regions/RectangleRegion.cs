using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class RectangleRegion : Region
    {
        // Very confusing!! Width = width ratio
        public RectangleRegion(Point topLeft, Point bottomRight, string name = "Untitled Rectangle") : base(name)
        {
            TopLeftPoint = topLeft;
            //BottomRightPoint = bottomRight;
            Width = bottomRight.X - topLeft.X;
            Height = bottomRight.Y - topLeft.Y;

            Type = RegionType.Rectangle;
        }

        public Point TopLeftPoint { set; get; }
        public double Width { set; get; }
        public double Height { set; get; }

    }
}
