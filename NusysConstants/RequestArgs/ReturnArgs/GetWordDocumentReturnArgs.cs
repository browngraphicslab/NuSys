using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// The return args class used for the GetWordDocumentRequest 
    /// </summary>
    public class GetWordDocumentReturnArgs : ServerReturnArgsBase
    {
        /// <summary>
        /// REQUIRED: The byte array of the requested word document.
        /// </summary>
        public byte[] WordBytes = null;

        /// <summary>
        /// This validity check just makes sure that the byte array has been set
        /// </summary>
        /// <returns></returns>
        protected override bool CheckIsValid()
        {
            return WordBytes != null;
        }
    }
}
