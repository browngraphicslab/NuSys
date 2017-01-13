using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// Notification class used to tell the clients who is now the holder of a certain lock
    /// </summary>
    public class LockHolderChangedNotification : Notification
    {
        /// <summary>
        /// This constructor needs a fully-populated args class, and then can be sent to clients
        /// </summary>
        /// <param name="args"></param>
        public LockHolderChangedNotification(LockHolderChangedNotificationArgs args) : base(NusysConstants.NotificationType.LockHolderChanged, args)
        {
        }
    }
}