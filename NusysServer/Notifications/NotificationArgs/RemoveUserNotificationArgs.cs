using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// the arguments class for notifying when a client drops
    /// </summary>
    public class RemoveUserNotificationArgs : INotificationArgumentable
    {
        /// <summary>
        /// The nusys client ID that you want to drop
        /// REQUIRED
        /// </summary>
        public string ClientIdToDrop{ get; set; }

        /// <summary>
        ///  this pack to notification keys method just adds id of the client you are dropping
        /// </summary>
        /// <returns></returns>
        public Message PackToNotificationKeys()
        {
            var message = new Message();
            Debug.Assert(ClientIdToDrop != null);

            //weird serialization things for security reasons
            message[NusysConstants.DROP_USER_NOTIFICATION_USER_ID_KEY] = ClientIdToDrop;

            return message;
        }
    }
}