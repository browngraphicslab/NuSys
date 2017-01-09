using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Request args used for the request to fetch the Word Document bytes from the server for a specified word document
    /// </summary>
    public class GetWordDocumentRequestArgs : ServerRequestArgsBase
    {
        /// <summary>
        /// Parameterless constructor jsut sets the request type
        /// </summary>
        public GetWordDocumentRequestArgs() : base(NusysConstants.RequestType.GetWordDocumentRequest) {}

        /// <summary>
        /// REQUIRED: The content Id of the word document we are trying to fetch
        /// </summary>
        public string ContentId = null;

        /// <summary>
        /// This simply check if the content Id has been set
        /// </summary>
        /// <returns></returns>
        protected override bool CheckArgsAreComplete()
        {
            return ContentId != null;
        }
    }
}
