using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using System.Diagnostics;

namespace NuSysApp
{
    class CreateNewLinkLibraryElementRequestArgs : CreateNewLibraryElementRequestArgs
    {
        public string LibraryElementModelId1 { set; get; }
        public string LibraryElementModelId2 { set; get; }


        public override Message PackToRequestKeys()
        {
            var message = base.PackToRequestKeys();

            Debug.Assert(LibraryElementModelId1 != null);
            Debug.Assert(LibraryElementModelId2 != null);


            if (LibraryElementModelId1 != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_1_KEY] = LibraryElementModelId1;
            }
            if (LibraryElementModelId2 != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_2_KEY] = LibraryElementModelId2;
            }
            return message;
        }
    }
}
