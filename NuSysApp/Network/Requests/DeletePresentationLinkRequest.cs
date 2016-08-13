using System.Diagnostics;
using System.Threading.Tasks;
using NusysIntermediate;
using System;
using Newtonsoft.Json;

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
        /// Prefered constructor When creating new request to send to the server. To use, create new DeletePresentationLinkRequestArgs class and 
        /// pass in the corresponding data to it. To use this request, await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request) then call DeletePresentationLinkFromLibrary
        /// </summary>
        /// <param name="linkId"></param>
        public DeletePresentationLinkRequest(string linkId) : base(NusysConstants.RequestType.DeletePresentationLinkRequest)
        {
            _message[NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY] = linkId;
        }

        /// <summary>
        /// this method will parse and delete the returned presentation link after the request has successfully returned. 
        /// Will throw an exception if the request has not returned yet or has failed. 
        /// Returned whether the new presentation link was deleted
        /// </summary>
        /// <returns></returns>
        public bool DeletePresentationLinkFromLibrary()
        {
            if (WasSuccessful() != true)
            {
                //If this fails here, check with .WasSuccessful() before calling this method.
                throw new Exception("The request hasn't returned yet or was unsuccessful");
            }

            //make sure the key to delete is present
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));

            var presentationLinkID = _message.GetString(NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY);
            //TODO: DO SOMETHING WITH THIS ID

            return false;
        }

        /// <summary>
        /// This is called when the server sends a message to the client (except the client who initially created the request). It should delete the presentation link client side.
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            //make sure the key to delete is present
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));

            var presentationLinkID = _message.GetString(NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY);
            //TODO: DO SOMETHING WITH THIS ID
        }

        //just checks to see if the message contains an id to request
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));
        }
    }
}