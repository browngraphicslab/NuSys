using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NusysIntermediate;
using NusysServer.Util.SQLQuery;
using NusysServer.Util;
using Newtonsoft.Json;

namespace NusysServer
{
    /// <summary>
    /// the request handler class for updating any and all LibraryElements.
    /// </summary>
    public class UpdateLibraryElementRequestHandler : RequestHandler
    {
        /// <summary>
        /// this handle request method will forward the message to other clients and save changes to the server.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.UpdateLibraryElementModelRequest);
            var message = GetRequestMessage(request);

            //make sure the element being updated has an ID
            if (!message.ContainsKey(NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID))
            {
                throw new Exception("An Updatelibrary element Request must have an library ID to update");
            }
            if (message.GetBool(NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_SAVE_TO_SERVER_BOOLEAN, true))
            {
                SaveToServer(message);
            }
            //if the access is being updated, let others know of the "new" library element.
            if (!string.IsNullOrEmpty(message.GetString(NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY)))
            {
                ForwardNewLibraryElementToOthers(message.GetString(NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID), senderHandler);
            }
            else
            {
                ForwardMessage(message, senderHandler);
            }
            var returnMessage = new Message();
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = true;

            return returnMessage;
        }

        /// <summary>
        /// This method should update all the necessary sql tables to update the library element.
        /// </summary>
        /// <param name="message"></param>
        private void SaveToServer(Message message)
        {
            var libraryId = message.GetString(NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID);
            List<SQLUpdatePropertiesArgs> propertiesToAdd = new List<SQLUpdatePropertiesArgs>();
            List<SqlQueryEquals> libraryElementNonPropertiesUpdates = new List<SqlQueryEquals>();
            //Set the last edited time stamp to now
            message[NusysConstants.LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY] = DateTime.UtcNow.ToString();
            foreach (var kvp in message)
            {
                //Check if key that needs to be updated is in the properties table or library element table
                if (!NusysConstants.LIBRARY_ELEMENT_MODEL_ACCEPTED_KEYS.Keys.Contains(kvp.Key))
                {

                    if (NusysConstants.ILLEGAL_PROPERTIES_TABLE_KEY_NAMES.Contains(kvp.Key))
                    {
                        continue;
                    }
                    SQLUpdatePropertiesArgs property = new SQLUpdatePropertiesArgs();
                    property.PropertyKey = kvp.Key;
                    property.PropertyValue = kvp.Value.ToString();
                    property.LibraryOrAliasId = libraryId;
                    propertiesToAdd.Add(property);
                }
                else
                {
                    SqlQueryEquals updateValue =
                        new SqlQueryEquals(Constants.SQLTableType.LibraryElement, kvp.Key, kvp.Value.ToString());
                    libraryElementNonPropertiesUpdates.Add(updateValue);
                }
            }
            //Update or insert properties into table
            if (propertiesToAdd.Any())
            {
                SQLUpdateOrInsertPropertyQuery updateOrInsertPropertiesQuery =
                    new SQLUpdateOrInsertPropertyQuery(propertiesToAdd);
                if (!updateOrInsertPropertiesQuery.ExecuteCommand())
                {
                    throw new Exception("Could not update or insert the properties from the sql query" + updateOrInsertPropertiesQuery.CommandString);
                }
            }

            //update library element table
            SQLUpdateRowQuery updateRowQueryQuery =
                new SQLUpdateRowQuery(new SingleTable(Constants.SQLTableType.LibraryElement), libraryElementNonPropertiesUpdates,
                    new SqlQueryEquals(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY, libraryId));
            if (!updateRowQueryQuery.ExecuteCommand())
            {
                throw new Exception("Could not update library element from the sql query" + updateRowQueryQuery.CommandString);
            }
        }

        /// <summary>
        /// This method will fetch all the necessary data from the library element, metadata, and properties sql tables, create a new json serialized
        /// library element and forward it to other clients with the new library element request type. This should only be called when the users update the library element
        /// changing it from private to public. 
        /// </summary>
        private void ForwardNewLibraryElementToOthers(string libraryId, NuWebSocketHandler senderHandler)
        {
            SqlJoinOperationArgs libraryElementJoinProperties = new SqlJoinOperationArgs();
            libraryElementJoinProperties.LeftTable = new SingleTable(Constants.SQLTableType.LibraryElement);
            libraryElementJoinProperties.RightTable = new SingleTable(Constants.SQLTableType.Properties);
            libraryElementJoinProperties.JoinOperator = Constants.JoinedType.LeftJoin;
            libraryElementJoinProperties.Column1 = Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement,
                NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY).First();
            libraryElementJoinProperties.Column2 = Constants.GetFullColumnTitle(Constants.SQLTableType.Properties,
                NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY).First();
            JoinedTable propertiesJoinLibraryElement = new JoinedTable(libraryElementJoinProperties);

            SqlJoinOperationArgs libraryElementJoinPropertiesJoinMetadata = new SqlJoinOperationArgs();
            libraryElementJoinPropertiesJoinMetadata.LeftTable = propertiesJoinLibraryElement;
            libraryElementJoinPropertiesJoinMetadata.RightTable = new SingleTable(Constants.SQLTableType.Metadata);
            libraryElementJoinPropertiesJoinMetadata.JoinOperator = Constants.JoinedType.LeftJoin;
            libraryElementJoinPropertiesJoinMetadata.Column1 = Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement,
                NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY).First();
            libraryElementJoinPropertiesJoinMetadata.Column2 = Constants.GetFullColumnTitle(Constants.SQLTableType.Metadata,
                NusysConstants.METADATA_LIBRARY_ELEMENT_ID_COLUMN_KEY).First();
            JoinedTable propertiesJoinLibraryElementJoinMetadata = new JoinedTable(libraryElementJoinPropertiesJoinMetadata);

            var conditionalForLibraryElementId = new SqlQueryEquals(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY, libraryId);
            var query = new SQLSelectQuery(propertiesJoinLibraryElementJoinMetadata, conditionalForLibraryElementId);
            var libraryElementReturnedMessages = query.ExecuteCommand();
            PropertiesAndMetadataParser propertiesAndMetadataParser = new PropertiesAndMetadataParser();
            var libraryElementConcatPropertiesMessages = propertiesAndMetadataParser.ConcatPropertiesAndMetadata(new List<Message>(libraryElementReturnedMessages)).First();

            var libraryElementModel = LibraryElementModelFactory.CreateFromMessage(Constants.StripTableNames(libraryElementConcatPropertiesMessages));
            var modelJson = JsonConvert.SerializeObject(libraryElementModel);
            Message forwardMessage = new Message();
            forwardMessage[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY] = modelJson;
            Request createNewLibraryElementRequest = new Request(NusysConstants.RequestType.CreateNewLibraryElementRequest, forwardMessage);
            forwardMessage = createNewLibraryElementRequest.GetFinalMessage();
            ForwardMessage(forwardMessage, senderHandler);
        }
    }
}