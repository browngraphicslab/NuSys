using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            Task.Run(async delegate
            {
                try
                {
                    await RunParser(searchString);
                }
                catch (Exception e)
                {
                    senderHandler.SendError(e);
                    ErrorLog.AddError(e);
                }
            });
            return new ServerReturnArgsBase() {WasSuccessful =  true};
        }

        /// <summary>
        /// Method to acutally instanitate and run the parser with the given searchString
        /// </summary>
        /// <param name="searchString"></param>
        private async Task RunParser(string searchString)
        {
            var parsed = await HtmlImporter.RunWithSearch(searchString);
            parsed.RemoveAt(0);
            var docs = parsed.SelectMany(i => i);

        }
    }
}