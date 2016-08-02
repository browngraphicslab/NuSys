using System.Collections.Generic;
using System.Diagnostics;
using NusysIntermediate;

namespace NusysServer
{
    public class CreateNewElementRequestHandler: RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.NewElementRequest);
            var message = GetRequestMessage(request);
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_PARENT_COLLECTION_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_X_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_Y_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_SIZE_WIDTH_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_SIZE_HEIGHT_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_CREATOR_ID_KEY));


            var addAliasMessage = new Message();
            addAliasMessage[NusysConstants.ALIAS_ID_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_ID_KEY];
            addAliasMessage[NusysConstants.ALIAS_LIBRARY_ID_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY];
            addAliasMessage[NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_PARENT_COLLECTION_ID_KEY];
            addAliasMessage[NusysConstants.ALIAS_LOCATION_X_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_X_KEY];
            addAliasMessage[NusysConstants.ALIAS_LOCATION_Y_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_Y_KEY];
            addAliasMessage[NusysConstants.ALIAS_SIZE_HEIGHT_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_SIZE_HEIGHT_KEY];
            addAliasMessage[NusysConstants.ALIAS_SIZE_WIDTH_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_SIZE_WIDTH_KEY];
            addAliasMessage[NusysConstants.ALIAS_CREATOR_ID_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_CREATOR_ID_KEY];

            var success = ContentController.Instance.SqlConnector.AddAlias(addAliasMessage);

            var forwardMessage = new Message(message);
            forwardMessage.Remove(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);
            NuWebSocketHandler.BroadcastToSubset(forwardMessage, new HashSet<NuWebSocketHandler>() { senderHandler });

            var returnMessage = new Message();
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;

            return returnMessage;
        }
    }
}