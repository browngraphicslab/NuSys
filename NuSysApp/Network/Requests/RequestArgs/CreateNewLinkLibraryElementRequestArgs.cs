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
        /// <summary>
        /// This is the in library element model id of the link that is being created
        /// </summary>
        public string LibraryElementModelInId { set; get; }
        /// <summary>
        /// This is the out library element model id of the link that is being created
        /// </summary>
        public string LibraryElementModelOutId { set; get; }

        /// <summary>
        /// This packs all of the variables into the message that is send to the server
        /// </summary>
        /// <returns></returns>
        public override Message PackToRequestKeys()
        {
            var message = base.PackToRequestKeys();
            // checks if the values are there
            Debug.Assert(LibraryElementModelInId != null);
            Debug.Assert(LibraryElementModelOutId != null);
            
            Debug.Assert(LibraryElementModelInId != LibraryElementModelOutId, "The UI should never allow create link message to be sent for two instances of the same library element");

            // packs the in id
            if (LibraryElementModelInId != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_IN_KEY] = LibraryElementModelInId;

            }
            // packs the out id
            if (LibraryElementModelOutId != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_OUT_KEY] = LibraryElementModelOutId;
            }
            return message;
        }
    }
}

