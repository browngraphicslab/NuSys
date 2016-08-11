using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// the request handler for chat message requests
    /// </summary>
    public class ChatRequestHandler : RequestHandler
    {
        /// <summary>
        /// this simply forwards the message on to others and returns that it was successful.  
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            var message = GetRequestMessage(request);

            #region EasterEgg

            if (message.GetString(NusysConstants.CHAT_REQUEST_CHAT_MESSAGE_KEY).ToLower().Contains("does luke suck?"))
            {
                message[NusysConstants.CHAT_REQUEST_CHAT_MESSAGE_KEY] = "Chatbot NEVER mentions that name... ";
            }
            #endregion EasterEgg

            ForwardMessage(message, senderHandler);
            return message;
        }
    }
}