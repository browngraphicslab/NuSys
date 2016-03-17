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
using System.Xml;
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
        }

        public async Task Init()
        {
            ServerBaseURI = "://"+WaitingRoomView.ServerName+"/api/";
            var credentials = GetUserCredentials();
            var uri = GetUri("values/"+credentials, true);
            await _socket.ConnectAsync(uri);
        }

        private string GetUserCredentials()
        {
            return WaitingRoomView.ServerSessionID.ToString();
        }
        private Uri GetUri(string additionToBase, bool useWebSocket = false)
        {
            var firstpart = useWebSocket ? "ws" : "http";
            firstpart += WaitingRoomView.TEST_LOCAL_BOOLEAN ? "" : "s";
            return new Uri(firstpart + ServerBaseURI + additionToBase);
        }

        private void SocketClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            //TODO add in closing handler 
        }

        private async void MessageRecieved(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                using (DataReader reader = args.GetDataReader())

                {
                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    string read = reader.ReadString(reader.UnconsumedBufferLength);
                    //Debug.WriteLine(read + "\r\n");
                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                    };
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(read, settings);
                    if (dict.ContainsKey("server_indication_from_server"))
                    {
                        if (dict.ContainsKey("notification_type") &&
                            (string)dict["notification_type"] == "content_available")
                        {
                            if (dict.ContainsKey("id"))
                            {
                                var id = dict["id"];

                                //id, data, type, title
                                var contentId = (string)dict["id"];
                                string title = null;
                                ElementType type = ElementType.Text;
                                string data = dict.ContainsKey("data") ? (string)dict["data"] : null;
                                if (dict.ContainsKey("title"))
                                {
                                    title = (string)dict["title"];
                                }
                                if (dict.ContainsKey("type"))
                                {
                                    type = (ElementType)Enum.Parse(typeof(ElementType), (string)dict["type"], true);
                                }

                                NodeContentModel element = new NodeContentModel(data,contentId,type,title);
                                UITask.Run(delegate
                                {

                                    // TODO: add back in
                                    //                    SessionController.Instance.Library.AddNewElement(element);
                                    //                                SessionController.Instance.LibraryBucketViewModel.AddNewElement(element);
                                });
                                Task.Run(async delegate {
                                    await GetContent((string)id);
                                });
                            }
                        }

                    }
                    else
                    {
                        OnMessageRecieved?.Invoke(new Message(dict));
                    }
                }
            }
            catch (Exception e)
            {
                throw new IncomingDataReaderException();
            }
        }

        public async Task<List<Dictionary<string,object>>> GetContentWithoutData(List<string> contentIds)
        {
            try
            {
                return await Task.Run(async delegate
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };

                    var contentIdStrings = JsonConvert.SerializeObject(contentIds, settings);

                    var client = new HttpClient( new HttpClientHandler{ClientCertificateOptions = ClientCertificateOption.Automatic});
                    var response = await client.PostAsync(GetUri("getcontent/"), new StringContent(contentIdStrings, Encoding.UTF8, "application/xml"));

                    string data;
                    using (var content = response.Content)
                    {
                        data = await content.ReadAsStringAsync();
                    }
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(data);
                        var list =  JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(doc.ChildNodes[0].InnerText, settings);
                        return list;
                    }
                    catch (Exception boolParsException)
                    {
                        Debug.WriteLine("error parsing bool and serverSessionId returned from server");
                    }
                    return new List<Dictionary<string, object>>();
                });

            }
            catch (Exception e)
            {
                //throw new Exception("couldn't connect to the server and get content info");
                return new List<Dictionary<string, object>>();
            }
        }
        public async Task GetContent(string contentId)
        {
            try
            {
                await Task.Run(async delegate
                {
                    HttpClient client = new HttpClient();
                    var response = await client.GetAsync(GetUri("getcontent/" + contentId));
                
                    string data;
                    using (var responseContent = response.Content)
                    {
                        data = await responseContent.ReadAsStringAsync();
                    }

                    JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(data, settings);

                    var contentData = (string)dict["data"] ?? "";

                    var contentTitle = dict.ContainsKey("title") ? (string)dict["title"] : null;
                    var contentType = dict.ContainsKey("type")
                        ? (ElementType) Enum.Parse(typeof (ElementType), (string)dict["type"], true)
                        : ElementType.Text;
                    //var contentAliases = dict.ContainsKey("aliases") ? JsonConvert.DeserializeObject<List<string>>(dict["aliases"].ToString()) : new List<string>();
                    NodeContentModel content;
                    if (contentType == ElementType.Collection)
                    {
                        content = new CollectionContentModel(contentId, null, contentTitle);
                    }
                    else
                    {
                        content = new NodeContentModel(contentData, contentId, contentType, contentTitle);
                    }
                    await UITask.Run(async delegate
                    {
                        var cc = SessionController.Instance.ContentController;
                        if (cc.Get(contentId) != null)
                        {
                            cc.Get(contentId).Data = contentData;
                            cc.Get(contentId).Loaded = true;
                            cc.Get(contentId).FireContentChanged();
                            /*
                        if (cc.ContainsAndLoaded(contentId))
                        {
                            cc.Get(contentId).Data = contentData;
                        }
                        else
                        {
                            cc.Get(contentId).Data = contentData;
                            cc.Get(contentId).Loaded = true;
                            cc.Get(contentId).FireContentChanged();
                        }*/
                        }
                        else
                        {
                            SessionController.Instance.ContentController.Add(content);
                        }

                        if (SessionController.Instance.LoadingDictionary.ContainsKey(contentId))
                        {
                            foreach (var controller in SessionController.Instance.LoadingDictionary[contentId])
                            {
                                await controller.FireContentLoaded(content);
                            }
                        }
                    });
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

        public async Task<List<Message>> GetWorkspaceAsElementMessages(string id)
        {
            var url = GetUri("getworkspace/" + id);
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            string data;
            using (var content = response.Content)
            {
                data = await content.ReadAsStringAsync();
            }
            var list = JsonConvert.DeserializeObject<List<string>>(data);
            var returnList = new List<Message>();
            foreach (var s in list)
            {
                returnList.Add(new Message(s));
            }
            return returnList;
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

        public class IncomingDataReaderException : Exception
        {
            public IncomingDataReaderException(string s = "") : base("Error with incoming data reader message.  " + s){}
        }
    }
}
