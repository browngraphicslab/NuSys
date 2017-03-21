using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    public class UploadFileRequestHandler : FullArgsRequestHandler<UploadFileRequestArgs, UploadFileReturnArgs>
    {
        protected override UploadFileReturnArgs HandleArgsRequest(UploadFileRequestArgs args, NuWebSocketHandler senderHandler)
        {
            var retArgs = new UploadFileReturnArgs();
            retArgs.WasSuccessful = FileHelper.CreateFile(args.Bytes, args.Id); ;
            return retArgs;
        }
    }
}