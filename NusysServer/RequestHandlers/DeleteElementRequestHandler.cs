using System.Collections.Generic;
using System.Diagnostics;
using NusysIntermediate;

namespace NusysServer
{
    public class DeleteElementRequestHandler : RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.DeleteElementRequest);
            var message = GetRequestMessage(request);
            Debug.Assert(message.ContainsKey(NusysConstants.DELETE_ELEMENT_REQUEST_LIBRARY_ID_KEY));

            //safetly create new message to pass into the delete library element method
            var safedeleteAliasMessage = new Message();
            safedeleteAliasMessage[NusysConstants.ALIAS_ID_KEY] = message[NusysConstants.DELETE_ELEMENT_REQUEST_LIBRARY_ID_KEY];

            //delete library element
            var success = ContentController.Instance.SqlConnector.DeleteAlias(safedeleteAliasMessage);

            //notify everyone that a library element has been deleted
            var forwardMessage = new Message(message);
            forwardMessage.Remove(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);
            NuWebSocketHandler.BroadcastToSubset(forwardMessage, new HashSet<NuWebSocketHandler>() { senderHandler });

            var returnMessage = new Message();
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            return returnMessage;
        }
    }
}