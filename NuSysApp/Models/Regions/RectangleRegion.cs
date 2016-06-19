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
        public RectangleRegion(Point p1, Point p2, string name = "Untitled Rectangle") : base(name)
        {
            TopLeftPoint = p1;
            BottomRightPoint = p2;
            Type = RegionType.Rectangle;
        }

        public Point TopLeftPoint { set; get; }
        public Point BottomRightPoint { set; get; }
    }
}
