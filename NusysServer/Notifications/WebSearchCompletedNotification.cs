using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// Notification for when a web search is completed.
    /// </summary>
    public class WebSearchCompletedNotification : Notification
    {
        /// <summary>
        /// notification class to tell the users when their web search has completed.
        /// </summary>
        /// <param name="args"></param>
        public WebSearchCompletedNotification(WebSearchCompletedNotificationArgs args) : base(NusysConstants.NotificationType.WebSearchCompleted, args){}
    }
}