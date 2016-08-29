using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// the request args class for creating a new AnalysisModelMadeNotification.
    /// Should simply store the content Id of of the analysis model that has been made
    /// </summary>
    public class AnalysisModelMadeNotificationArgs : INotificationArgumentable
    {
        /// <summary>
        /// REQUIRED.
        /// This string id is the id of the analysis model that has been made that you are notifying the user about
        /// </summary>
        public string ContentId { get; set; }

        /// <summary>
        ///  this pack to notification keys method just adds the content id to the notification message
        /// </summary>
        /// <returns></returns>
        public Message PackToNotificationKeys()
        {
            var message = new Message();

            Debug.Assert(!string.IsNullOrEmpty(ContentId));

            message[NusysConstants.ANALYSIS_MODEL_MADE_NOTIFICATION_CONTENT_ID_KEY] = ContentId;

            return message;
        }
    }
}