using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysAp
{
    /// <summary>
    /// Request class used to execute a web search from the server
    /// </summary>
    public class WebSearchRequest : FullArgsRequest<WebSearchRequestArgs, ServerReturnArgsBase>
    {
        /// <summary>
        /// Constructor takes in a fully populated WebSearchRequestArgs.
        /// Then use await NusysNetworkSession.ExecuteRequestAsync
        /// </summary>
        /// <param name="args"></param>
        public WebSearchRequest(WebSearchRequestArgs args) : base(args){}

        /// <summary>
        /// this won't need to be called since the server doesn't forward this requset to all users
        /// </summary>
        /// <param name="senderArgs"></param>
        /// <param name="returnArgs"></param>
        public override void ExecuteRequestFunction(WebSearchRequestArgs senderArgs, ServerReturnArgsBase returnArgs){}
    }
}
