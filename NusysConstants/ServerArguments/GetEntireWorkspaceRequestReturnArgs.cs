using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class GetEntireWorkspaceRequestReturnArgs
    {
        public IEnumerable<string> AliasStrings;
        public IEnumerable<string> ContentMessages;
        public IEnumerable<string> PresentationLinks;
    }
}
