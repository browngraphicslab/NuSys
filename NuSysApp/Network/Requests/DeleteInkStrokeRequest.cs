using System.Diagnostics;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class DeleteInkStrokeRequest : Request
    {
        /// <summary>
        /// This constructor should only be used to create a new request from the message that was returned from the server.
        /// </summary>
        public DeleteInkStrokeRequest(Message message) : base(NusysConstants.RequestType.DeleteInkStrokeRequest, message)
        {
        }

        /// <summary>
        /// Prefered constructor When creating new request to send to the server. To use, pass in the ink stroke id 
        /// of the stroke you want to remove. 
        /// Also pass in the content id of the stroke that you want to remove.
        /// To use this request, await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request). 
        /// </summary>
        public DeleteInkStrokeRequest(string inkStrokeId, string contentId) : base(NusysConstants.RequestType.DeleteInkStrokeRequest)
        {
            Debug.Assert(!string.IsNullOrEmpty(contentId));
            Debug.Assert(!string.IsNullOrEmpty(inkStrokeId));

            _message[NusysConstants.DELETE_INK_STROKE_REQUEST_STROKE_ID_KEY] = inkStrokeId;
            _message[NusysConstants.DELETE_INK_STROKE_REQUEST_CONTENT_ID_KEY] = contentId;
        }

        /// <summary>
        /// This should be called when the server forwards the request to the client (except the client who initially created the request). 
        /// It should delete the ink stroke locally
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_INK_STROKE_REQUEST_STROKE_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_INK_STROKE_REQUEST_CONTENT_ID_KEY));

            var contentId = _message.GetString(NusysConstants.DELETE_INK_STROKE_REQUEST_CONTENT_ID_KEY);
            var inkStrokeId = _message.GetString(NusysConstants.DELETE_INK_STROKE_REQUEST_STROKE_ID_KEY);

            Debug.Assert(!string.IsNullOrEmpty(contentId));
            Debug.Assert(!string.IsNullOrEmpty(inkStrokeId));

            var controller = SessionController.Instance.ContentController.GetContentDataController(contentId);
            Debug.Assert(controller != null);

            //controller?.RemoveInk(inkStrokeId); // TODO uncomment
        }

        /// <summary>
        /// Makes sure that the request has the ID of the stroke to delete
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_INK_STROKE_REQUEST_STROKE_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_INK_STROKE_REQUEST_CONTENT_ID_KEY));

        }
    }
}