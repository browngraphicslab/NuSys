using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// This handler should be overriden by all the request handlers moving forward after 1/7/17.
    /// In the overriden class you should specify exactly what incoming and outgoing args you want.
    /// These two args classes MUST match up exactly with the args for the associated FullArgsRequest in NuSysApp.
    /// One property to note is that the Return args class must have an empty constructor.
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="S"></typeparam>
    public abstract class FullArgsRequestHandler<T,S> : RequestHandler where S : ServerReturnArgsBase,new() where T : ServerRequestArgsBase
    {
        /// <summary>
        /// this is the handler method required by all sub-classes of this one.
        /// It should instantiate a new return args of the appropriate type and return it from this overriden method.
        /// You should either add ReturnArgs.WasSuccessful = true in the method or throw an error if not.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        protected abstract S HandleArgsRequest(T args, NuWebSocketHandler senderHandler);

        protected void ForwardRequest(T args, S returnArgs, NuWebSocketHandler handlerToIgnore = null)
        {
            var m = new Message();
            m[NusysConstants.REQUEST_TYPE_STRING_KEY] = args.RequestType.ToString();
            m[NusysConstants.FULL_ARGS_REQUEST_RETURN_ARGS_KEY] = returnArgs;
            m[NusysConstants.SERVER_ARGS_REQUEST_ARGS_CLASS_KEY] = args;
            ForwardMessage(m,handlerToIgnore);
        }

        /// <summary>
        /// This is the overriden original handleRequest method.
        /// It still returns a message class but that should be changed once all request handlers have been ported to the new 'FullArgsRequest' method.
        /// This will catch any exceptions thrown in the HandleArgsRequest method and appropriately return error-populated return arguments.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            var m = new Message();

            var requestMessage = GetRequestMessage(request);

            var t = Type.GetType(requestMessage.GetString(NusysConstants.FULL_ARGS_REQUEST_ARGS_INSTANCE_TYPE_KEY)+",NusysIntermediate", false, true) ?? typeof(T);
            //this will assert the type of request
            var args = GetRequestArgsInstance(request,t);
            S s;
            try
            {
                s = HandleArgsRequest(args, senderHandler);
                Debug.Assert(s.WasSuccessful = true," If this request wasn't successful, throw an error with a descriptive message.  ");
                s.CheckValidity();
            }
            catch (Exception e)
            {
                s = new S();
                s.WasSuccessful = false;
                s.ErrorMessage = e.Message;
                m[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = false;
                m[NusysConstants.REQUEST_ERROR_MESSAGE_KEY] = e.Message;
            }
            m[NusysConstants.FULL_ARGS_REQUEST_RETURN_ARGS_KEY] = s;
            return m;
        }

        /// <summary>
        /// This method will get the args instance of a very specific type
        /// </summary>
        /// <param name="request"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        protected T GetRequestArgsInstance(Request request, Type t)
        {
            var castRequest = new ServerArgsRequest<T>(request); //cast the request essentially
            if (castRequest == null)
            {
                throw new Exception(
                    "Request was of unexpected type.  Expected a ServerArgsRequest of argsClass type : " +
                    typeof(T).ToString());
            }
            return castRequest.GetArgsClassFromMessage(t);
        }

        /// <summary>
        /// Returns the type of T that this needs to parse.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Type GetArgsInstanceType(Request request)
        {
            var args = GetRequestArgs<T>(request);
            return args.GetInstanceType();
        }
    }
}