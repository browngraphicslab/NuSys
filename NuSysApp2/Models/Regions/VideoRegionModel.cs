﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp2
{
    public class VideoRegionModel : RectangleRegion
    {
        public VideoRegionModel(string id) : base(id, ElementType.VideoRegion)
        {
        }
        public double Start { get; set; }
        public double End { get; set; }
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