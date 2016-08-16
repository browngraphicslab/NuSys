using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NusysIntermediate;
using NusysServer.Util.SQLQuery;

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
            var libraryId = message.GetString(NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID);
            ForwardMessage(message, senderHandler);
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
            SQLUpdateOrInsertPropertyQuery updateOrInsertPropertiesQuery =
                new SQLUpdateOrInsertPropertyQuery(propertiesToAdd);
            if (!updateOrInsertPropertiesQuery.ExecuteCommand())
            {
                throw new Exception("Could not update or insert the properties from the sql query" + updateOrInsertPropertiesQuery.CommandString);
            }

            //update library element table
            SQLUpdateRowQuery updateRowQueryQuery =
                new SQLUpdateRowQuery(new SingleTable(Constants.SQLTableType.LibraryElement), libraryElementNonPropertiesUpdates,
                    new SqlQueryEquals(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY, libraryId));
            if (!updateRowQueryQuery.ExecuteCommand())
            {
                throw new Exception("Could not update library element from the sql query" + updateRowQueryQuery.CommandString);
            }
            var returnMessage = new Message();
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = true;

            return returnMessage;
        }
    }
}