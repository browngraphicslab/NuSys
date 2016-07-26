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
        private static WebSocketCollection clients = new WebSocketCollection();
        private static JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
        public override void OnOpen()
        {
            clients.Add(this);
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
        }

        public override void OnClose()
        {
            if (ActiveClient.ActiveClients.ContainsKey(this))
            {
                BroadcastRemovingUser(ActiveClient.ActiveClients[this]);
                ActiveClient.ActiveClients[this].Disconnect();
            }//TODO Broadcast this closing somewhere
        }


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

        private static Dictionary<string, object> GetUserAdditionDict(ActiveClient client)
        {
            var dict = client.Client.GetDict();
            dict[Constants.FROM_SERVER_MESSAGE_INDICATOR_STRING] = true;
            dict["notification_type"] = "add_user";
            dict["user_id"] = client.Client.ID;
            return dict;
        }
        private static Dictionary<string, object> GetUserRemoveDict(ActiveClient client)
        {
            var dict = new Dictionary<string, object>();
            dict[Constants.FROM_SERVER_MESSAGE_INDICATOR_STRING] = true;
            dict["notification_type"] = "remove_user";
            dict["user_id"] = client.Client.ID;
            return dict;
        }

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
        }
        public static void BroadcastPresentationLinkRemove(string id1, string id2)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict[Constants.FROM_SERVER_MESSAGE_INDICATOR_STRING] = true;
            dict["notification_type"] = "remove_presentation_link";
            dict["id1"] = id1;
            dict["id2"] = id2;
            Broadcast(dict);
        }
        public static void BroadcastPresentationLinkAdd(string id1, string id2, string contentId)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict[Constants.FROM_SERVER_MESSAGE_INDICATOR_STRING] = true;
            dict["notification_type"] = "add_presentation_link";
            dict["id1"] = id1;
            dict["id2"] = id2;
            if (ActiveClient.CollectionSubscriptions.ContainsKey(contentId))
            {
                foreach (var client in ActiveClient.CollectionSubscriptions[contentId])
                {
                    client.SocketHandler?.Send(dict);
                }
            }
        }
        public static void BroadcastRegionUpdate(string region, string id, HashSet<NuWebSocketHandler> ignoreHandlers)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict[Constants.FROM_SERVER_MESSAGE_INDICATOR_STRING] = true;
            dict["notification_type"] = "region_update";
            dict["region_string"] = region;
            dict["region_id"] = id;
            foreach (var client in clients)
            {
                if (ignoreHandlers.Contains(client))
                {
                    continue;
                }
                (client as NuWebSocketHandler)?.Send(dict);
            }
        }
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
        }
        public static void BroadcastToSubset(string message, IEnumerable<string> collectionIDs, HashSet<NuWebSocketHandler> exclusions = null)
        {
            exclusions = exclusions ?? new HashSet<NuWebSocketHandler>();
            HashSet<NuWebSocketHandler> socketsToBroadcast = new HashSet<NuWebSocketHandler>();
            foreach (string collectionID in collectionIDs)
            {
                if (ActiveClient.CollectionSubscriptions.ContainsKey(collectionID))
                {
                    foreach (var activeClient in ActiveClient.CollectionSubscriptions[collectionID])
                    {
                        socketsToBroadcast.Add(activeClient.SocketHandler);
                    }
                }
            }
            foreach (var socket in socketsToBroadcast)
            {
                if (!exclusions.Contains(socket))
                {
                    socket.Send(message);
                }
            }
        }
        public static void BroadcastToSubset(Dictionary<string, object> dict, Func<WebSocketHandler, bool> func)
        {
            var message = JsonConvert.SerializeObject(dict, settings);
            foreach (var socket in clients)
            {
                if (func(socket))
                {
                    socket.Send(message);
                }
            }
        }
        public static void BroadcastToSubset(string message, IEnumerable<WebSocketHandler> set, IEnumerable<WebSocketHandler> exclusions)
        {
            foreach (var socket in set)
            {
                if (!exclusions.Contains(socket))
                {
                    socket.Send(message);
                }
            }
        }
        private static void Broadcast(Dictionary<string, object> dict)
        {
            var message = JsonConvert.SerializeObject(dict, settings);
            Broadcast(message);
        }

        private static void Broadcast(string message)
        {
            clients.Broadcast(message);
        }

        public void Send(Dictionary<string, object> dict)
        {
            Send(JsonConvert.SerializeObject(dict, settings));
        }
    }
}