using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Request args class to hold data for unsubscribing to a lock's updates
    /// </summary>
    public class UnSubscribeToLockRequestArgs : ServerRequestArgsBase
    {
        /// <summary>
        /// REQUIRED: the list of string ids of the lockables you wish to unsubscribe from
        /// </summary>
        public  HashSet<string >LockableIds { get; set; } = null;

        /// <summary>
        /// parameterless constructor just sets the request type;
        /// </summary>
        public UnSubscribeToLockRequestArgs() : base(NusysConstants.RequestType.UnSubscribeToLockRequest){}

        /// <summary>
        /// this will just make sure the lock id is set
        /// </summary>
        /// <returns></returns>
        protected override bool CheckArgsAreComplete()
        {
            if (LockableIds == null)
            {
                return false;
            }
            foreach (var id in LockableIds)
            {
                if (string.IsNullOrEmpty(id))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
