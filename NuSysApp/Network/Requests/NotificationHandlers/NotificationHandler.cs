using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    abstract class NotificationHandler
    {
        /// <summary>
        /// This is just so that the subclasses dont need to keep using SessionController.Instance.NuSysNetworkSession
        /// </summary>
        public NuSysNetworkSession NuSysNetworkSession { get; private set; }
         
        /// <summary>
        /// method used to get the message from a notification
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected Message GetNotificationMessage(Notification notification)
        {
            var message = notification.GetMessage();
            return message;
            NuSysNetworkSession = SessionController.Instance.NuSysNetworkSession;

        }
       
        /// <summary>
        /// the method that is called by the request router to actually handle an incoming request.  
        /// The senderHandler is the WebSocketHandler of the original sender.
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        public abstract Message HandleNotification(Notification notification);

    }
}
