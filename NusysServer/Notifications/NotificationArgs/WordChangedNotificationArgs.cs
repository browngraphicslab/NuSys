using NusysIntermediate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// class used as the arguments for the notification used to tell all clients about an update made to a word document
    /// </summary>
    public class WordChangedNotificationArgs : INotificationArgumentable
    {
        /// <summary>
        /// The string for the contentDataModelId of the word document that was changed
        /// REQUIRED
        /// </summary>
        public string ContentDataModelId { get; set; }

        /// <summary>
        ///  this pack to notification keys method just adds the contentDataModelId to the request message
        /// </summary>
        /// <returns></returns>
        public Message PackToNotificationKeys()
        {
            var message = new Message();
            Debug.Assert(ContentDataModelId != null);

            //weird serialization things for security reasons
            message[NusysConstants.WORD_CHANGED_NOTIFICATION_CONTENT_DATA_MODEL_ID_KEY] = ContentDataModelId;

            return message;
        }
    }
}