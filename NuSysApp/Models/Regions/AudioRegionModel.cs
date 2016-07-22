using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class AudioRegionModel: Region
    {
        public double Start { set; get; }
        public double End { set; get; }

        public delegate void TimeChangeHandler();
        public event TimeChangeHandler OnTimeChange;


        public AudioRegionModel(string libraryId) : base(libraryId, ElementType.AudioRegion)
        {
        }
    }
}
