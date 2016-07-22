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
        public VideoRegionModel(string libraryId) : base(libraryId, ElementType.VideoRegion)
        {
        }
        public double Start { get; set; }
        public double End { get; set; }

    }
}
