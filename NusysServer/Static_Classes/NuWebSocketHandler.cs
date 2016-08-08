using Microsoft.ServiceModel.WebSockets;
using Microsoft.Web.WebSockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;


namespace NusysServer
{
    public class NuWebSocketHandler : WebSocketHandler
    {
        /// <summary>
        /// the list of all the current connected clients.  Each client is one websockethandler
        /// </summary>
        private static WebSocketCollection AllClients = new WebSocketCollection();
        private static JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };

        /// <summary>
        /// essentially the constructor.  Called whenever a new client, and therefore new nuwebsockethandler, is made
        /// </summary>
        public override void OnOpen()
        {
            AllClients.Add(this);
            var ip = this.WebSocketContext.UserHostAddress;

            this.MaxIncomingMessageSize = Int32.MaxValue;//TODO broadcast this openeing somewhere

            BroadcastNewUser(ActiveClient.ActiveClients[this]);
            foreach (var activeClient in ActiveClient.ActiveClients)
            {
                var dict = GetUserAdditionDict(activeClient.Value);
                if (activeClient.Key != this)
                {
                    this.Send(dict);
                }
            }

            var list = new List<string>();

            for(int i = 1; i <= 105; i++)
            {
                list.Add("A random string for testing topic modeling");
            }

            Task.Run(async delegate {
                await TextProcessor.GetTextAnalytics(list);
            });

        }

        /// <summary>
        /// the event handler whenever the socket is closed by either end
        /// </summary>
        public override void OnClose()
        {
            if (ActiveClient.ActiveClients.ContainsKey(this))
            {
                BroadcastRemovingUser(ActiveClient.ActiveClients[this]);
                ActiveClient.ActiveClients[this].Disconnect();
            }
        }

        /// <summary>
        /// called automatically whenever a socketHandler gets a message from its client
        /// </summary>
        /// <param name="message"></param>
        public override void OnMessage(string message)
        {
            Task.Run(delegate
            {
                Dictionary<string, object> dict = null;
                try
                {
                    dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(message, settings);
                }
                catch (JsonReaderException jre)
                {
                    var matches = Regex.Match(message, "(?:({[^}]+}) *)*");
                    string[] miniStrings = matches.Groups[1].Captures.Cast<Capture>().Select(c => c.Value).ToArray();

                    if (miniStrings.Length > 1)
                    {
                        foreach (var s in miniStrings)
                        {
                            OnMessage(s);
                        }
                    }
                    else
                    {
                        try
                        {
                            dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(message, settings);
                        }
                        catch (Exception shitIsFuckedUp)
                        {
                            ErrorLog.AddError(shitIsFuckedUp);
                            ErrorLog.AddErrorString("regex json error above ^^^^");
                        }
                    }
                }
                if (dict != null)
                {
                    try
                    {
                        var success = RequestRouter.HandleRequest(new Message(dict), this);
                        
                    }
                    catch (Exception e)
                    {
                        ErrorLog.AddError(e);
                        Send(e.Message);
                    }
                }
            });
            //BroadcastToSubset(message,new List<NuWebSocketHandler>() {this});
        }

        public static void BroadcastNewUser(ActiveClient client)
        {
            var dict = GetUserAdditionDict(client);
            Broadcast(dict);
        }

        public static void BroadcastRemovingUser(ActiveClient client)
        {
            var dict = GetUserRemoveDict(client);
            Broadcast(dict);
        }

        /// <summary>
        /// THIS SHOULD BE DEPRICATED
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private static Dictionary<string, object> GetUserAdditionDict(ActiveClient client)
        {
            var dict = client.Client.GetDict();
            //dict[Constants.FROM_SERVER_MESSAGE_INDICATOR_STRING] = true;
            dict["notification_type"] = "add_user";
            dict["user_id"] = client.Client.ID;
            return dict;
        }

