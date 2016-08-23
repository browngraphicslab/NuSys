using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp2
{
    public class AudioRegionController : RegionController
    {
        public event RegionTimeChangedEventHandler TimeChanged;
        public delegate void RegionTimeChangedEventHandler(object sender, double start, double end);

        public TimeRegionModel AudioRegionModel
        {
            get
            {
                Debug.Assert(LibraryElementModel is TimeRegionModel);
                return LibraryElementModel as TimeRegionModel;
            }
        }
        public AudioRegionController(TimeRegionModel model) : base(model)
        {

        }
        public void SetStartTime(double startTime)
        {
            AudioRegionModel.Start = startTime;
            TimeChanged?.Invoke(this, AudioRegionModel.Start, AudioRegionModel.End);
            _debouncingDictionary.Add("start", startTime);
        }
        public void SetEndTime(double endTime)
        {
            AudioRegionModel.End = endTime;
            TimeChanged?.Invoke(this, AudioRegionModel.Start, AudioRegionModel.End);
            _debouncingDictionary.Add("end", endTime);
        }

        public override void UnPack(Message message)
        {
            SetBlockServerInteraction(true);
            if (message.ContainsKey("start"))
            {
                SetStartTime(message.GetDouble("start"));
            }
            if (message.ContainsKey("end"))
            {
                SetEndTime(message.GetDouble("end"));
            }
            base.UnPack(message);
            SetBlockServerInteraction(false);
        }
    }
}
