﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class RectangleRegion : Region
    {
        public RectangleRegion(Point topLeft, Point bottomRight, string name = "Untitled Rectangle") : base(name)
        {
            TopLeftPoint = topLeft;
            BottomRightPoint = bottomRight;
            Type = RegionType.Rectangle;
        }

        public Point TopLeftPoint { set; get; }
        public Point BottomRightPoint { set; get; }
    }
}
