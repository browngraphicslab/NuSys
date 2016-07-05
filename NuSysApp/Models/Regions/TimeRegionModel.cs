using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class TimeRegionModel: Region
    {
        public double Start { set; get; }
        public double End { set; get; }

        public delegate void TimeChangeHandler();
        public event TimeChangeHandler OnTimeChange;


        public TimeRegionModel(string name, double start, double end) : base(name)
        {
            Start = start;
            End = end;
            Type = RegionType.Time;
        }
    }
}
