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
    /// the request handler used to get the aliases of a certain library element.
    /// Should make the simple call to the sql table and return it.
    /// </summary>
    public class GetAliasesOfLibraryElementRequestHandler : RequestHandler
    {
        /// <summary>
        /// this override handler will get the library element id, make the sql table query, and return the list of element models ot the original sender.
        /// This will not forward the request onto any other clients
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.GetAliasesOfLibraryElementRequest);
            var message = GetRequestMessage(request);
            if (!message.ContainsKey(NusysConstants.GET_ALIASES_OF_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY))
            {
                throw new Exception( "The Get aliases of Library Element request must contain a library element id with which to query");
            }
            var libraryElementId = message.GetString(NusysConstants.GET_ALIASES_OF_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY);

            var query = new SQLSelectQuery(new SingleTable(Constants.SQLTableType.Alias),new SqlQueryEquals(Constants.SQLTableType.Alias, NusysConstants.ALIAS_LIBRARY_ID_KEY,libraryElementId));
            var queryResults = query.ExecuteCommand();
            var models = queryResults.Select(m => ElementModelFactory.CreateFromMessage(m));

            var returnMessage = new Message();
            returnMessage[NusysConstants.GET_ALIASES_OF_LIBRARY_ELEMENT_REQUEST_RETURNED_ELEMENTS_MODELS_KEY] = JsonConvert.SerializeObject(models);
            return returnMessage;
        }
    }
}