        /// <summary>
        /// THIS SHOULD BE DEPRICATED
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private static Dictionary<string, object> GetUserRemoveDict(ActiveClient client)
        {
            var dict = new Dictionary<string, object>();
            //dict[Constants.FROM_SERVER_MESSAGE_INDICATOR_STRING] = true;
            dict["notification_type"] = "remove_user";
            dict["user_id"] = client.Client.ID;
            return dict;
        }
        /*
        public static void BroadcastContentUpdate(string id, IEnumerable<string> keysToUpdate,
            HashSet<NuWebSocketHandler> ignoreHandlers = null)
        {
            ignoreHandlers = ignoreHandlers ?? new HashSet<NuWebSocketHandler>();
            if (id == null || ContentsHolder.Instance.Contents[id] == null)
            {
                return;
            }
            var dict = new Dictionary<string, object>();
            var contentDict = ContentsHolder.Instance.Contents[id].Dictionary;

            foreach (var key in keysToUpdate)
            {
                if (contentDict.ContainsKey(key))
                {
                    dict[key] = contentDict[key];
                }
            }
            dict["notification_type"] = "content_update";
            dict[Constants.FROM_SERVER_MESSAGE_INDICATOR_STRING] = true;
            dict["id"] = id;
            foreach (var client in clients)
            {
                if (ignoreHandlers.Contains(client))
                {
                    continue;
                }
                (client as NuWebSocketHandler)?.Send(dict);
            }
        }*/
        /*
        public static void BroadcastContentDataUpdate(NusysContent content)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict[Constants.FROM_SERVER_MESSAGE_INDICATOR_STRING] = true;
            dict["notification_type"] = "content_data_update";
            var id = content.GetID();
            dict["id"] = id;
            dict["data"] = content.GetData();

            HashSet<string> set = new HashSet<string>();
            if (Alias.ContentIdtoAliasesDictionary.ContainsKey(id))
            {
                foreach (var alias in Alias.ContentIdtoAliasesDictionary[id])
                {
                    set.Add(alias.Creator);
                }
            }
            foreach (string collectionID in set)
            {
                if (ActiveClient.CollectionSubscriptions.ContainsKey(collectionID))
                {
                    foreach (var activeClient in ActiveClient.CollectionSubscriptions[collectionID])
                    {
                        activeClient.SocketHandler.Send(dict);
                    }
                }
            }
        }

        public static void BroadcastContentAvailable(NusysContent content)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>(content.Dictionary);
            dict[Constants.FROM_SERVER_MESSAGE_INDICATOR_STRING] = true;
            dict["notification_type"] = "content_available";
            dict["id"] = content.GetID();
            Broadcast(dict);
        }*/

        /// <summary>
        /// broadcasts a message to all clients except the ones in the exclusions list
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exclusions"></param>
        public static void BroadcastToSubset(Message message, HashSet<NuWebSocketHandler> exclusions = null)
        {
            exclusions = exclusions ?? new HashSet<NuWebSocketHandler>();
            foreach (var socket in AllClients)
            {
                if (!exclusions.Contains(socket))
                {
                    socket.Send(message.GetSerialized());
                }
            }
        }

        /// <summary>
        /// sends a dictionary to the client instance
        /// </summary>
        /// <param name="dict"></param>
        public void Send(Dictionary<string, object> dict)
        {
            Send(JsonConvert.SerializeObject(dict, settings));
        }


        /// <summary>
        /// STATIC.  
        /// sends a message to all clients
        /// </summary>
        /// <param name="message"></param>
        public static void Broadcast(Message message)
        {
            Broadcast(message.GetSerialized());
        }

        /// <summary>
        /// STATIC.  
        /// broadcasts a dictionary to all clients.  Should probably be replaced by the overload taking in a message
        /// </summary>
        /// <param name="dict"></param>
        private static void Broadcast(Dictionary<string, object> dict)
        {
            var message = JsonConvert.SerializeObject(dict, settings);
            Broadcast(message);
        }

        /// <summary>
        /// STATIC.  
        /// lowest-level broadcast method that takes in a string and sends it to all the clients
        /// </summary>
        /// <param name="message"></param>
        private static void Broadcast(string message)
        {
            AllClients.Broadcast(message);
        }
    }
}