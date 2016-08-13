using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    public class SearchRequest : Request
    {
        /// <summary>
        /// preferred constructor.  Pass in a search query and then call execute.
        /// The returned values can be fetch after a successful request has fully executed.
        /// </summary>
        /// <param name="queryArgs"></param>
        public SearchRequest(QueryArgs queryArgs) : base(NusysConstants.RequestType.SearchRequest)
        {
            _message[NusysConstants.SEARCH_REQUEST_SERIALIZED_QUERY_KEY] = JsonConvert.SerializeObject(queryArgs);
        }

        /// <summary>
        /// Checks to make sure the query exists as a key in the _message;
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.SEARCH_REQUEST_SERIALIZED_QUERY_KEY));
        }

        /// <summary>
        /// the method to be called after the request has returned and was successfull.  
        /// This will return the server-returned SearchResults class for the correspondingQuery.  
        /// </summary>
        /// <returns></returns>
        public SearchResult GetReturnedResult()
        {
            CheckWasSuccessfull();
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.SEARCH_REQUEST_RETURNED_SEARCH_RESULTS_KEY));
            return _returnMessage.Get<SearchResult>(NusysConstants.SEARCH_REQUEST_RETURNED_SEARCH_RESULTS_KEY);
        }
    }
}
