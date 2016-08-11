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
    /// the request handler for all search requests
    /// </summary>
    public class SearchRequestHandler : RequestHandler
    {
        /// <summary>
        /// this request handler should only handle the search results and return the result to the original sender.  
        /// The request is not forwarded to any other clients. 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.SearchRequest);
            var message = GetRequestMessage(request);
            if (!message.ContainsKey(NusysConstants.SEARCH_REQUEST_SERIALIZED_QUERY_KEY))
            {
                throw new Exception("No search query was found in the search request.");
            }

            //get the query from the json
            var query = message.Get<Query>(NusysConstants.SEARCH_REQUEST_SERIALIZED_QUERY_KEY);

            //todo actually search and return a new search result

            var result = new SearchResult();

            var returnMessage = new Message();
            returnMessage[NusysConstants.SEARCH_REQUEST_RETURNED_SEARCH_RESULTS_KEY] =JsonConvert.SerializeObject(returnMessage);

            return returnMessage;
        }
    }
}