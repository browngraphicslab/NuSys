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
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.CreateNewLibraryElementRequest);

            var message = GetRequestMessage(request);
            if (!message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY))
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY] = NusysConstants.GenerateId();
            }
            //Debug.Assert(message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY));
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
            var addLibraryElementMessage = new Message();
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY] = message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY];
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY] = message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY]; 
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_TYPE_KEY] = message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY];
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_CREATION_TIMESTAMP_KEY] = message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATION_TIMESTAMP_KEY];
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_FAVORITED_KEY] = message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_FAVORITED_KEY];
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY] =  message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_KEYWORDS_KEY];
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_TITLE_KEY] = message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY];
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_SMALL_ICON_URL_KEY] = message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_SMALL_ICON_URL_KEY];
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY] = message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_MEDIUM_ICON_URL_KEY];
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_LARGE_ICON_URL_KEY] = message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LARGE_ICON_URL_KEY];
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY] = message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_ACCESS_KEY];



            var success = ContentController.Instance.SqlConnector.AddLibraryElement(addLibraryElementMessage);

            var forwardMessage = new Message(message);
            forwardMessage.Remove(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);// This step is a must since the client must recieve this message an not try to resume an awaiting thread
            NuWebSocketHandler.BroadcastToSubset(forwardMessage,new HashSet<NuWebSocketHandler>() {senderHandler});

            var returnMessage = new Message();
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;

            return returnMessage;
        }
    }
}