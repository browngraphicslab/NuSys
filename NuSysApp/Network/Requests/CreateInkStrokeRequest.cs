using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class CreateInkStrokeRequest : Request
    {
        /// <summary>
        /// This constructor should only be used to create a new request from the message that was returned from the server.
        /// </summary>
        public CreateInkStrokeRequest(Message message) : base(NusysConstants.RequestType.CreateInkStrokeRequest, message)
        {
        }

        /// <summary>
        /// Prefered constructor When creating new request to send to the server. To use, create new CreateInkStrokeRequestArgs class and 
        /// pass in the corresponding data to it. To use this request, await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request).
        /// </summary>
        /// <param name="requestArgs"></param>
        public CreateInkStrokeRequest(CreateInkStrokeRequestArgs requestArgs) : base(requestArgs, NusysConstants.RequestType.CreateInkStrokeRequest)
        {
        }

        /// <summary>
        /// This should be called when the server forwards the request to the client (except the client who initially created the request). 
        /// It should add the ink locally
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            var inkModel = _message.Get<InkModel>(NusysConstants.CREATE_INK_STROKE_REQUEST_RETURNED_INK_MODEL_KEY);

            Debug.Assert(inkModel != null);
            if(inkModel == null)
            {
                return;
            }

            var contentController = SessionController.Instance.ContentController.GetContentDataController(inkModel.ContentId);

            Debug.Assert(contentController != null);

            contentController?.AddInk(inkModel);
        }

        /// <summary>
        /// Makes sure that the request has the ID of the content where the ink was drawn, the ink stroke id, and the list of points. Should only contain assert statements
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_INK_STROKE_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_INK_STROKE_REQUEST_POINTS_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_INK_STROKE_REQUEST_STROKE_ID_KEY));
        }
    }
}
