using NusysIntermediate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    class CreateNewTimeSpanRegionRequestArgs : CreateNewRegionLibraryElementRequestArgs
    {
        public double RegionStart { get; set; }
        public double RegionEnd { get; set; }

        public override Message PackToRequestKeys()
        {
            var message = base.PackToRequestKeys();
            //just checking if the values exist
            Debug.Assert(RegionStart != null);
            Debug.Assert(RegionEnd != null);

            //add the start
            if (RegionStart != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_TIMESPAN_START] = RegionStart;
            }
            //add the end
            if (RegionEnd != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_TIMESPAN_END] = RegionEnd;
            }

            return message;
        }
    }
}
