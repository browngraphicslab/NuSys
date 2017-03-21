using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class GetFileBytesRequest : CallbackRequest<GetFileBytesRequestArgs, GetFileBytesRequestReturnArgs>
    {
        public GetFileBytesRequest(GetFileBytesRequestArgs args,
            CallbackArgs<CallbackRequest<GetFileBytesRequestArgs, GetFileBytesRequestReturnArgs>> callbackArgs)
            : base(args, callbackArgs){}

        public byte[] GetRetunedBytes()
        {
            CheckWasSuccessfull();
            return ReturnArgs.Bytes;
        }
        public override void ExecuteRequestFunction(GetFileBytesRequestArgs senderArgs, GetFileBytesRequestReturnArgs returnArgs)
        {
            throw new NotImplementedException();
        }
    }
}
