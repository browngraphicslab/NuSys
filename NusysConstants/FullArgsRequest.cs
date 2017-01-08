﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Abstract class that should be used for all requests moving forward as of 1/7/17.
    /// This requires you specify the outgoing and returned arguments classes for this request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="S"></typeparam>
    public abstract class FullArgsRequest<T,S> : ServerArgsRequest<T> where S : ServerReturnArgsBase, new() where T: ServerRequestArgsBase
    {
        /// <summary>
        /// Private returnArgs variable which can be returned in the public version after this has been set;
        /// </summary>
        private S _returnArgs;

        /// <summary>
        /// private boolean representing if the return args have already been attemped to be parsed.  
        /// This is primarily to make sure we don't json-deserialize multiple times.
        /// </summary>
        private bool _returnArgsParsed = false;

        /// <summary>
        /// The JSON-parsed return args class to be used after a successful request.
        /// This will parse the return args if it hasn't already been parsed, but will be constant time after the first parse.
        /// This will throw an error if the request wasn't successfull
        /// Can be null;
        /// </summary>
        public S ReturnArgs
        {
            get
            {
                if (_returnArgs == null && !_returnArgsParsed)
                {
                    CheckWasSuccessfull();
                    _returnArgs = _returnMessage.Get<S>("return_args");
                    _returnArgsParsed = true;
                }
                return _returnArgs;
            }
        }

        /// <summary>
        /// This constructor just takes in a fully populated arguments class.
        /// After populating this arguments class, you can tell the nusys network session to asynchronously execute this request.
        /// </summary>
        /// <param name="args"></param>
        public FullArgsRequest(T args) :base(args){}

        /// <summary>
        /// This override will look at the returned args and return true if they were successful otherwise it will default to the base class.
        /// </summary>
        /// <returns></returns>
        public override bool? WasSuccessful()
        {
            if (_returnArgsParsed && _returnArgs != null)
            {
                return _returnArgs.WasSuccessful;
            }
            return base.WasSuccessful();
        }
    }
}