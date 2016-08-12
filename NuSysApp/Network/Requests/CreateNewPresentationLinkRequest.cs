using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using NuSysApp.Network.Requests.RequestArgs;

namespace NuSysApp
{
    class CreateNewPresentationLinkRequest : Request
    {
        /// <summary>
        /// used when the server sends back a message
        /// </summary>
        /// <param name="message"></param>
        public CreateNewPresentationLinkRequest(Message message) : base(NusysConstants.RequestType.CreateNewPresentationLinkRequest, message)
        {
        }

        /// <summary>
        /// Used to create a new request
        /// </summary>
        /// <param name="requestArgs"></param>
        public CreateNewPresentationLinkRequest(CreateNewPresentationLinkRequestArgs requestArgs) : base(requestArgs,  NusysConstants.RequestType.CreateNewPresentationLinkRequest)
        {
        }

        //just checks to see if the message contains an id to request
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_IN_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_OUT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_PARENT_COLLECTION_ID_KEY));
        }
    }
}
