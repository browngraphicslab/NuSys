using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// args class used in the return on a GetEntireWorkspaceRequest.
    /// Each property is a piece on an etnire workspace that needs to be returned;
    /// </summary>
    public class GetEntireWorkspaceRequestReturnArgs
    {
        public IEnumerable<string> AliasStrings;
        public IEnumerable<string> ContentMessages;
        public IEnumerable<string> PresentationLinks;
    }
}
