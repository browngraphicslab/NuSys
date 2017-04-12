using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// Request used to ask for the lock on an item.
    /// After executing this, call RequestedLockGranted() to see if we do now have the lock
    /// </summary>
    public class GetLockRequest : FullArgsRequest<GetLockRequestArgs, GetLockRequestReturnArgs>
    {
        /// <summary>
        /// Request class used to ask for the lock permissions for a lockable
        /// </summary>
        /// <param name="args"></param>
        public GetLockRequest(GetLockRequestArgs args) : base(args){ }

        /// <summary>
        /// This will never be called since the server does not forward this request type
        /// </summary>
        /// <param name="senderArgs"></param>
        /// <param name="returnArgs"></param>
        public override void ExecuteRequestFunction(GetLockRequestArgs senderArgs, GetLockRequestReturnArgs returnArgs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// After a succesfull request, this will return whether we are the holder of the requested lock
        /// </summary>
        /// <returns></returns>
        public bool RequestedLockGranted()
        {
            CheckWasSuccessfull();
            return ReturnArgs.CurrentLockHolder == WaitingRoomView.UserID;
        }

        /// <summary>
        /// Method to call after this request has finished successfully to get the string userId of the current lock holder
        /// </summary>
        /// <returns></returns>
        public string CurrentLockHolder()
        {
            CheckWasSuccessfull();
            return ReturnArgs.CurrentLockHolder;
        }
    }
}
