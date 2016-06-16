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
        public delegate void selectHandler(TimeRegionModel playbackElement);
        public event selectHandler OnSelect;

        public delegate void TimeChangeHandler();
        public event TimeChangeHandler OnTimeChange;

        public delegate void deselectHandler(TimeRegionModel playbackElement);
        public event deselectHandler OnDeselect;
        private Boolean selected;
        

        public TimeRegionModel(string name, double start, double end) : base(name)
        {
        
            Start = start;
            End = end;
            Type = RegionType.Time;
        }

        public void Select()
        {
            OnSelect?.Invoke(this);
        }

        public void Deselect()
        {
            OnDeselect?.Invoke(this);
        }

    }
}
