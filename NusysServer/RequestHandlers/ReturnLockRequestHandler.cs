using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// Request handler class for handling lock returns.
    /// Should rely on the contentcontroller's lock controllers.
    /// Shouldn't forward the request to anybody else.
    /// </summary>
    public class ReturnLockRequestHandler : FullArgsRequestHandler<ReturnLockRequestArgs, ReturnLockRequestReturnArgs>
    {
        /// <summary>
        /// This override should not forward this request to any other clients.
        /// Should remove the lock if the requester holds it.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        protected override ReturnLockRequestReturnArgs HandleArgsRequest(ReturnLockRequestArgs args, NuWebSocketHandler senderHandler)
        {
            var returnArgs = new ReturnLockRequestReturnArgs();
            if (ContentController.Instance.LockController.GetUserFromLock(args.LockableId) == senderHandler)
            {
                ContentController.Instance.LockController.RemoveLock(args.LockableId);
            }
            returnArgs.WasSuccessful = true;
            returnArgs.UserId = ContentController.Instance.LockController.GetUserFromLock(args.LockableId).GetUserId();
            return returnArgs;
        }
    }
}