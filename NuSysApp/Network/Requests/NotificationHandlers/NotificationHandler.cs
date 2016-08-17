using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public abstract class NotificationHandler
    {
        /// <summary>
        /// This is just so that the subclasses dont need to keep using SessionController.Instance.NuSysNetworkSession
        /// </summary>
        public NuSysNetworkSession NuSysNetworkSession = SessionController.Instance.NuSysNetworkSession;
       
        /// <summary>
        /// the method that is called by the nusysNetwork session to handle notifications
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        public abstract void HandleNotification(Message message);

        /// <summary>
        /// protected method used to assert that a message is of a certain type. 
        ///  simply increases code reuse
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        protected void AssertType(Message notificationMessage, NusysConstants.NotificationType type)
        {
            Debug.Assert(notificationMessage.ContainsKey(NusysConstants.NOTIFICATION_TYPE_STRING_KEY));
            Debug.Assert(notificationMessage.GetEnum<NusysConstants.NotificationType>(NusysConstants.NOTIFICATION_TYPE_STRING_KEY) == type);
        }
    }
}
