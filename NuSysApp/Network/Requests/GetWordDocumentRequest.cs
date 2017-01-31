using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// This class should be used to fetch the bytes of a word document from the server.  
    /// </summary>
    public class GetWordDocumentRequest : FullArgsRequest<GetWordDocumentRequestArgs, GetWordDocumentReturnArgs>
    {
        /// <summary>
        /// The constructor should take in a fully-populated args class.
        /// After constructing this, use the NusysNetwork session to execute this request asynchornously.
        /// </summary>
        /// <param name="args"></param>
        public GetWordDocumentRequest(GetWordDocumentRequestArgs args) : base(args){ }

        /// <summary>
        /// This should never be called because the server shouldn't forward this request to other clients.
        /// </summary>
        /// <param name="senderArgs"></param>
        /// <param name="returnArgs"></param>
        public override void ExecuteRequestFunction(GetWordDocumentRequestArgs senderArgs, GetWordDocumentReturnArgs returnArgs){ Debug.Assert(false, "this shouldn't happen");}

        /// <summary>
        /// Call this AFTER A SUCCESSFUL REQUEST EXECUTION to get the bytes you requested.
        /// </summary>
        /// <returns></returns>
        public byte[] GetReturnedDocumentBytes()
        {
            return ReturnArgs.WordBytes;
        }
    }
}
