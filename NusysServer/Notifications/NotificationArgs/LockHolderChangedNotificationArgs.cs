using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// Notification args class used to create a LockHolderChanged notification
    /// </summary>
    public class LockHolderChangedNotificationArgs : INotificationArgumentable
    {
        /// <summary>
        /// The string id of the loackable that has changed its holder
        /// </summary>
        public string LockableId { get; set; }

        /// <summary>
        /// the string id of the user who now holds the lock
        /// </summary>
        public string HolderUserId { get; set; }

        /// <summary>
        /// THis should simply set the message's keys using the correct values
        /// </summary>
        /// <returns></returns>
        public Message PackToNotificationKeys()
        {
            var m = new Message();
            Debug.Assert(LockableId != null);
            Debug.Assert(HolderUserId != null);
            m[NusysConstants.LOCK_HOLDER_CHANGED_NOTIFICATION_LOCKABLE_ID_KEY] = LockableId;
            m[NusysConstants.LOCK_HOLDER_CHANGED_NOTIFICATION_USER_ID_KEY] = HolderUserId;
            return m;
        }
    }
}