using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate 
{
    /// <summary>
    /// an abstract class that should be the base class of all the requests using args classes as the primary means of transporting data.
    /// This class requires an args class type to be built into the declarations.
    /// This type will be used when returning the args class.
    /// This class simply requires all sub classes to have a get args method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServerArgsRequest<T> : Request where T : ServerRequestArgsBase 
    {
        /// <summary>
        /// Default constructor for derserializing messages from the server.
        /// </summary>
        /// <param name="message"></param>
        public ServerArgsRequest(Message message) : base(message) { }

        /// <summary>
        /// the base constructor takes in an args class.  
        /// It will create a request based on the request type of the request args.
        /// This will then set the requests' arguments class key's value to the serialzed version of the request args.
        /// </summary>
        /// <param name="requestArgs"></param>
        /// <param name="requestType"></param>
        public ServerArgsRequest (T requestArgs) : base(requestArgs.RequestType)
        {
            requestArgs.CheckValidity();
            _message[NusysConstants.SERVER_ARGS_REQUEST_ARGS_CLASS_KEY] = requestArgs;
        }

        /// <summary>
        /// constructor is just for server use.
        /// Please dont use this unless you know what you're doing.
        /// </summary>
        /// <param name="request"></param>
        public ServerArgsRequest(Request request) : base(request.GetRequestType())
        {
            _message = request.GetMessage();
        }

        /// <summary>
        /// this method method will return a fully populated server args class.
        /// It should probably only be called server side and should really only be called by the abstracy GetArgs() method.
        /// It will parse and deserializ the args class from the request message.
        /// </summary>
        /// <returns></returns>
        public T GetArgsClassFromMessage(Type t = null)
        {
            if (t == null)
            {
                t = typeof(T);
            }
            return (T)(_message.ContainsKey(NusysConstants.SERVER_ARGS_REQUEST_ARGS_CLASS_KEY) ? _message.Get(NusysConstants.SERVER_ARGS_REQUEST_ARGS_CLASS_KEY,t) : null);
        }
    }
}
