using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NusysIntermediate;
using NusysServer.Util.SQLQuery;

namespace NusysServer
{
    public class ElementUpdateRequestHandler : RequestHandler
    {
        /// <summary>
        /// will forward the request to all clients if it has an ID. 
        /// Then will save to the sql tables if the request asks to do so.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.ElementUpdateRequest);
            var message = GetRequestMessage(request);

            //make sure the element being updated has an ID
            if (!message.ContainsKey(NusysConstants.ELEMENT_UPDATE_REQUEST_ELEMENT_ID_KEY))
            {
                throw new Exception("An elementUpdateRequest must have an element ID to update");
            }
            var aliasId = message.GetString(NusysConstants.ELEMENT_UPDATE_REQUEST_ELEMENT_ID_KEY);
            ForwardMessage(message, senderHandler);
            List<SQLUpdatePropertiesArgs> propertiesToAdd = new List<SQLUpdatePropertiesArgs>();
            List<SqlQueryEquals> elementNonPropertiesUpdates = new List<SqlQueryEquals>();
            //if the client asked to save the update
            if (message.GetBool(NusysConstants.ELEMENT_UPDATE_REQUEST_SAVE_TO_SERVER_BOOLEAN))
            {
                //This updates the last edited timestamp of the collection that the element is in
                SQLSelectQuery parentCollectionIdQuery = new SQLSelectQuery(new SingleTable(Constants.SQLTableType.Alias, Constants.GetFullColumnTitle(Constants.SQLTableType.Alias, NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY)), new SqlQueryEquals(Constants.SQLTableType.Alias, NusysConstants.ALIAS_ID_KEY, aliasId));
                SqlQueryEquals lastEditedTimeStampUpdate = new SqlQueryEquals(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY, DateTime.UtcNow.ToString());
                SqlQueryEquals conditional = new SqlQueryEquals(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY, parentCollectionIdQuery);
                SQLUpdateRowQuery updateCollectionsLastEditedTime = new SQLUpdateRowQuery(new SingleTable(Constants.SQLTableType.LibraryElement), new List<SqlQueryEquals>() { lastEditedTimeStampUpdate }, conditional);
                var successUpdatingTimeStamp = updateCollectionsLastEditedTime.ExecuteCommand();
                Debug.Assert(successUpdatingTimeStamp == true);

                foreach (var kvp in message)
                {
                    //Check if key that needs to be updated is in the properties table or library element table
                    if (!NusysConstants.ALIAS_ACCEPTED_KEYS.Keys.Contains(kvp.Key))
                    {

                        if (NusysConstants.ILLEGAL_PROPERTIES_TABLE_KEY_NAMES.Contains(kvp.Key))
                        {
                            continue;
                        }
                        SQLUpdatePropertiesArgs property = new SQLUpdatePropertiesArgs();
                        property.PropertyKey = kvp.Key;
                        property.PropertyValue = kvp.Value.ToString();
                        property.LibraryOrAliasId = aliasId;
                        propertiesToAdd.Add(property);
                    }
                    else
                    {
                        SqlQueryEquals updateValue =
                            new SqlQueryEquals(Constants.SQLTableType.Alias, kvp.Key, kvp.Value.ToString());
                        elementNonPropertiesUpdates.Add(updateValue);
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
                //update alias table
                SQLUpdateRowQuery updateRowQueryQuery =
                    new SQLUpdateRowQuery(new SingleTable(Constants.SQLTableType.Alias), elementNonPropertiesUpdates,
                        new SqlQueryEquals(Constants.SQLTableType.Alias, NusysConstants.ALIAS_ID_KEY, aliasId));
                if (!updateRowQueryQuery.ExecuteCommand())
                {
                    throw new Exception("Could not update library element from the sql query" + updateRowQueryQuery.CommandString);
                }
            }
            var returnMessage = new Message();
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = true;

            return returnMessage;
        }
    }
}