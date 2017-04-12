using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// This request args class will help facilitate the ability to request a watch on a certain lock..
    /// These args should allow the user to also request the lock.
    /// </summary>
    public class SubscribeToLockRequestArgs : ServerRequestArgsBase
    {
        /// <summary>
        /// This lockable Id is required so the server knows what lockable you want to watch.
        /// REQUIRED
        /// </summary>
        public string LockableId { get; set; } = null;

        /// <summary>
        /// This nullable boolean represents whether you also want this subscription to act as a request to obtain the lock.
        /// REQUIRED, cant be null upon sending.
        /// </summary>
        public bool? RequestLock { get; set; } = null;

        /// <summary>
        /// parameterless construcor is required for simple new()'ing.
        /// </summary>
        public SubscribeToLockRequestArgs() : base(NusysConstants.RequestType.SubscribeToLockRequest){}

        /// <summary>
        /// This override should just make sure all the args have been properly set.
        /// </summary>
        /// <returns></returns>
        protected override bool CheckArgsAreComplete()
        {
            return LockableId != null && RequestLock != null;
        }
    }
}
