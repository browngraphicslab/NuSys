using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    class CompoundRegion : Region
    {
        public CompoundRegion(params Region[] regions)
        {
            Regions = regions;
        }
        public Region[] Regions { set; get; }
    }
}
