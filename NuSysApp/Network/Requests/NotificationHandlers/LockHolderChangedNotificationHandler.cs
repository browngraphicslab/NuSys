using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// The handler class for when a lock is updated on the server
    /// </summary>
    public class LockHolderChangedNotificationHandler : NotificationHandler
    {
        /// <summary>
        /// This should simply tell the lock controller about the updated lock
        /// </summary>
        /// <param name="message"></param>
        public override void HandleNotification(Message message)
        {
            var lockId = message.GetString(NusysConstants.LOCK_HOLDER_CHANGED_NOTIFICATION_LOCKABLE_ID_KEY);
            var userId = message.GetString(NusysConstants.LOCK_HOLDER_CHANGED_NOTIFICATION_USER_ID_KEY);
            userId = string.IsNullOrEmpty(userId) ? null : userId;
            Debug.Assert(!string.IsNullOrEmpty(lockId));
            SessionController.Instance.NuSysNetworkSession.LockController.UpdateLock(lockId,userId);
        }
    }
}
