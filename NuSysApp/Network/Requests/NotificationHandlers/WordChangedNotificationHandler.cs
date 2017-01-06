using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using System.Diagnostics;

namespace NuSysApp
{
    /// <summary>
    /// Notification handler class that is called whenever this client recieves a notification about a word document
    /// being updated by a user.
    /// This should simply fetch the new contentDataModel for the word document if it is needed locally, otherwise
    /// this specific client can essentailly ignore the update.
    /// </summary>
    public class WordChangedNotificationHandler : NotificationHandler
    {
        /// <summary>
        /// This handle notification will check to  see if the updated word document is needed locally.
        /// If it is, it will fetch the updated one.  If it isn't, it  will ignore this notification.
        /// </summary>
        /// <param name="message"></param>
        public override void HandleNotification(Message notificationMessage)
        {
            AssertType(notificationMessage, NusysConstants.NotificationType.WordChanged);
            Debug.Assert(notificationMessage.ContainsKey(NusysConstants.WORD_CHANGED_NOTIFICATION_CONTENT_DATA_MODEL_ID_KEY));
            var contentDataModelId = notificationMessage.GetString(NusysConstants.WORD_CHANGED_NOTIFICATION_CONTENT_DATA_MODEL_ID_KEY);
            if (!string.IsNullOrEmpty(contentDataModelId))
            {
                if (SessionController.Instance.ContentController.ContainsContentDataModel(contentDataModelId))
                {
                    SessionController.Instance.ContentController.ReFetchContentData(contentDataModelId);
                }
            }
        }
    }
}
