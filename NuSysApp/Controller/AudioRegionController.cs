using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class AudioRegionController : RegionController
    {
        public event RegionSizeChangedEventHandler SizeChanged;
        public delegate void RegionSizeChangedEventHandler(object sender, Point topLeft, Point bottomRight);

        public TimeRegionModel Model
        {
            get { return base.Model  as TimeRegionModel;}
        }
        public AudioRegionController(TimeRegionModel model) : base(model)
        {
        }
        public void ChangeSize(double start, double end)
        {
            Model.Start = start;
            Model.End = end;
        }
    }
}
