using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    class CompoundRegion : Region
    {
        public CompoundRegion(string name, params Region[] regions) : base(name)
        {
            Regions = regions;
            Type = RegionType.Compound;
        }
        public Region[] Regions { set; get; }
    }
}