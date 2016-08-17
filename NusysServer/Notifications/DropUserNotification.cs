using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// nitification used to notify all users when anpther client drops 
    /// </summary>
    public class DropUserNotification : Notification
    {
        /// <summary>
        /// the constructor just takes in a RemoveUserNotificationArgs.
        /// First you should fill in the argument class with the id you want to drop, then call this constructor.
        /// </summary>
        /// <param name="args"></param>
        public DropUserNotification(RemoveUserNotificationArgs args) : base(NusysConstants.NotificationType.RemoveUser, args){}
    }
}