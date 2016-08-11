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
            var libraryId = message.GetString(NusysConstants.ELEMENT_UPDATE_REQUEST_ELEMENT_ID_KEY);
            ForwardMessage(message, senderHandler);
            List<SQLUpdatePropertiesArgs> propertiesToAdd = new List<SQLUpdatePropertiesArgs>();
            List<SqlQueryEquals> elementNonPropertiesUpdates = new List<SqlQueryEquals>();
            //if the client asked to save the update
            if (message.GetBool(NusysConstants.ELEMENT_UPDATE_REQUEST_SAVE_TO_SERVER_BOOLEAN))
            {
                
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
                        property.LibraryOrAliasId = libraryId;
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
                SQLUpdateOrInsertPropertyQuery updateOrInsertPropertiesQuery =
                    new SQLUpdateOrInsertPropertyQuery(propertiesToAdd);
                if (!updateOrInsertPropertiesQuery.ExecuteCommand())
                {
                    throw new Exception("Could not update or insert the properties from the sql query" + updateOrInsertPropertiesQuery.CommandString);
                }

                //update alias table
                SQLUpdateRowQuery updateRowQueryQuery =
                    new SQLUpdateRowQuery(new SingleTable(Constants.SQLTableType.Alias), elementNonPropertiesUpdates,
                        new SqlQueryEquals(Constants.SQLTableType.Alias, NusysConstants.ALIAS_ID_KEY, libraryId));
                if (!updateRowQueryQuery.ExecuteCommand())
                {
                    throw new Exception("Could not update library element from the sql query" + updateRowQueryQuery.CommandString);
                }
            }
            return new Message();
        }
    }
}