using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class UploadFileRequestArgs : ServerRequestArgsBase
    {
        public UploadFileRequestArgs() : base(NusysConstants.RequestType.UploadFileRequest) { }

        public byte[] Bytes { get; set; }
        public string Id { get; set; }

        protected override bool CheckArgsAreComplete()
        {
            return Bytes != null && !string.IsNullOrEmpty(Id);
        }
    }
}
