using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using System.Diagnostics;

namespace NuSysApp
{
    class CreateNewMetadataRequest : Request
    {
        public CreateNewMetadataRequest(Message message) : base(NusysConstants.RequestType.CreateNewMetadataRequest, message)
        {

        }

        public CreateNewMetadataRequest(CreateNewMetadataRequestArgs requestArgs) : base(requestArgs, NusysConstants.RequestType.CreateNewMetadataRequest)
        {

        }

        /// <summary>
        /// simlply debug.asserts the important ID's.  
        /// Then adds a couple timestamps to the outgoing request message;
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_KEY_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_VALUE_KEY));
        }


    }
}
