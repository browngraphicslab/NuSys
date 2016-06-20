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
            BottonRight = bottomright;
            Start = start;
            End = end;
             
        }
        Point TopLeft { get; set; }
        Point BottonRight { get; set; }
        double Start { get; set; }
        double End { get; set; }

    }
}
