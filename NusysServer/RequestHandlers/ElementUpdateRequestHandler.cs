using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    public class ElementUpdateRequestHandler : RequestHandler
    {
        /// <summary>
        /// will forward the request to all clients if it has an ID. 
        /// Then will save to the sql tables if the request asks to do so.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.ElementUpdateRequest);
            var message = GetRequestMessage(request);

            //make sure the element being updated has an ID
            if (!message.ContainsKey(NusysConstants.ELEMENT_UPDATE_REQUEST_ELEMENT_ID_KEY))
            {
                throw new Exception("An elementUpdateRequest must have an element ID to update");
            }

            ForwardMessage(message,senderHandler);

            //if the client asked to save the update
            if (message.GetBool(NusysConstants.ELEMENT_UPDATE_REQUEST_SAVE_TO_SERVER_BOOLEAN))
            {
                //todo actually save the update
                
            }

            return new Message();
        }
    }
}