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
    public class AddUserNotificationHandler : NotificationHandler
    {
        /// <summary>
        /// this handler will add the user passed in the notification message as a json. 
        /// </summary>
        /// <param name="notificationMessage"></param>
        public override void HandleNotification(Message notificationMessage)
        {
            AssertType(notificationMessage, NusysConstants.NotificationType.AddUser);
            Debug.Assert(notificationMessage.ContainsKey(NusysConstants.ADD_USER_NOTIFICATION_USER_JSON_KEY));
            var user = notificationMessage.Get<NetworkUser>(NusysConstants.ADD_USER_NOTIFICATION_USER_JSON_KEY);

            if (!NuSysNetworkSession.NetworkMembers.ContainsKey(user.UserID))
            {
                NuSysNetworkSession.NetworkMembers[user.UserID] = user;
            }

            if (!NuSysNetworkSession.UserIdToDisplayNameDictionary.ContainsKey(user.UserID))
            {
                NuSysNetworkSession.UserIdToDisplayNameDictionary.Add(user.UserID, user.DisplayName);
            }

            //fire the event in the nusys network session
            NuSysNetworkSession.FireAddNetworkUser(user);
        }
    }
}
