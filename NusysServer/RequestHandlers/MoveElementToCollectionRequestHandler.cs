using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;
using System.Diagnostics;

namespace NusysServer
{
    public class MoveElementToCollectionRequestHandler : RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.MoveElementToCollectionRequest);
            var message = GetRequestMessage(request);

            Debug.Assert(message.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_ELEMENT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_NEW_PARENT_COLLECTION_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_X_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_Y_KEY));

            //Create list of things to update and the sql query.
            List<SqlQueryEquals> columnsToUpdate = new List<SqlQueryEquals>();
            columnsToUpdate.Add(new SqlQueryEquals(Constants.SQLTableType.Alias, NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY, message.GetString(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_NEW_PARENT_COLLECTION_ID_KEY)));
            columnsToUpdate.Add(new SqlQueryEquals(Constants.SQLTableType.Alias, NusysConstants.ALIAS_LOCATION_X_KEY, message.GetDouble(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_X_KEY).ToString()));
            columnsToUpdate.Add(new SqlQueryEquals(Constants.SQLTableType.Alias, NusysConstants.ALIAS_LOCATION_Y_KEY, message.GetDouble(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_Y_KEY).ToString()));

            var conditional = new SqlQueryEquals(Constants.SQLTableType.Alias, NusysConstants.ALIAS_ID_KEY, message.GetString(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_ELEMENT_ID_KEY));
            SQLUpdateRowQuery updateRowQuery = new SQLUpdateRowQuery(new SingleTable(Constants.SQLTableType.Alias), columnsToUpdate, conditional);

            var success = updateRowQuery.ExecuteCommand();

            //If the command failed to execute, then do not forward the request to all other clients, 
            //simply return a failed message to the original sender.
            Message returnMessage = new Message(message);
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            if (!success)
            {
                return returnMessage;
            }
            ForwardMessage(message, senderHandler);
            return returnMessage;
        }
    }
}