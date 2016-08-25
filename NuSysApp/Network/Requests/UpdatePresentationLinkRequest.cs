using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

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
        /// Prefered constructor When creating new request to send to the server. To use, create new UpdatePresentationLinkRequestArgs class and 
        /// pass in the corresponding data to it. To use this request, await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request) then call UpdatePresentationLinkInLibrary. If you do not wish
        /// to save the update to the server, set save to server = false. By default, it is set to true.
        /// </summary>
        /// <param name="requestArgs"></param>
        public UpdatePresentationLinkRequest(UpdatePresentationLinkRequestArgs requestArgs, bool saveToServer = true) : base(requestArgs, NusysConstants.RequestType.UpdatePresentationLinkRequest)
        {
            _message[NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_SAVE_TO_SERVER_BOOLEAN] = saveToServer;
        }

        /// <summary>
        /// this method will parse and update the returned presentation link after the request has successfully returned. 
        /// Will throw an exception if the request has not returned yet or has failed. 
        /// Returned whether the new presentation link was updated
        /// </summary>
        /// <returns></returns>
        public bool UpdatePresentationLinkFromLibrary()
        {
            if (WasSuccessful() != true)
            {
                //If this fails here, check with .WasSuccessful() before calling this method.
                throw new Exception("The request hasn't returned yet or was unsuccessful");
            }

            //make sure the key to delete is present
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));

            var presentationLinkID = _message.GetString(NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY);
            //TODO: DO SOMETHING WITH THIS ID and with all the other stuff in _message

            return false;
        }

        /// <summary>
        /// This is called when the server sends a message to the client (except the client who initially created the request). It should update the presentation link client side.
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            //make sure the key to delete is present
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));
            var presentationLinkID = _message.GetString(NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY);
            //TODO: DO SOMETHING WITH THIS ID
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
