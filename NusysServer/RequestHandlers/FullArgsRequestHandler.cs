﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            //this will assert the type of request
            var args = this.GetRequestArgs<T>(request);
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
            m["return_args"] = s;
            return m;
        }
    }
}