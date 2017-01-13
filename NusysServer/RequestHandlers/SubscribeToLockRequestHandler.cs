using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// This handler class should use the lock controllers to subscribe to a certain lock
    /// </summary>
    public class SubscribeToLockRequestHandler : FullArgsRequestHandler<SubscribeToLockRequestArgs, SubscribeToLockRequestReturnArgs>
    {
        /// <summary>
        /// This handleReques method should not forward the request on to anybody, the notification system should take care of all that.
        /// This will just tell the lock controllers who has what
        /// </summary>
        /// <param name="args"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        protected override SubscribeToLockRequestReturnArgs HandleArgsRequest(SubscribeToLockRequestArgs args, NuWebSocketHandler senderHandler)
        {
            var returnArgs = new SubscribeToLockRequestReturnArgs();
            if (args.RequestLock == true)
            {
                ContentController.Instance.LockController.AddLock(args.LockableId, senderHandler); // attempts to get the lock
            }
            ContentController.Instance.LockListeners.AddUserToListenToLock(args.LockableId, senderHandler);//listens to future updates to this lock
            var holder = ContentController.Instance.LockController.GetUserFromLock(args.LockableId);
            returnArgs.UserIdOfLockHolder = NusysClient.IDtoUsers[holder].UserID;
            returnArgs.WasSuccessful = true;
            return returnArgs;
        }
    }
}