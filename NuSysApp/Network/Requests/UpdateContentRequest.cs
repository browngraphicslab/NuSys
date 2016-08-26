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
        /// pass in the corresponding data to it. To use this request, await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request) then call UpdateContentLocally.
        ///  If you do not want to save the update to server, set savetoserver = false. By default its set to true
        /// </summary>
        /// <param name="requestArgs"></param>
        public UpdateContentRequest(UpdateContentRequestArgs requestArgs, bool saveUpdateToServer = true) : base(requestArgs, NusysConstants.RequestType.UpdateContentRequest)
        {
            _message[NusysConstants.UPDATE_CONTENT_REQUEST_SAVE_TO_SERVER_BOOLEAN] = saveUpdateToServer;
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

            //get the controller of the content data model that we want to update
            var contentDataController = SessionController.Instance.ContentController.GetContentDataController(contentId);

            if (contentDataController != null)
            {
                contentDataController.UpdateFromServer(updatedContent);
            }
        }

        /// <summary>
        /// This should be called when the server forwards the request to the client (except the client who initially created the request). 
        /// It should update the content locally.
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_CONTENT_REQUEST_UPDATED_CONTENT_KEY));
            var contentId = _message.GetString(NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_ID_KEY);
            var updatedContent = _message.GetString(NusysConstants.UPDATE_CONTENT_REQUEST_UPDATED_CONTENT_KEY);

            //get the controller of the content data model that we want to update
            var contentDataController = SessionController.Instance.ContentController.GetContentDataController(contentId);

            if (contentDataController != null)
            {
                contentDataController.UpdateFromServer(updatedContent);
            }
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