using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp2
{
    public class TimeRegionModel: Region
    {
        public double Start { set; get; }
        public double End { set; get; }

        public delegate void TimeChangeHandler();
        public event TimeChangeHandler OnTimeChange;


        public TimeRegionModel(string id) : base(id,ElementType.AudioRegion)
        {
        }
        public override async Task UnPack(Message message)
        {
            if (message.ContainsKey("start"))
            {
                Start = message.GetDouble("start");
            }
            if (message.ContainsKey("end"))
            {
                End = message.GetDouble("end");
            }
            await base.UnPack(message);
        }
    }
}
