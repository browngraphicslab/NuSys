using System.Collections.Generic;
using System.Diagnostics;
using NusysIntermediate;

namespace NusysServer
{
    public class DeleteLibraryElementRequestHandler : RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.DeleteLibraryElementRequest);
            var message = GetRequestMessage(request);
            Debug.Assert(message.ContainsKey(NusysConstants.DELETE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY));

            //safetly create new message to pass into the delete library element method
            var deleteLibraryElementMessage = new Message();
            deleteLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY] = message[NusysConstants.DELETE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY];

            //delete library element and also related metadata from the metadata table
            var success = ContentController.Instance.SqlConnector.DeleteLibraryElement(deleteLibraryElementMessage);

            ForwardMessage(message, senderHandler);

            var returnMessage = new Message();
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            if (success)
            {
                returnMessage[NusysConstants.DELETE_LIBRARY_ELEMENT_REQUEST_RETURNED_DELETED_LIBRARY_IDS_KEY] =
                    new List<string>() {message.GetString(NusysConstants.DELETE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY)};
            }
            return returnMessage;
        }
    }
}