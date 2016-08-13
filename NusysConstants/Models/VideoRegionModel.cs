<<<<<<< Updated upstream
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysIntermediate
=======
﻿namespace NusysIntermediate
>>>>>>> Stashed changes
{
    public class VideoRegionModel : RectangleRegion
    {
        public VideoRegionModel(string libraryId) : base(libraryId, NusysConstants.ElementType.VideoRegion)
        {
        }
        public double Start { get; set; }
        public double End { get; set; }

        public override void UnPackFromDatabaseKeys(Message message)
        {
            base.UnPackFromDatabaseKeys(message);
            if (message.ContainsKey(NusysConstants.TIMESPAN_REGION_START_KEY))
            {
                Start = message.GetDouble(NusysConstants.TIMESPAN_REGION_START_KEY);
            }
            if (message.ContainsKey(NusysConstants.TIMESPAN_REGION_END_KEY))
            {
                End = message.GetDouble(NusysConstants.TIMESPAN_REGION_END_KEY);
            }
        }
    }
}
