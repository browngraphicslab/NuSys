using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class PdfRegion : Region
    {
        public PdfRegion(string name, Point p1, Point p2, int pageLocation) : base(name)
        {
            Point1 = p1;
            Point2 = p2;
            PageLocation = pageLocation;
        }

        public Point Point1 { set; get; }
        public Point Point2 { set; get; }
        public int PageLocation { get; set; }
    }
}
