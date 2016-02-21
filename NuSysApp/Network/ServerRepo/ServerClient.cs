using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
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
        private ManualResetEvent _manualResetEvent;
        private bool _waiting = false;

        public delegate void MessageRecievedEventHandler(Message message);
        public event MessageRecievedEventHandler OnMessageRecieved;

        public delegate void ClientDroppedEventHandler(string ip);
        public event ClientDroppedEventHandler OnClientDrop;//todo add this in, and onclientconnection event

        public string ServerBaseURI { get; private set; }

        public ServerClient()//Server name: http://nurepo6916.azurewebsites.net/api/values/1
        {
            _socket = new MessageWebSocket();
            _socket.Control.MessageType = SocketMessageType.Utf8;
            _socket.MessageReceived += MessageRecieved;
            _socket.Closed += SocketClosed;
            _dataMessageWriter = new DataWriter(_socket.OutputStream);
            _manualResetEvent = new ManualResetEvent(false);
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
            //TODO add in closing handler 
        }

        private async void MessageRecieved(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            using (DataReader reader = args.GetDataReader())
            {
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                string read = reader.ReadString(reader.UnconsumedBufferLength);
                //Debug.WriteLine(read + "\r\n");
                JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(read, settings);
                if (dict.ContainsKey("server_indication_from_server"))
                {
                    if (dict.ContainsKey("notification_type") && (string)dict["notification_type"] == "content_available")
                    {
                        if (dict.ContainsKey("id"))
                        {
                            var id = dict["id"];
                            LibraryElement element = new LibraryElement(dict);
                            UITask.Run(delegate {
                                                    SessionController.Instance.Library.AddNewElement(element);
                            });
                            await GetContent((string) id);
                        }
                    }
                }
                else
                {
                    OnMessageRecieved?.Invoke(new Message(dict));
                }
            }
        }

        public async Task GetContent(string contentId)
        {
            try
            {
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(GetUri("getcontent/" + contentId));
                
                string data;
                using (var content = response.Content)
                {
                    data = await content.ReadAsStringAsync();
                }
                await UITask.Run(async delegate
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(data, settings);

                    var contentData = (string)dict["data"] ?? "";
                    var contentTitle = dict.ContainsKey("title") ? (string)dict["title"] : null;
                    var contentAliases = dict.ContainsKey("aliases") ? JsonConvert.DeserializeObject<List<string>>(dict["aliases"].ToString()) : new List<string>();
                    var content = new NodeContentModel(contentData, contentId, contentTitle, contentAliases);
                    if (SessionController.Instance.ContentController.Get(contentId) == null)
                    {
                        SessionController.Instance.ContentController.Add(content);
                    }
                    else
                    {
                        SessionController.Instance.ContentController.OverWrite(content);
                    }
                    if (SessionController.Instance.LoadingNodeDictionary.ContainsKey(contentId))
                    {
                        foreach (var tuple in SessionController.Instance.LoadingNodeDictionary[contentId])
                        {
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
                    }
                });
            }
            catch (Exception e)
            {
                //TODO add in error handling
            }
        }
        public async Task SendDictionaryToServer(Dictionary<string, string> dict)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            var serialized = JsonConvert.SerializeObject(dict,settings);
            await SendToServer(serialized);
        }

        public async Task SendMessageToServer(Message message)
        {
            await SendToServer(message.GetSerialized());
        }
        private async Task SendToServer(string message)
        {
            try
            {
                _dataMessageWriter.WriteString(message);
                await _dataMessageWriter.StoreAsync();
            }
            catch (Exception e)
            {
                throw new Exception("Exception caught during writing to server data writer");
            }
        }
        public async Task<Dictionary<string,Dictionary<string,object>>> GetRepo()
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(GetUri("getcontent"));

            string data;
            using (var content = response.Content)
            {
                data = await content.ReadAsStringAsync();
            }
            JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            var deserialized = JsonConvert.DeserializeObject<Dictionary<string, object>>(data, settings);
            var final = new Dictionary<string,Dictionary<string,object>>();
            foreach (var kvp in deserialized)
            {
                final[kvp.Key] = JsonConvert.DeserializeObject<Dictionary<string, object>>(kvp.Value.ToString(), settings);
            }
            return final;
        }
        /*
        public async Task DeleteAllRepoFiles()
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(GetUri("delete"));
        }

        public async Task<bool> DeleteContent(string id)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(GetUri("delete/"+id));
            string data;
            using (var content = response.Content)
            {
                data = await content.ReadAsStringAsync();
            }
            bool success = bool.Parse(data.Substring(1,data.Length-2));
            return success;
        }

        public async Task<bool> UpdateContent(string id, Dictionary<string, string> dict)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            var uri = GetUri("update/" + id);
            client.BaseAddress = uri;
            var message = new HttpRequestMessage();
            message.Content = new StringContent(JsonConvert.SerializeObject(dict, settings), Encoding.UTF8, "application/json");
            
            var response = await client.SendAsync(message);
            string data;
            using (var content = response.Content)
            {
                data = await content.ReadAsStringAsync();
            }
            try
            {
                bool success = bool.Parse(data);
                return success;
            }
            catch (Exception e)
            {
                return false;
            }


        }
        */
    }
}
