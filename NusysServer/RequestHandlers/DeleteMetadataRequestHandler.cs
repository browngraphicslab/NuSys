using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;
using System.Diagnostics;

namespace NusysServer
{
    public class DeleteMetadataRequestHandler : RequestHandler
    {
        /// <summary>
        /// Deletes the metadata entry from the SQL table and forwards the request to all other clients.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.DeleteMetadataRequest);
            var message = GetRequestMessage(request);
            Debug.Assert(message.ContainsKey(NusysConstants.DELETE_METADATA_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.DELETE_METADATA_REQUEST_METADATA_KEY));

            //create new message to pass into the delete metadata method
            var deleteMetadataEntryMessage = new Message();
            deleteMetadataEntryMessage[NusysConstants.DELETE_METADATA_REQUEST_LIBRARY_ID_KEY] = message[NusysConstants.DELETE_METADATA_REQUEST_LIBRARY_ID_KEY];
            deleteMetadataEntryMessage[NusysConstants.DELETE_METADATA_REQUEST_METADATA_KEY] = message[NusysConstants.DELETE_METADATA_REQUEST_METADATA_KEY];
           
            //delete the metadata entry from the appropriate library element
            var success = ContentController.Instance.SqlConnector.DeleteMetadataEntry(deleteMetadataEntryMessage);

            ForwardMessage(message, senderHandler);

            var returnMessage = new Message(message);
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            return returnMessage;
        }
    }
}