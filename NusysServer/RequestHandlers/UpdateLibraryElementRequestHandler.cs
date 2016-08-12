using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// the request handler class for updating any and all LibraryElements.
    /// </summary>
    public class UpdateLibraryElementRequestHandler : RequestHandler
    {
        /// <summary>
        /// this handle request method will forward the message to other clients and save changes to the server.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            return new Message();//TODO NOT THIS
        }
    }
}