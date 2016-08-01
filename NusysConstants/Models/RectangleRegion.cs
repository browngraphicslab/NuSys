using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class RectangleRegion : Region
    {

        public RectangleRegion(string libraryId, NusysConstants.ElementType type) : base(libraryId, type)
        {
        }

        public PointModel TopLeftPoint { set; get; }
        public double Width { set; get; }
        public double Height { set; get; }
    }
}
