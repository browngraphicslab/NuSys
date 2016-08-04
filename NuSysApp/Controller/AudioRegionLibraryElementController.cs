using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class AudioRegionLibraryElementController : RegionLibraryElementController
    {
        public event RegionTimeChangedEventHandler TimeChanged;
        public delegate void RegionTimeChangedEventHandler(object sender, double start, double end);

        public AudioRegionModel AudioRegionModel
        {
            get { return base.LibraryElementModel  as AudioRegionModel;}
        }
        public AudioRegionLibraryElementController(AudioRegionModel model) : base(model)
        {

        }
        public void SetStartTime(double startTime)
        {
            AudioRegionModel.Start = startTime;
            TimeChanged?.Invoke(this, AudioRegionModel.Start, AudioRegionModel.End);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add("start", AudioRegionModel.Start);
            }
        }
        public void SetEndTime(double endTime)
        {
            AudioRegionModel.End = endTime;
            TimeChanged?.Invoke(this, AudioRegionModel.Start, AudioRegionModel.End);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add("end", AudioRegionModel.End);
            }
        }
        public override void UnPack(Message message)
        {
            SetBlockServerBoolean(true);
            if (message.ContainsKey("start"))
            {
                SetStartTime(message.GetDouble("start"));
            }
            if (message.ContainsKey("end"))
            {
                SetEndTime(message.GetDouble("end"));
            }
            base.UnPack(message);
            SetBlockServerBoolean(false);
        }
    }
}
