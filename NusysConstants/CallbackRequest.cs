using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public abstract class CallbackRequest<outT,returnT> : FullArgsRequest<outT,returnT> where returnT : ServerReturnArgsBase, new() where outT : ServerRequestArgsBase
    {
        /// <summary>
        /// the args class used to define the callback
        /// </summary>
        private CallbackArgs<CallbackRequest<outT, returnT>> _callbackArgs;

        /// <summary>
        /// This constructor takes in a fully populated arguments class and a callbackArgs object.
        /// After populating this arguments class, you can tell the nusys network session to asynchronously execute this request.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="callbackArgs"></param>
        public CallbackRequest(outT args, CallbackArgs<CallbackRequest<outT, returnT>> callbackArgs) : base(args)
        {
            Debug.Assert(callbackArgs?.SuccessFunction != null);
            _message[NusysConstants.FULL_ARGS_REQUEST_ARGS_INSTANCE_TYPE_KEY] = args.GetType().ToString();
            _callbackArgs = callbackArgs;
        }

        /// <summary>
        /// method to call to have this request execute its callback function
        /// </summary>
        /// <param name="requestWasSuccessfull"></param>
        /// <returns></returns>
        public bool ExecuteCallback(bool requestWasSuccessfull)
        {
            requestWasSuccessfull &= WasSuccessful() == true;
            Debug.Assert(_callbackArgs != null);
            bool? callbackSuccess = null;
            if (requestWasSuccessfull)
            {
                callbackSuccess = _callbackArgs.SuccessFunction?.Invoke(this);
            }
            else
            {
                callbackSuccess = _callbackArgs.FailureFunction?.Invoke(this);
            }
            return callbackSuccess != false;
        }
    }
}
