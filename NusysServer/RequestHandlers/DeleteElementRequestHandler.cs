using System.Collections.Generic;
using System.Diagnostics;
using NusysIntermediate;
using System.Linq;

namespace NusysServer
{
    public class DeleteElementRequestHandler : RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.DeleteElementRequest);
            var message = GetRequestMessage(request);
            Debug.Assert(message.ContainsKey(NusysConstants.DELETE_ELEMENT_REQUEST_ELEMENT_ID));

            var parentCollectionId = GetParentCollectionIdOfAlias(message.GetString(NusysConstants.DELETE_ELEMENT_REQUEST_ELEMENT_ID));

            //safetly create new message to pass into the delete library element method
            var safedeleteAliasMessage = new Message();
            safedeleteAliasMessage[NusysConstants.ALIAS_ID_KEY] = message[NusysConstants.DELETE_ELEMENT_REQUEST_ELEMENT_ID];

            //delete library element
            var success = ContentController.Instance.SqlConnector.DeleteAlias(safedeleteAliasMessage);

            //if it failed, return that it failed
            if (!success)
            {
                return new Message() { { NusysConstants.REQUEST_SUCCESS_BOOL_KEY ,false} };
            }
            //Update the last edited time stamp of the collection
            UpdateLibraryElementLastEditedTimeStamp(parentCollectionId);

            //notify everyone that a library element has been deleted
            ForwardMessage(message, senderHandler);

            var returnMessage = new Message();
            if (success)
            {
                //add in the id so the original sender can delete locally.
                returnMessage[NusysConstants.DELETE_ELEMENT_REQUEST_RETURNED_DELETED_ELEMENT_ID] = message[NusysConstants.DELETE_ELEMENT_REQUEST_ELEMENT_ID];
            }
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            return returnMessage;
        }
    }
}