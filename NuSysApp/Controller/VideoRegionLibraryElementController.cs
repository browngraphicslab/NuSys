using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class VideoRegionLibraryElementController : RectangleRegionLibraryElementController
    {
        public delegate void IntervalChangedEventHandler(object sender, double start, double end);

        public event IntervalChangedEventHandler IntervalChanged;
        public VideoRegionModel VideoRegionModel
        {
            get
            {
                return base.LibraryElementModel as VideoRegionModel;
            }
        }
        public VideoRegionLibraryElementController(VideoRegionModel model) : base(model)
        {
            
        }

        public void SetStartTime(double startTime)
        {
            VideoRegionModel.Start = startTime;
            IntervalChanged?.Invoke(this, VideoRegionModel.Start, VideoRegionModel.End);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add("start", VideoRegionModel.Start);
            }
        }
        public void SetEndTime(double endTime)
        {
            VideoRegionModel.End = endTime;
            IntervalChanged?.Invoke(this, VideoRegionModel.Start, VideoRegionModel.End);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add("end", VideoRegionModel.Start);
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
