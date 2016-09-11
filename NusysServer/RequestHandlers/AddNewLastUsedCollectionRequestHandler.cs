using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// the request handler used to add a new entry to the Last Used Collections sql table.
    /// </summary>
    public class AddNewLastUsedCollectionRequestHandler : RequestHandler
    {
        /// <summary>
        /// this override handle request method should simply add or update a row in the Last Used Collections sql table.
        /// Then it should delete the oldest entries until there are at most ten per user id.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.AddNewLastUsedCollectionRequest);
            var args = GetRequestArgs<AddNewLastUsedCollectionServerRequestArgs>(request);

            if (args == null)
            {
                throw new Exception("AddNewLastUsedCollectionServerRequestArgs were null");
            }
            var userId = args.UserId;
            var collectionId = args.CollectionLibraryId;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(collectionId))
            {
                throw new Exception("AddNewLastUsedCollectionServerRequestArgs properties were not set correctly: "+JsonConvert.SerializeObject(args));
            }

            var currentTime = DateTime.Now.ToLongTimeString();

            var commandString = "UPDATE " + Constants.GetTableName(Constants.SQLTableType.LastUsedCollections) + " SET " + NusysConstants.LAST_USED_COLLECTIONS_TABLE_LAST_USED_DATE + " = '" + currentTime + "' WHERE " + NusysConstants.LAST_USED_COLLECTIONS_TABLE_USER_ID + " = '" + userId + "' AND " + NusysConstants.LAST_USED_COLLECTIONS_TABLE_COLLECTION_LIBRARY_ID + " = '" + collectionId + "' " +
                                "IF @@ROWCOUNT = 0 INSERT INTO " + Constants.GetTableName(Constants.SQLTableType.LastUsedCollections) + " (" + NusysConstants.LAST_USED_COLLECTIONS_TABLE_LAST_USED_DATE + ", " + NusysConstants.LAST_USED_COLLECTIONS_TABLE_COLLECTION_LIBRARY_ID + ", " + NusysConstants.LAST_USED_COLLECTIONS_TABLE_USER_ID + ") VALUES ('" + currentTime + "', '" + collectionId + "', '" + userId+ "');";

            var cmd = ContentController.Instance.SqlConnector.MakeCommand(commandString);
            var success = cmd.ExecuteNonQuery() > 0;

            return new Message() { { NusysConstants.REQUEST_SUCCESS_BOOL_KEY,success}};
        }
    }
}