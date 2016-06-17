using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public abstract class Region
    {
        public enum RegionType
        {
            Rectangle,
            Time,
            Compound
        }
        public RegionType Type { get; set; }
        public string Name { get; set; }

        public Region(string name)
        {
            Name = name;
        }

    }
}