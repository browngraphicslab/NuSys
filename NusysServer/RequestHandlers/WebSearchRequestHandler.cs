using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// Class used to handle the web search request
    /// </summary>
    public class WebSearchRequestHandler : FullArgsRequestHandler<WebSearchRequestArgs, ServerReturnArgsBase>
    {
        /// <summary>
        /// This method should not forward the request to anybody, but should rather just 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        protected override ServerReturnArgsBase HandleArgsRequest(WebSearchRequestArgs args, NuWebSocketHandler senderHandler)
        {
            var searchString = args.SearchString;
        }
    }
}