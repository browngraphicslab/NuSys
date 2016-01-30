﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Newtonsoft.Json;
using NuSysApp.Network.Requests.SystemRequests;

namespace NuSysApp
{
    public class ServerClient
    {
        private MessageWebSocket _socket;
        private DataWriter _dataMessageWriter;

        public string ServerBaseURI { get; private set; }

        public ServerClient()//Server name: http://nurepo6916.azurewebsites.net/api/values/1
        {
            _socket = new MessageWebSocket();
            _socket.Control.MessageType = SocketMessageType.Utf8;
            _socket.MessageReceived += MessageRecieved;
            _socket.Closed += SocketClosed;
            _dataMessageWriter = new DataWriter(_socket.OutputStream);
        }

        public async Task Init()
        {
            ServerBaseURI = "://"+WaitingRoomView.ServerName+"/api/";
            var uri = GetUri("values", true);
            await _socket.ConnectAsync(uri);
        }

        private Uri GetUri(string additionToBase, bool useWebSocket = false)
        {
            var firstpart = useWebSocket ? "ws" : "http";
            return new Uri(firstpart + ServerBaseURI + additionToBase);
        }

        private void SocketClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            //TODO
        }

        private async void MessageRecieved(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            using (DataReader reader = args.GetDataReader())
            {
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                string read = reader.ReadString(reader.UnconsumedBufferLength);
                //Debug.WriteLine(read + "\r\n");
                JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(read, settings);
                if (dict.ContainsKey("notification_type") && dict["notification_type"] == "content_available")
                {
                    if (dict.ContainsKey("id"))
                    {

                        var id = dict["id"];
                        var request = new ContentAvailableNotificationSystemRequest(id);
                        var network = SessionController.Instance.NuSysNetworkSession;
                        await network.ExecuteSystemRequestLocally(request);
                    }
                }
            }
        }

        public async Task GetContent(string contentId)
        {
            try
            {
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(GetUri("contents/"+contentId));

                string data;
                using (var content = response.Content)
                {
                    data = await content.ReadAsStringAsync();
                }
                if (data.Length >= 2)
                {
                    data = data.Substring(1, data.Length - 2);
                }
                await UITask.Run(async delegate
                {
                    SessionController.Instance.ContentController.Add(data, contentId);
                    if (SessionController.Instance.LoadingNodeDictionary.ContainsKey(contentId))
                    {
                        var tuple = SessionController.Instance.LoadingNodeDictionary[contentId];
                        LoadNodeView view = tuple.Item2;
                        AtomModel model = tuple.Item1;
                        var factory = new FreeFormNodeViewFactory();
                        FrameworkElement newView;
                        newView = await factory.CreateFromSendable(model, null);
                        SessionController.Instance.ActiveWorkspace.Children.Remove(model.Id);
                        SessionController.Instance.ActiveWorkspace.Children.Add(model.Id, newView);
                        SessionController.Instance.ActiveWorkspace.AtomViewList.Remove(view);
                        SessionController.Instance.ActiveWorkspace.AtomViewList.Add(newView);
                    }
                });
            }
            catch (Exception e)
            {
                //TODO add in error handling
            }
        }

        public async Task CreateOrUpdateContent(Dictionary<string, string> dict)
        {
            try
            {
                if (!dict.ContainsKey("id") || !dict.ContainsKey("data"))
                {
                    throw new Exception("Adding content must contain and id, data, and type");
                }
                JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                var serialized = JsonConvert.SerializeObject(dict,settings);
                _dataMessageWriter.WriteString(serialized);
                await _dataMessageWriter.StoreAsync();
            }
            catch (Exception e)
            {
                
            }
        }
    }
}
