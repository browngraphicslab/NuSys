using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    public class NusysClient
    {
        /// <summary>
        ///  The dictionary of Active users. 
        ///  This will store the users and the key will be their active websocket handler
        /// ONLY ACTIVE USERS SHOULD BE IN THIS DICTIONARY
        /// </summary>
        public static ConcurrentDictionary<NuWebSocketHandler, NusysClient> IDtoUsers = new ConcurrentDictionary<NuWebSocketHandler, NusysClient>();

        /// <summary>
        /// the dictiory of clients who have requested a login and been given a session id.  
        /// They will later create a web socket with the server under the pretense of a session id.  
        /// They will only be accepted during the web socket creation attempt if their sessionId is present in this dictionary
        /// </summary>
        public static ConcurrentDictionary<string, NusysClient> PreSessionClients = new ConcurrentDictionary<string, NusysClient>();

        /// <summary>
        /// the double hashed username of this user.
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// the stringified password salt applied ot the singly-hashed password
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// the double hashed and singly salted password of this user
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// the list of ten or fewer most recently visited collection Id's
        /// </summary>
        public List<string> LastVisitedCollections { get; set; }

        /// <summary> 
        /// the display name of the current user
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// static method used to add a session id and client to the list of waiting clients.  
        /// Clients in this list have sessions established with this id, but have yet to start the sessions.
        /// </summary>
        /// <param name="sessionString"></param>
        /// <param name="client"></param>
        public static void WaitForClient(string sessionString, NusysClient client)
        {
            PreSessionClients.TryAdd(sessionString, client);
        }

        /// <summary>
        /// creates and returns a NusysClient from the database keys in a message. 
        /// 
        /// Static method.
        /// </summary>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public static NusysClient CreateFromDatabaseMessage(Message userMessage)
        {
            //create the new user based off of databse keys
            var user = new NusysClient()
            {
                DisplayName = userMessage.GetString(NusysConstants.USERS_TABLE_USER_DISPLAY_NAME_KEY),
                LastVisitedCollections = userMessage.GetList<string>(NusysConstants.USERS_TABLE_LAST_TEN_COLLECTIONS_USED_KEY),
                UserID = userMessage.GetString(NusysConstants.USERS_TABLE_HASHED_USER_ID_KEY),
                Password = userMessage.GetString(NusysConstants.USERS_TABLE_HASHED_PASSWORD_KEY),
                Salt = userMessage.GetString(NusysConstants.USERS_TABLE_SALT_KEY)
            };
            return user;
        }

        /// <summary>
        /// static method to create a web socket handler from a session Id.  
        /// This method will return true if there was an awaiting session with that ID.
        /// If this method returns true, it will also have added the handler to the dictionary of active clients
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static bool FetchAwaitingSession(string sessionId, NuWebSocketHandler handler)
        {
            //if either are null there is clearly an invalid session, return false
            if (sessionId == null || handler == null)
            {
                return false;
            }
            if (PreSessionClients.ContainsKey(sessionId))
            {
                //remove the session from the list of waiting sessions
                NusysClient outClient;
                PreSessionClients.TryRemove(sessionId, out outClient);

                //add the handler and client to the list of active users by WebSocketHandler
                IDtoUsers.TryAdd(handler, outClient);

                return true;
            }
            return false;
        }
    }
}
