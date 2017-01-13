using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Very basic request args class used to request a lock on an id
    /// </summary>
    public class GetLockRequestArgs : ServerRequestArgsBase
    {
        /// <summary>
        /// REQUIRED: the Lockable Id of the item you are trying to fetch the lock for 
        /// </summary>
        public string LockableId { get; set; } = null;

        /// <summary>
        /// Parameterless constructor just sets the request type;
        /// </summary>
        public GetLockRequestArgs() : base(NusysConstants.RequestType.GetLockRequest){}

        /// <summary>
        /// simply checks to see if the id has been set
        /// </summary>
        /// <returns></returns>
        protected override bool CheckArgsAreComplete()
        {
            return LockableId != null;
        }
    }
}
