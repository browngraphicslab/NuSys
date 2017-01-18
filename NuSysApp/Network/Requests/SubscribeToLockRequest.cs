using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// Request class used to listen to a lock's changes on the server.
    /// This will give the option to also attempt to obtain the lock.
    /// Populate the args class and then use the nusysnetworksession to asynchronously execute this request
    /// </summary>
    public class SubscribeToLockRequest : FullArgsRequest<SubscribeToLockRequestArgs, SubscribeToLockRequestReturnArgs>
    {
        /// <summary>
        /// This constructor just takes in a fully-populated args class like all recent requests.
        /// After you create this request use the nusysnetworksession to asynchronously execute it.
        /// </summary>
        /// <param name="args"></param>
        public SubscribeToLockRequest(SubscribeToLockRequestArgs args) : base(args) {}

        /// <summary>
        /// This function should never be called since the server will not forward this request
        /// </summary>
        /// <param name="senderArgs"></param>
        /// <param name="returnArgs"></param>
        public override void ExecuteRequestFunction(SubscribeToLockRequestArgs senderArgs, SubscribeToLockRequestReturnArgs returnArgs){}

        /// <summary>
        /// Call this after a successfull request to see if this local user holds the lock
        /// </summary>
        /// <returns></returns>
        public bool UserHasLock()
        {
            CheckWasSuccessfull();
            return ReturnArgs.UserIdOfLockHolder == SessionController.Instance.LocalUserID;
        }

        /// <summary>
        /// After a successful request, this will return who currently holds the lock you requested watch of
        /// </summary>
        /// <returns></returns>
        public string LockHolderUserId()
        {
            CheckWasSuccessfull();
            return ReturnArgs.UserIdOfLockHolder;
        }
    }
}
