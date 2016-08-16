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
    /// the arguments class for notiying 
    /// </summary>
    public class NewUserNotificationArgs : INotificationArgumentable
    {
        /// <summary>
        /// The nusys client that you want to add to another client.  
        /// REQUIRED
        /// </summary>
        public NusysClient ClientToAdd { get; set; }

        /// <summary>
        ///  this pack to notification keys method just adds the json for the client to add
        /// </summary>
        /// <returns></returns>
        public Message PackToNotificationKeys()
        {
            var message = new Message();
            Debug.Assert(ClientToAdd != null);

            //weird serialization things for security reasons
            message[NusysConstants.ADD_USER_NOTIFICATION_USER_JSON_KEY] = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<BaseClient>(JsonConvert.SerializeObject(ClientToAdd)));

            return message;
        }
    }
}