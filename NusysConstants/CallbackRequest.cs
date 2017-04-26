using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{

    /// Example usage (if this pull request is pushed to server after 3/15/17):
    /// 
    /// 
    /// 
    ///        public void Test()
    ///        {
    ///            var args = new UploadFileRequestArgs();
    ///            args.Bytes = new byte[3];
    ///            args.Id = "test";
    ///
    ///            var request = new UploadFileRequest(args, SuccessFunction);
    ///            request.Execute();
    ///        }
    ///
    //        private bool SuccessFunction(CallbackRequest<UploadFileRequestArgs, UploadFileReturnArgs> callbackRequest)
    ///        {
    ///            return true;
    ///        }


    ///<summary>
    /// Class used to make requests that call a callback instead of being 'awaitable'.  
    /// </summary>
    /// <typeparam name="outT"></typeparam>
    /// <typeparam name="returnT"></typeparam>
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
        /// constructor to use if you just want to pass in a success function
        /// </summary>
        /// <param name="args"></param>
        /// <param name="success"></param>
        public CallbackRequest(outT args, Func<CallbackRequest<outT, returnT>, bool> success) : this(args, new CallbackArgs<CallbackRequest<outT, returnT>>() {SuccessFunction = success }) { }

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

        /// <summary>
        /// This override will call the callback function
        /// </summary>
        /// <param name="success"></param>
        public override void SetReturnedFromServer(bool success)
        {
            var callbackSuccess = ExecuteCallback(success);
            Debug.Assert(callbackSuccess);
            base.SetReturnedFromServer(success);
        }
    }
}
