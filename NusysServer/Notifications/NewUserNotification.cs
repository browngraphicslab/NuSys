using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// the class used to notify clients of other clients
    /// </summary>
    public class NewUserNotification : Notification
    {
        /// <summary>
        /// constructor takes in a NewUserNotificationArgs.
        /// Populate the args and check each property to see which are required for the creation of the notification
        /// </summary>
        /// <param name="args"></param>
        public NewUserNotification(NewUserNotificationArgs args) : base(NusysConstants.NotificationType.AddUser, args){}
    }
}