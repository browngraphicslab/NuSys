using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// Request handler class for getting the byte array for a word document.
    /// </summary>
    public class GetWordDocumentRequestHandler : FullArgsRequestHandler<GetWordDocumentRequestArgs, GetWordDocumentReturnArgs>
    {
        /// <summary>
        /// This request handler shouldn't forward this request at all. 
        /// This should simply use the content ID of the word document to get the file bytes and return them.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        protected override GetWordDocumentReturnArgs HandleArgsRequest(GetWordDocumentRequestArgs args, NuWebSocketHandler senderHandler)
        {
            var returnArgs = new GetWordDocumentReturnArgs();
            returnArgs.WordBytes = FileHelper.GetWordBytesFromContentId(args.ContentId);
            returnArgs.WasSuccessful = true;
            return returnArgs;
        }
    }
}