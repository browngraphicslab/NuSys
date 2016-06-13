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
        public HashSet<string> RegionStrings;
        public HashSet<string> InkStrings;
        public LoadContentEventArgs(string data = null, HashSet<string> regionStrings = null, HashSet<string> inkStrings = null)
        {
            Data = data;
            RegionStrings = regionStrings;
            InkStrings = inkStrings;
        }
    }
}
