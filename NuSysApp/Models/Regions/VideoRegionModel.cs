using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class VideoRegionModel : RectangleRegion
    {
        public VideoRegionModel(Point topLeft, Point bottomRight, double start = .25, double end = .75, string name="Untitled Region") : base(topLeft, bottomRight, name)
        {
            Start = start;
            End = end;
            Type = RegionType.Video; 
        }
        public double Start { get; set; }
        public double End { get; set; }

    }
}
