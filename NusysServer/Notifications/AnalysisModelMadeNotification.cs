using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// the notification class to notify the client that their analysis model for an uploaded content has been made.
    /// </summary>
    public class AnalysisModelMadeNotification : Notification
    {
        /// <summary>
        /// the constructor just takes in the analysis model made notification arguments.
        /// Populate that class and then construct this notification.
        /// </summary>
        /// <param name="args"></param>
        public AnalysisModelMadeNotification(AnalysisModelMadeNotificationArgs args) : base(NusysConstants.NotificationType.AnalysisModelMade, args){}
    }
}