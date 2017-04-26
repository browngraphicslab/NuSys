using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
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



    /// <summary>
    /// This is a sample callback request that actually could be used to upload files, but as oif 3/15/17 has essentially an empty server function.
    /// This class for now acts as a sample callback request.
    /// </summary>
    public class UploadFileRequest : CallbackRequest<UploadFileRequestArgs, UploadFileReturnArgs>
    {
        /// <summary>
        /// Usual constructor that takes in a full CallbackArgs class
        /// </summary>
        /// <param name="args"></param>
        /// <param name="callbackArgs"></param>
        public UploadFileRequest(UploadFileRequestArgs args, CallbackArgs<CallbackRequest<UploadFileRequestArgs, UploadFileReturnArgs>> callbackArgs) : base(args, callbackArgs) {}

        /// <summary>
        /// constructor that can be used to create this request with just a success function and the regular args class
        /// </summary>
        /// <param name="args"></param>
        /// <param name="successFunc"></param>
        public UploadFileRequest(UploadFileRequestArgs args, Func<CallbackRequest<UploadFileRequestArgs, UploadFileReturnArgs>,bool> successFunc) : base(args, successFunc) { }

        /// <summary>
        /// This should never be called because this request type isn't forwarded to others
        /// </summary>
        /// <param name="senderArgs"></param>
        /// <param name="returnArgs"></param>
        public override void ExecuteRequestFunction(UploadFileRequestArgs senderArgs, UploadFileReturnArgs returnArgs){}

    }
}
