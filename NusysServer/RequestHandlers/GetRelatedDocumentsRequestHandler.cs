using System.Diagnostics;
using NusysIntermediate;

namespace NusysServer.RequestHandlers
{
    public class GetRelatedDocumentsRequestHandler: RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.GetRelatedDocumentsRequest);
            var message = GetRequestMessage(request);
            Debug.Assert(message.ContainsKey(NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_LIBRARY_ID_KEY));
            var libraryId = message.Get(NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_LIBRARY_ID_KEY);
            //TODO DO stuff with related documents and return the list of related documents
            
            var returnMessage = new Message(message);
            //returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            //returnMessage[NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_RETURNED_RELATED_DOCUMENT_LIBRARY_IDS_KEY] = success;
            return returnMessage;

        }
    }
}