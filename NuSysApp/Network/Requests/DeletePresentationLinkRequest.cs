using System.Diagnostics;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class DeletePresentationLinkRequest : Request
    {
        /// <summary>
        /// Used for when the server returns a new message
        /// </summary>
        /// <param name="message"></param>
        public DeletePresentationLinkRequest(Message message) : base(NusysConstants.RequestType.DeletePresentationLinkRequest, message)
        {
        }
        
        /// <summary>
        /// Used when createing a new request
        /// </summary>
        /// <param name="linkId"></param>
        public DeletePresentationLinkRequest(string linkId) : base(NusysConstants.RequestType.DeletePresentationLinkRequest)
        {
            _message[NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY] = linkId;
        }

        //just checks to see if the message contains an id to request
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));
        }
    }
}