using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysServer
{
    public abstract class RequestHandler
    {
        public abstract Message HandleRequest(Request request, NuWebSocketHandler senderHandler);

        protected Message GetRequestMessage(Request request)
        {
            var message = request.GetMessage();
            return message;
        }
    }
}
