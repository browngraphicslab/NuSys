using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class VideoRegionController : RegionController
    {
        public event RegionSizeChangedEventHandler SizeChanged;
        public delegate void RegionSizeChangedEventHandler(object sender, Point topLeft, Point bottomRight);

        public VideoRegionModel Model
        {
            get { return base.Model  as VideoRegionModel;}
        }
        public VideoRegionController(VideoRegionModel model) : base(model)
        {
        }
        public void ChangeSize(double start, double end, Point topLeft, Point bottomRight)
        {
            Model.Start = start;
            Model.End = end;
            Model.TopLeft = topLeft;
            Model.BottomRight = bottomRight;
        }
    }
}
