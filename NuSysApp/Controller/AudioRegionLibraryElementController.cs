using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class AudioRegionLibraryElementController : RegionLibraryElementController
    {
        public event RegionTimeChangedEventHandler TimeChanged;
        public delegate void RegionTimeChangedEventHandler(object sender, double start, double end);

        public TimeRegionModel Model
        {
            get { return base.Model  as TimeRegionModel;}
        }
        public AudioRegionLibraryElementController(TimeRegionModel model) : base(model)
        {

        }
        public void ChangeEndPoints(double start, double end)
        {
            Model.Start = start;
            Model.End = end;
            TimeChanged?.Invoke(this, start, end);
            UpdateServer();
        }
    }
}
