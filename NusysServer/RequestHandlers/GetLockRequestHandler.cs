using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// Class to handle the getLockRequest.
    /// This should not forward the reuqest to other users
    /// </summary>
    public class GetLockRequestHandler : FullArgsRequestHandler<GetLockRequestArgs, GetLockRequestReturnArgs>
    {
        /// <summary>
        /// This handle override should not forawrd the reuqest to other users.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        protected override GetLockRequestReturnArgs HandleArgsRequest(GetLockRequestArgs args, NuWebSocketHandler senderHandler)
        {
            var returnArgs = new GetLockRequestReturnArgs();
            ContentController.Instance.LockController.AddLock(args.LockableId, senderHandler);
            returnArgs.WasSuccessful = true;
            returnArgs.CurrentLockHolder = ContentController.Instance.LockController.GetUserFromLock(args.LockableId).GetUserId();
            return returnArgs;
        }
    }
}