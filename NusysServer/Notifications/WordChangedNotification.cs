using NusysIntermediate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// Notification class used to tell the client about an updated word document via its contentDataModelId;
    /// </summary>
    public class WordChangedNotification : Notification
    {
        /// the constructor just takes in a WordChangedNotificationArgs.
        /// First you should fill in the argument class with the id you want to drop, then call this constructor.
        /// </summary>
        /// <param name="args"></param>
        public WordChangedNotification(WordChangedNotificationArgs args) : base(NusysConstants.NotificationType.WordChanged, args){ }
    }
}