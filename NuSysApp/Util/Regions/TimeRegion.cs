using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    class TimeRegion : Region
    {
        public TimeRegion(double time1, double time2)
        {
            Time1 = time1;
            Time2 = time2; 
        }
        public double Time1 { set; get; }
        public double Time2 { set; get; }
    }
}
