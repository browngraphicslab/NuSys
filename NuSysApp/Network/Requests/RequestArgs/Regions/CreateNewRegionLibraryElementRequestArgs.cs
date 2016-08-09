using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// this is the subclass 
    /// </summary>
    public class CreateNewRegionLibraryElementRequestArgs : CreateNewLibraryElementRequestArgs
    {
        /// <summary>
        /// REQUIRED
        /// The library element ID of the clipping parent for this region
        /// </summary>
        public string ClippingParentLibraryId { get; set; }

        /// <summary>
        /// just adds the region clipping parent to the message, otherwise the same as the base class. 
        /// </summary>
        /// <returns></returns>
        public override Message PackToRequestKeys()
        {
            var message = base.PackToRequestKeys();

            Debug.Assert(ClippingParentLibraryId != null);

            //add the clipping parent id
            if (ClippingParentLibraryId != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_CLIPPING_PARENT_ID] = ClippingParentLibraryId;
            }
            return message;
        }
    }
}
