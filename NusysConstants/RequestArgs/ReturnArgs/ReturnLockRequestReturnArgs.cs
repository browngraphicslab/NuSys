using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// simple class that is returned after a ReturnLockRequest and holds the id of the user who held the lock last
    /// </summary>
    public class ReturnLockRequestReturnArgs : ServerReturnArgsBase
    {
        /// <summary>
        /// The string user id of the person who holds the lock after the request. 
        ///  Should only be non-null if something went wrong or the original used didn't hold the lock they tried to release.
        /// </summary>
        public string UserId { get; set; } = null;
    }
}
