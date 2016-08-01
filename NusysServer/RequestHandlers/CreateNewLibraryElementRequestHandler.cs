using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    public class CreateNewLibraryElementRequestHandler : RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.CreateNewLibrayElementRequest);

            var message = GetRequestMessage(request);

            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_TYPE_KEY));
            /*
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_CREATION_TIMESTAMP_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_FAVORITED_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_TITLE_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_SMALL_ICON_URL_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_LARGE_ICON_URL_KEY));
            */
            //todo send notification to everyone

            var success = ContentController.Instance.SqlConnector.AddLibraryElement(message);

            var forwardMessage = new Message(message);
            forwardMessage.Remove(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);
            NuWebSocketHandler.BroadcastToSubset(forwardMessage,new HashSet<NuWebSocketHandler>() {senderHandler});

            var returnMessage = new Message();
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;

            return returnMessage;
        }
    }
}