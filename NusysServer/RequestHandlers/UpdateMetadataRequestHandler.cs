using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;
using System.Diagnostics;
using NusysServer.Util.SQLQuery;

namespace NusysServer
{
    public class UpdateMetadataRequestHandler : RequestHandler
    {
        /// <summary>
        /// Updates the metadata entry from the SQL table and forwards the request to all other clients.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.UpdateMetadataEntryRequest);
            var message = GetRequestMessage(request);
            Debug.Assert(message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_VALUE));

            //create new message to pass into the update metadata method
            var updateMetadataEntryMessage = new Message();
            updateMetadataEntryMessage[NusysConstants.UPDATE_METADATA_REQUEST_LIBRARY_ID_KEY] = message[NusysConstants.UPDATE_METADATA_REQUEST_LIBRARY_ID_KEY];
            updateMetadataEntryMessage[NusysConstants.UPDATE_METADATA_REQUEST_METADATA_KEY] = message[NusysConstants.UPDATE_METADATA_REQUEST_METADATA_KEY];
            updateMetadataEntryMessage[NusysConstants.UPDATE_METADATA_REQUEST_METADATA_VALUE] = message[NusysConstants.UPDATE_METADATA_REQUEST_METADATA_VALUE];

            //Update the metadata entry from the appropriate library element
            var success = UpdateMetadataSQLTable(updateMetadataEntryMessage);
            //Update the last edited time stamp of the library element 
            UpdateLibraryElementLastEditedTimeStamp(message.GetString(NusysConstants.UPDATE_METADATA_REQUEST_LIBRARY_ID_KEY));
            ForwardMessage(message, senderHandler);

            var returnMessage = new Message(message);
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            return returnMessage;
        }

        /// <summary>
        /// This updates the metadata sql table using the message that was passed in. It will return whehter or not the update was successfull
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool UpdateMetadataSQLTable(Message message)
        {
            if (!message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_LIBRARY_ID_KEY) || !message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_KEY))
            {
                return false;
            }
            var toUpdate = new List<SqlQueryEquals>
            {
                new SqlQueryEquals(Constants.SQLTableType.Metadata, NusysConstants.METADATA_VALUE_COLUMN_KEY,
                    message.GetString(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_VALUE))
            };

            var key = message.GetString(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_KEY);
            var libraryId = message.GetString(NusysConstants.UPDATE_METADATA_REQUEST_LIBRARY_ID_KEY);

            var libraryIdConditional = new SqlQueryEquals(Constants.SQLTableType.Metadata, NusysConstants.METADATA_LIBRARY_ELEMENT_ID_COLUMN_KEY, libraryId);
            var keyConditional = new SqlQueryEquals(Constants.SQLTableType.Metadata, NusysConstants.METADATA_KEY_COLUMN_KEY, key);

            var keyAndIdConditional = new SqlQueryOperator(keyConditional, libraryIdConditional, Constants.Operator.And);
            var cmd = new SQLUpdateRowQuery(new SingleTable(Constants.SQLTableType.Metadata), toUpdate, keyAndIdConditional);
            return cmd.ExecuteCommand();
        }
    }
}