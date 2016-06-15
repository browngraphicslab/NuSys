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
        public RectangleRegion(Point p1, Point p2)
        {
            Point1 = p1;
            Point2 = p2;
        }

        public Point Point1 { set; get; }
        public Point Point2 { set; get; }
    }
}
