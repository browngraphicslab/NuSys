using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class VideoRegionModel : Region
    {
        public VideoRegionModel(Point topleft, Point bottomright, double start, double end, string name="untitled node") : base(name)
        {
            TopLeft = topleft;
            BottomRight = bottomright;
            Start = start;
            End = end;
            Type = RegionType.Video; 
        }
        public Point TopLeft { get; set; }
        public Point BottomRight { get; set; }
        public double Start { get; set; }
        public double End { get; set; }

    }
}
