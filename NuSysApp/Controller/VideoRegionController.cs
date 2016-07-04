using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class VideoRegionController : RectangleRegionController
    {
        public delegate void IntervalChangedEventHandler(object sender, double start, double end);

        public event IntervalChangedEventHandler IntervalChanged;
        public VideoRegionModel VideoRegionModel
        {
            get
            {
                return base.Model as VideoRegionModel;
            }
        }
        public VideoRegionController(VideoRegionModel model) : base(model)
        {
            
        }

        public void SetStartTime(double startTime)
        {
            VideoRegionModel.Start = startTime;
            IntervalChanged?.Invoke(this, VideoRegionModel.Start, VideoRegionModel.End);
            UpdateServer();
        }
        public void SetEndTime(double endTime)
        {
            VideoRegionModel.Start = endTime;
            IntervalChanged?.Invoke(this, VideoRegionModel.Start, VideoRegionModel.End);
            UpdateServer();
        }

        public override void UnPack(Region region)
        {
            SetBlockServerBoolean(true);
            var r = region as VideoRegionModel;
            if (r != null)
            {
                SetStartTime(r.Start);
                SetEndTime(r.End);
            }
            base.UnPack(region);
            SetBlockServerBoolean(false);
        }
    }
}
