using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LoadContentEventArgs
    {
        public string Data;
        public HashSet<Region> RegionStrings;
        public HashSet<string> InkStrings;
        public LoadContentEventArgs(string data = null, HashSet<Region> regionStrings = null, HashSet<string> inkStrings = null)
        {
            Data = data;
            RegionStrings = regionStrings;
            InkStrings = inkStrings;
        }
    }
}
