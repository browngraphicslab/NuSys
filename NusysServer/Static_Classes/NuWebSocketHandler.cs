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

            BroadcastNewUser(NusysClient.IDtoUsers[this]);
            foreach (var activeClient in NusysClient.IDtoUsers)
            {
                var dict = GetUserAdditionNotification(activeClient.Value);
                if (activeClient.Key != this)
                {
                    this.Notify(dict);
                }
            }
        }

        /// <summary>
        /// method that will get create a notification object that indicates the current user is joining the network.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private static DropUserNotification GetUserRemovalNotification(NusysClient client)
        {
            return new DropUserNotification(new RemoveUserNotificationArgs() {ClientIdToDrop = client.UserID });
        }

        /// <summary>
        /// method that will get create a notification object that indicates the current user is joining the network.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private static NewUserNotification GetUserAdditionNotification(NusysClient client)
        {
            return new NewUserNotification(new NewUserNotificationArgs() { ClientToAdd = client});
        }

        /// <summary>
        /// the event handler whenever the socket is closed by either end
        /// </summary>
        public override void OnClose()
        {
            if (NusysClient.IDtoUsers.ContainsKey(this))
            {
                BroadcastRemovingUser(NusysClient.IDtoUsers[this]);
                NusysClient outClient;
                NusysClient.IDtoUsers.TryRemove(this, out outClient);
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

        public static void BroadcastNewUser(NusysClient client)
        {
            var notification = GetUserAdditionNotification(client);
            NotifyAll(notification);
        }

        public static void BroadcastRemovingUser(NusysClient client)
        {
            var notification = GetUserRemovalNotification(client);
            NotifyAll(notification);
        }


        /// <summary>
        /// to send an error message to the client.  
        /// Just pass in the exception and the message will be send to the client.
        /// </summary>
        /// <param name="e"></param>
        public void SendError(Exception e)
        {
            var errorMessage = new Message();
            errorMessage[NusysConstants.REQUEST_ERROR_MESSAGE_KEY] = e.Message + "  Stack Trace: "+e.StackTrace;
            if (this.WebSocketContext.IsClientConnected)
            {
                Send(errorMessage.GetSerialized());
            }
        }
        
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
        /// method used to notify a user of a notification.  
        /// </summary>
        /// <param name="notification"></param>
        public void Notify(Notification notification)
        {
            var m = notification.GetFinalMessage();
            Send(m.GetSerialized());
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

        /// <summary>
        /// method used to broadcast a notification to all clients that are currently connected.  
        /// This method will just forward the ntofication to each of the NuWebSocketHandlers' Nofity methods
        /// </summary>
        /// <param name="notification"></param>
        public static void NotifyAll(Notification notification)
        {
            foreach (var client in AllClients)
            {
                (client as NuWebSocketHandler)?.Notify(notification);
            }
        }
    }
}