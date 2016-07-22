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

        public RectangleRegion(string libraryId, ElementType type) : base(libraryId, type)
        {
        }

        public Point TopLeftPoint { set; get; }
        public double Width { set; get; }
        public double Height { set; get; }
    }
}
