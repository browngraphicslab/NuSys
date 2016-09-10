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
    /// the request handler used to fetch the last used collections for a certain user.
    /// </summary>
    public class GetLastUsedCollectionsRequestHandler : RequestHandler
    {
        /// <summary>
        /// this override hande request method will make a sql call to fetch all the last used collections for a certain user.\
        /// Then it will populate a return message with the correct return arguments.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.GetLastUsedCollectionsRequest);
            var args = GetRequestArgs<GetLastUsedCollectionsServerRequestArgs>(request);
            if (args == null)
            {
                throw new Exception("the GetLastUsedCollectionsServerRequestArgs was null");
            }
            if (string.IsNullOrEmpty(args.UserId))
            {
                throw new Exception(" the GetLastUsedCollectionsServerRequestArgs user id was null or empty");
            }
            var cmd = new SQLSelectQuery(
                new SingleTable(Constants.SQLTableType.LastUsedCollections),
                new SqlQueryEquals(Constants.SQLTableType.LastUsedCollections,NusysConstants.LAST_USED_COLLECTIONS_TABLE_USER_ID,args.UserId));

            var results = cmd.ExecuteCommand();
            var returnModels = new List<LastUsedCollectionModel>();
            foreach (var result in results)
            {
                var model = new LastUsedCollectionModel();
                model.UnPackFromDatabaseMessage(result);
                returnModels.Add(model);
            }
            return new Message() { {NusysConstants.GET_LAST_USED_COLLECTIONS_REQUEST_RETURNED_MODELS_KEY , JsonConvert.SerializeObject(returnModels)} };
        }
    }
}