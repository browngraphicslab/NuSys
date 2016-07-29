using System.Collections.Generic;
using System.Diagnostics;
using NusysIntermediate;

namespace NusysServer
{
    public class DeleteLibraryElementRequestHandler:RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.DeleteLibraryElementRequest);
            var message = GetRequestMessage(request);
            Debug.Assert(message.ContainsKey(NusysConstants.DELETE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY));
            var success = ContentController.Instance.SqlConnector.DeleteLibraryElement(message);

            var forwardMessage = new Message(message);
            forwardMessage.Remove(NusysConstants.DELETE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY);
            NuWebSocketHandler.BroadcastToSubset(forwardMessage, new HashSet<NuWebSocketHandler>() { senderHandler });

            var returnMessage = new Message();
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;

            return returnMessage;
        }
    }
}