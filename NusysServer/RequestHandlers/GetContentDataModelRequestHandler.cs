using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    public class GetContentDataModelRequestHandler : RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.GetContentDataModelRequest);
            var message = GetRequestMessage(request);
            if (!message.ContainsKey(NusysConstants.GET_CONTENT_DATA_MODEL_REQUEST_CONTENT_ID_KEY))
            {
                throw new Exception("GetContentDataModelRequest must have an id for the content to get.  "+ NusysConstants.GET_CONTENT_DATA_MODEL_REQUEST_CONTENT_ID_KEY);
            }
            var contentId = message.GetString(NusysConstants.GET_CONTENT_DATA_MODEL_REQUEST_CONTENT_ID_KEY);
            var model = ContentController.Instance.SqlConnector.GetContentDataModel(contentId);
            var returnMessage = new Message();
            returnMessage[NusysConstants.GET_CONTENT_DATA_MODEL_REQUEST_RETURNED_CONTENT_DATA_MODEL_KEY] = JsonConvert.SerializeObject(model);
            return returnMessage;
        }
    }
}