using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Simple return args class with the current owner of the lock
    /// </summary>
    public class GetLockRequestReturnArgs : ServerReturnArgsBase
    {
        /// <summary>
        /// The user id of the person who holds the requested lock
        /// </summary>
        public string CurrentLockHolder { get; set; } = null;
    }
}
