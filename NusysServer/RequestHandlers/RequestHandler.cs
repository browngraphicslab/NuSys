using System;
using System.CodeDom;
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
        /// <summary>
        /// the method that is called by the request router to actually handle an incoming request.  
        /// The senderHandler is the WebSocketHandler of the original sender.
        /// When returning from this request, you must add to the message that the request has failed, otherwise it will default to being successful.
        /// To add a failure indicator to the reutnred message, add to the message the key value pair: {NusysConstants.REQUEST_SUCCESS_BOOL_KEY, false}
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Message HandleRequest(Request request, NuWebSocketHandler senderHandler);

        /// <summary>
        /// method used to get the message from a request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected Message GetRequestMessage(Request request)
        {
            var message = request.GetMessage();
            return message; 
        }

        /// <summary>
        /// a protected method used to forward messages to everyone else from a requst handler.  
        /// Will automatically remove the id from the message that indicates an awaiting thread on the client side.
        /// WILL NOT MODIFY THE MESSAGE INSTANCE PASSED IN.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="senderHandlerToIgnore"></param>
        protected void ForwardMessage(Message messageToForward, NuWebSocketHandler senderHandlerToIgnore)
        {
            var forwardMessage = new Message(messageToForward);
            forwardMessage.Remove(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);
            NuWebSocketHandler.BroadcastToSubset(forwardMessage, new HashSet<NuWebSocketHandler>() { senderHandlerToIgnore });
        }

        /// <summary>
        /// This is a protected method that will get you the server args of a certain request.
        /// This method first casts the requeest to the type you specified, then gets the args from it.
        /// 
        /// It will throw an exception if the cast fails.
        /// 
        /// It can also return null if the args class was null or the key for the args class wans't found in the request message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        protected T GetRequestArgs<T>(Request request) where T : ServerRequestArgsBase
        {
            var castRequest = new ServerArgsRequest<T>(request); //cast the request essentially
            if (castRequest == null)
            {
                throw new Exception("Request was of unexpected type.  Expected a ServerArgsRequest of argsClass type : "+typeof(T).ToString());
            }
            return castRequest.GetArgsClassFromMessage();
        }
    }
}
