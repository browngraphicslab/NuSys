using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class GetFileBytesRequestArgs : ServerRequestArgsBase
    {
        public GetFileBytesRequestArgs() : base( NusysConstants.RequestType.GetFileBytesRequest)
        {
        }

        public string ContentId { get; set; }

        protected override bool CheckArgsAreComplete()
        {
            return !string.IsNullOrEmpty(ContentId);
        }
    }
}
