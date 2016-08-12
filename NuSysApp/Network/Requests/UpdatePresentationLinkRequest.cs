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
    class UpdatePresentationLinkRequest : Request
    {
        /// <summary>
        /// Used when server sends back message
        /// </summary>
        /// <param name="message"></param>
        public UpdatePresentationLinkRequest(Message message) : base(NusysConstants.RequestType.UpdatePresentationLinkRequest, message)
        {
        }
        
        /// <summary>
        /// Used When creating new request
        /// </summary>
        /// <param name="requestArgs"></param>
        public UpdatePresentationLinkRequest(UpdatePresentationLinkRequestArgs requestArgs) : base(requestArgs, NusysConstants.RequestType.UpdatePresentationLinkRequest)
        {
        }

        /// <summary>
        /// Makes sure that the request has the id of the link that is about to be updated
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));
        }
    }
}
