using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Return args class after requesting a watch on a lock.
    /// </summary>
    public class SubscribeToLockRequestReturnArgs : ServerReturnArgsBase
    {
        /// <summary>
        /// This is the id of the user who currently holds the lock
        /// </summary>
        public string UserIdOfLockHolder { get; set; } = null;

        /// <summary>
        /// Should just make sure you've set the user id of the lock holder
        /// </summary>
        /// <returns></returns>
        protected override bool CheckIsValid()
        {
            return base.CheckIsValid();
        }
    }
}
