using NusysIntermediate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    class CreateNewPDFRegionLibraryElementRequestArgs : CreateNewRectangleRegionLibraryElementRequestArgs
    {
        public int PageLocation { get; set; }

        public override Message PackToRequestKeys()
        {
            var message = base.PackToRequestKeys();

            Debug.Assert(PageLocation != null);


            //add the page location
            if (PageLocation != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_PDF_PAGE_LOCATION] = PageLocation;
            }
            return message;
        }
    }
}
