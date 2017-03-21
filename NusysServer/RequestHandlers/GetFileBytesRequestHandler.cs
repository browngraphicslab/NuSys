using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    public class GetFileBytesRequestHandler : FullArgsRequestHandler<GetFileBytesRequestArgs, GetFileBytesRequestReturnArgs>
    {
        protected override GetFileBytesRequestReturnArgs HandleArgsRequest(GetFileBytesRequestArgs args, NuWebSocketHandler senderHandler)
        {
            var returnArgs = new GetFileBytesRequestReturnArgs();
            returnArgs.Bytes = FileHelper.GetFileBytes(args.ContentId);
            returnArgs.WasSuccessful = returnArgs.Bytes != null;
            return returnArgs;
        }
    }
}