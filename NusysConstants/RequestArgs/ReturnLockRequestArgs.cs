using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NusysIntermediate
{
    /// <summary>
    /// Very basic request args class used to return a lock on an lockable on the server
    /// </summary>
    public class ReturnLockRequestArgs : ServerRequestArgsBase
    {
        /// <summary>
        /// REQUIRED: the Lockable Id of the item you are trying to return the lock for 
        /// </summary>
        public string LockableId { get; set; } = null;

        /// <summary>
        /// Parameterless constructor just sets the request type;
        /// </summary>
        public ReturnLockRequestArgs() : base(NusysConstants.RequestType.ReturnLockRequest) { }

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
