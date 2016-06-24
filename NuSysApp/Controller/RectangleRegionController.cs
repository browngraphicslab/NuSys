using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class RectangleRegionController : RegionController
    {
        public event RegionSizeChangedEventHandler SizeChanged;
        public delegate void RegionSizeChangedEventHandler(object sender, Point topLeft, Point bottomRight);

        public RectangleRegion Model
        {
            get { return base.Model  as RectangleRegion;}
        }
        public RectangleRegionController(RectangleRegion model) : base(model)
        {
        }
        public void ChangeSize(Point topLeft, Point bottomRight)
        {
            Model.TopLeftPoint = topLeft;
            Model.BottomRightPoint = bottomRight;
        }
    }
}
