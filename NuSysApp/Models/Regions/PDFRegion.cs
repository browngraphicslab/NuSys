﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class PdfRegion : RectangleRegion 
    {
        public Point TopLeftPoint { set; get; }
        public Point BottomRightPoint { set; get; }
        public int PageLocation { get; set; }
        public PdfRegion(Point p1, Point p2, int pageLocation, string name = "Untitled Rectangle") : base(p1,p2,name)
        {
            PageLocation = pageLocation;
            Type = RegionType.Pdf;
        }
    }
}
