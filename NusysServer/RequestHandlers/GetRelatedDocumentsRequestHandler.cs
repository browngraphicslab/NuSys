using System;
using System.Diagnostics;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer.RequestHandlers
{
    public class GetRelatedDocumentsRequestHandler: RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.GetRelatedDocumentsRequest);
            var message = GetRequestMessage(request);
            Debug.Assert(message.ContainsKey(NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_CONTENT_ID_KEY));
            var contentId = message.Get(NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_CONTENT_ID_KEY);
            try
            {
                var tuples = ContentController.Instance.ComparisonController.GetComparison(contentId, 5); //get the five most related docs

                var returnMessage = new Message(message);
                returnMessage[NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_RETURNED_RELATED_DOCUMENT_CONTENT_IDS_KEY] = JsonConvert.SerializeObject(tuples);
                return returnMessage;
            }
            catch (Exception e)
            {
                senderHandler.SendError(new Exception("exception in doc comparison: "+e.Message));
                return new Message() { {NusysConstants.REQUEST_SUCCESS_BOOL_KEY, false}};
            }
        }
    }
}