using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp2
{
    public class VideoRegionController : RectangleRegionController
    {
        public delegate void IntervalChangedEventHandler(object sender, double start, double end);

        public event IntervalChangedEventHandler IntervalChanged;
        public VideoRegionModel VideoRegionModel
        {
            get
            {
                return LibraryElementModel as VideoRegionModel;
            }
        }
        public VideoRegionController(VideoRegionModel model) : base(model)
        {
            
        }

        public void SetStartTime(double startTime)
        {
            VideoRegionModel.Start = startTime;
            IntervalChanged?.Invoke(this, VideoRegionModel.Start, VideoRegionModel.End);
            _debouncingDictionary.Add("start", startTime);
        }
        public void SetEndTime(double endTime)
        {
            VideoRegionModel.End = endTime;
            IntervalChanged?.Invoke(this, VideoRegionModel.Start, VideoRegionModel.End);
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
