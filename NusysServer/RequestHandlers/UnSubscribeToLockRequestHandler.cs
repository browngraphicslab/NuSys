using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// Handler for unsubscribing to a lock.  This should use the lock controller and listenener.
    /// This should automatically remvoe the lock if the user currently holds it.
    /// </summary>
    public class UnSubscribeToLockRequestHandler : FullArgsRequestHandler<UnSubscribeToLockRequestArgs, ServerReturnArgsBase>
    {
        /// <summary>
        /// This handleArgs Override should not forward the request to anybody 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        protected override ServerReturnArgsBase HandleArgsRequest(UnSubscribeToLockRequestArgs args, NuWebSocketHandler senderHandler)
        {
            var returnArgs = new ServerReturnArgsBase();
            foreach (var id in args.LockableIds)
            {
                if (ContentController.Instance.LockController.GetUserFromLock(id) == senderHandler)
                {
                    ContentController.Instance.LockController.RemoveLock(id);
                }
                ContentController.Instance.LockListeners.RemoveListeningLock(id, senderHandler);
            }
            returnArgs.WasSuccessful = true;
            return returnArgs;
        }
    }
}