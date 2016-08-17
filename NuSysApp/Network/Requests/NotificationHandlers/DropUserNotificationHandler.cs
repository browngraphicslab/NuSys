using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// the notification handler for whenever a new client is added 
    /// </summary>
    public class DropUserNotificationHandler : NotificationHandler
    {
        /// <summary>
        /// this handler will add the user passed in the notification message as a json. 
        /// </summary>
        /// <param name="notificationMessage"></param>
        public override void HandleNotification(Message notificationMessage)
        {
            AssertType(notificationMessage, NusysConstants.NotificationType.RemoveUser);
            Debug.Assert(notificationMessage.ContainsKey(NusysConstants.DROP_USER_NOTIFICATION_USER_ID_KEY));
            var userId = notificationMessage.GetString(NusysConstants.DROP_USER_NOTIFICATION_USER_ID_KEY);

            if (NuSysNetworkSession.NetworkMembers.ContainsKey(userId))
            {
                NetworkUser outUser;
                NuSysNetworkSession.NetworkMembers.TryRemove(userId, out outUser);
            }

            //fire the event in the nusys network session
            NuSysNetworkSession.FireClientDrop(userId);
        }
    }
}
