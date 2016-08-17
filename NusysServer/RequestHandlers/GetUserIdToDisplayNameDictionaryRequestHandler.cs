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
    /// the request handler used when the client requests the dictionary for User Id to display names. 
    /// Should sql query for the data, construct the dicitonary, and return it
    /// </summary>
    public class GetUserIdToDisplayNameDictionaryRequestHandler : RequestHandler
    {
        /// <summary>
        /// this request handler will simply fetch the data from the database, 
        /// construct the dictionary from the returned messages, and then return the dictionary to the sender of the request.
        /// This handler will not forward the request to anybody else.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.GetUserIdToDisplayNameDictionaryRequest);

            //create a query for all the user ids and display names
            var query = new SQLSelectQuery(new SingleTable(Constants.SQLTableType.Users,Constants.GetFullColumnTitles(Constants.SQLTableType.Users, new List<string>()
            {
                NusysConstants.USERS_TABLE_HASHED_USER_ID_KEY,
                NusysConstants.USERS_TABLE_USER_DISPLAY_NAME_KEY
            } )));

            //get the query messages
            var queryResponses = query.ExecuteCommand();

            //creates a dictionary of ID to Display name from the returned responses
            Dictionary<string, string> dict = queryResponses.Select(
                message => new KeyValuePair<string, string>(
                message.GetString(NusysConstants.USERS_TABLE_HASHED_USER_ID_KEY),
                message.GetString(NusysConstants.USERS_TABLE_USER_DISPLAY_NAME_KEY)
                )).ToDictionary(k => k.Key, v => v.Value);

            var returnMessage = new Message()
            {
                {NusysConstants.GET_USER_ID_TO_DISPLAY_NAME_DICTIONARY_REQUEST_RETURNED_DICTIONARY,JsonConvert.SerializeObject(dict)}
            };
            return returnMessage;
        }
    }
}