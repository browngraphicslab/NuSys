using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// Class used to add extension methods to ILockables.
    /// This is kinda like adding instance methods to the interface
    /// </summary>
    public static class ILockableMethodExtensions
    {
        /// <summary>
        /// This returns true if the lock is known about locally and this client currently holds the lock for the passed-in lockable id.
        /// </summary>
        /// <param name="lockable"></param>
        /// <returns></returns>
        public static bool HasLock(this ILockable lockable)
        {
            Debug.Assert(lockable?.Id != null);
            return SessionController.Instance.NuSysNetworkSession.LockController.GetUserIdOfLockHolder(lockable.Id) == WaitingRoomView.UserID;
        }

        /// <summary>
        /// This returns whether the lockable is already registered for events in the lockController
        /// </summary>
        /// <param name="lockable"></param>
        /// <returns></returns>
        public static bool IsRegistered(this ILockable lockable)
        {
            Debug.Assert(lockable?.Id != null);
            return SessionController.Instance.NuSysNetworkSession.LockController.IsRegistered(lockable);
        }

        /// <summary>
        /// Method that should asynchronously register this ILockable for getting events.  
        /// Can return null network user if nobody has the lock after this is called.
        /// If you want to register and request the lock for this, set requestLock to true.
        /// Read the lock-controller's Register method headers to learn more.
        /// </summary>
        /// <param name="lockable"></param>
        /// <param name="requestLock"></param>
        /// <returns></returns>
        public static async Task<NetworkUser> RegisterAsync(this ILockable lockable, bool requestLock = true)
        {
            return await SessionController.Instance.NuSysNetworkSession.LockController.RegisterAsync(lockable, requestLock);
        }

        /// <summary>
        /// Method that should register this ILockable for getting events.  
        /// Small synchronozation edge case that can be fixed by calling the async version.  
        /// If you want to register and request the lock for this, set requestLock to true.
        /// Read the lock-controller's Register method headers to learn more.
        /// </summary>
        /// <param name="lockable"></param>
        /// <param name="requestLock"></param>
        /// <returns></returns>
        public static bool Register(this ILockable lockable, bool requestLock = true)
        {
            return SessionController.Instance.NuSysNetworkSession.LockController.Register(lockable, requestLock);
        }
    }
}
