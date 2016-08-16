
using NusysIntermediate;

namespace NusysServer.Notifications.NotificationArgs
{
    public interface INotificationArgumentable
    {
        /// <summary>
        /// This should return a message with all the data in it using the notification keys in the nusysconstants file
        /// </summary>
        /// <returns></returns>
        Message PackToNotificationKeys();
    }
}