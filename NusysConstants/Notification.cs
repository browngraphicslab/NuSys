using System;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class Notification
    {
        protected Message _message;
        private NusysConstants.NotificationType _notificationType;

        /// <summary>
        /// Use this to create a new notification passing in the notification type and an args class
        /// </summary>
        /// <param name="_notificationType"></param>
        /// <param name="message"></param>
        public Notification(NusysConstants.NotificationType notificationType, INotificationArgumentable args)
        {
            _message = args.PackToNotificationKeys();
            if (_message == null)
            {
                _message = new Message();
            }
            _notificationType = notificationType;
        }

        /// <summary>
        /// Returns the final message of this notification
        /// </summary>
        /// <returns></returns>
        public Message GetFinalMessage()
        {
            _message[NusysConstants.NOTIFICATION_TYPE_STRING_KEY] = _notificationType.ToString();
            _message["system_sent_timestamp"] = DateTime.UtcNow.Ticks;//TODO fix this
            return _message;
        }

        /// <summary>
        ///  THIS METHOD SHOULD ONLY BE USED TO VERIFY THE NOTIFICATION CONTAINS THE CORRECT KEYS AND OBJECTS.  
        /// 
        /// Please do not put any logic or calculations in this method.  
        /// It makes it hard to understand where things are happening
        /// </summary>
        /// <returns></returns>
        public virtual async Task CheckOutgoingNotification()
        {

        }

        public Message GetMessage()
        {
            return _message;
        }
    }
}