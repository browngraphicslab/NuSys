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
        public string Id { get; set; }

        public Region(string name = "Untitled Region")
        {
            Id = SessionController.Instance.GenerateId();
            Name = name;
        }

    }
}