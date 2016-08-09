using NusysIntermediate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    class CreateNewRectangleRegionLibraryElementRequestArgs : CreateNewRegionLibraryElementRequestArgs
    {
        public PointModel TopLeftPoint {get; set;}
        public double RegionWidth { get; set; }
        public double RegionHeight { get; set; }

        public override Message PackToRequestKeys()
        {
            var message = base.PackToRequestKeys();

            Debug.Assert(RegionWidth != null);
            Debug.Assert(RegionHeight != null);
            Debug.Assert(TopLeftPoint != null);

            //add the topleftpoint
            if (TopLeftPoint != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_RECTANGLE_TOP_LEFT_POINT] = TopLeftPoint;
            }     
            //add the width
            if (RegionWidth != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_RECTANGLE_WIDTH] = RegionWidth;
            }            
            //add the height
            if (RegionHeight != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_RECTANGLE_HEIGHT] = RegionHeight;
            }
            return message;
        }
    }
}
