using System.Diagnostics;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class UpdateContentRequest : Request
    {
        /// <summary>
        /// This constructor should only be used to create a new request from the message that was returned from the server.
        /// </summary>
        /// <param name="message"></param>
        public UpdateContentRequest(Message message) : base(NusysConstants.RequestType.UpdateContentRequest, message)
        {
        }

        /// <summary>
        /// Prefered constructor When creating new request to send to the server. To use, create new UpdateContentRequestArgs class and 
        /// pass in the corresponding data to it. To use this request, await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request) then call UpdateContentLocally
        /// </summary>
        /// <param name="requestArgs"></param>
        public UpdateContentRequest(UpdateContentRequestArgs requestArgs) : base(requestArgs, NusysConstants.RequestType.UpdateContentRequest)
        {
        }

        /// <summary>
        /// this method will parse and update the returned content after the request has successfully returned. 
        /// Will throw an exception if the request has not returned yet or has failed. 
        /// Returned whether the new content was updated or not
        /// </summary>
        public void UpdateContentLocally()
        {
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.UPDATE_CONTENT_REQUEST_UPDATED_CONTENT_KEY));
            var contentId = _returnMessage.GetString(NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_ID_KEY);
            var updatedContent = _returnMessage.GetString(NusysConstants.UPDATE_CONTENT_REQUEST_UPDATED_CONTENT_KEY);
            //TODO Update the content localy
        }

        /// <summary>
        /// This should be called when the server forwards the request to the client (except the client who initially created the request). It should update the content locally.
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_CONTENT_REQUEST_UPDATED_CONTENT_KEY));
            var contentId = _message.GetString(NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_ID_KEY);
            var updatedContent = _message.GetString(NusysConstants.UPDATE_CONTENT_REQUEST_UPDATED_CONTENT_KEY);
            //TODO Update the content localy

        }

        /// <summary>
        /// Makes sure that the request has the ID of the content to update, the type of content its updating, and the new content. Should only contain assert statements
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_TYPE_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_CONTENT_REQUEST_UPDATED_CONTENT_KEY));
        }
    }
}