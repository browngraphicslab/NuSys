using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// Class used to make a server request to return a lock.
    /// Will only remove the lock if this client is the one who holds it.
    /// </summary>
    public class ReturnLockRequest : FullArgsRequest<ReturnLockRequestArgs, ReturnLockRequestReturnArgs>
    {
        /// <summary>
        /// Constructor takes in a fully-populated args class.
        /// After constructing this request, use the nusysNetworkSession to await its execution.
        /// </summary>
        /// <param name="args"></param>
        public ReturnLockRequest(ReturnLockRequestArgs args) : base(args){}

        /// <summary>
        /// shouldnt be called since the server doesn't forward these requests
        /// </summary>
        /// <param name="senderArgs"></param>
        /// <param name="returnArgs"></param>
        public override void ExecuteRequestFunction(ReturnLockRequestArgs senderArgs, ReturnLockRequestReturnArgs returnArgs)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Method to call after a successful reqest to get the final lock holder's id.
        /// Will return null ifwe succesfully returned the lock.
        /// </summary>
        /// <returns></returns>
        public string GetFinalLockHolderId()
        {
            CheckWasSuccessfull();
            return ReturnArgs.UserId;
        }
    }
}
