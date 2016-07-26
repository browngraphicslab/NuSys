using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using Windows.UI.Input.Inking;

namespace NuSysApp
{
    public class ServerClient
    {
        private MessageWebSocket _socket;
        private DataWriter _dataMessageWriter;

        private HashSet<string> libraryIdsUsed = new HashSet<string>();

        public delegate void MessageRecievedEventHandler(Message message);
        public event MessageRecievedEventHandler OnMessageRecieved;

        public delegate void ClientDroppedEventHandler(string id);
        public event ClientDroppedEventHandler OnClientDrop;//todo add this in, and onclientconnection event

        public delegate void ContentAvailableNotificationEventHandler(Dictionary<string, object> dict);
        public event ContentAvailableNotificationEventHandler OnContentAvailable;

        public delegate void ClientJoinedEventHandler(NetworkUser user);
        public event ClientJoinedEventHandler OnClientJoined;

        public delegate void LockAddedEventHandler(object sender, string id, string userId);
        public event LockAddedEventHandler OnLockAdded;

        public delegate void LockRemovedEventHandler(object sender, string id);
        public event LockRemovedEventHandler OnLockRemoved;

        public delegate void OnContentUpdatedEventHandler(object sender, LibraryElementController controller,Message message);
        public event OnContentUpdatedEventHandler OnContentUpdated;

        public delegate void PresentationLinkAddedEventHandler(object sender, string id1, string id2);
        public event PresentationLinkAddedEventHandler PresentationLinkAdded;

        public delegate void PresentationLinkRemovedEventHandler(object sender, string id1, string id2);
        public event PresentationLinkRemovedEventHandler PresentationLinkRemoved;

        public static HashSet<string> NeededLibraryDataIDs = new HashSet<string>();
        private ConcurrentDictionary<string,Message> _returnMessages = new ConcurrentDictionary<string, Message>();
        private ConcurrentDictionary<string, ManualResetEvent> _requestEventDictionary = new ConcurrentDictionary<string, ManualResetEvent>();
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
            ServerBaseURI = "://" + WaitingRoomView.ServerName + "/api/";
            var credentials = GetUserCredentials();
            var uri = GetUri("values/" + credentials, true);
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
            throw new Exception("Server client failed from web socket closing!");
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
                    string id = null;
                    if (dict.ContainsKey("server_indication_from_server"))
                    {
                        if (dict.ContainsKey("notification_type"))
                        {
                            Debug.WriteLine("got notification " + (string)dict["notification_type"]);
                            switch ((string)dict["notification_type"])
                            {
                                case "content_available":
                                    if (WaitingRoomView.UserName.ToLower() != "rosemary" && WaitingRoomView.UserName.ToLower() != "gfxadmin" && WaitingRoomView.UserName.ToLower() != "rms")
                                    {
                                        if (dict.ContainsKey("creator_user_id") && (dict["creator_user_id"].ToString().ToLower() == "rosemary" || dict["creator_user_id"].ToString().ToLower() == "rms"))
                                        {
                                            break;
                                        }
                                    }
                                    OnContentAvailable?.Invoke(dict);
                                    break;
                                case "add_user":
                                    id = (string)dict["user_id"];
                                    var user = new NetworkUser(id, dict);
                                    OnClientJoined?.Invoke(user);
                                    break;
                                case "remove_user":
                                    id = (string)dict["user_id"];
                                    OnClientDrop?.Invoke(id);
                                    break;
                                case "content_update":
                                    id = dict["id"] as string;
                                    dict.Remove("id");
                                    dict.Remove("notification_type");
                                    if (SessionController.Instance.ContentController.GetLibraryElementController(id) != null)
                                    {
                                        OnContentUpdated?.Invoke(this, SessionController.Instance.ContentController.GetLibraryElementController(id),new Message(dict));
                                    }
                                    break;
                                case "content_data_update":
                                    id = dict["id"] as string;
                                    if (SessionController.Instance.ContentController.GetLibraryElementController(id) != null)
                                    {
                                        var controller =
                                            SessionController.Instance.ContentController.GetLibraryElementController(id);
                                        var loadArgs = new LoadContentEventArgs();
                                        loadArgs.Data = dict["data"] as string;
                                        controller.Load(loadArgs);
                                    }
                                    break;
                                case "remove_presentation_link":
                                    PresentationLinkRemoved?.Invoke(this,dict["id1"] as string, dict["id2"] as string);
                                    break;
                                case "add_presentation_link":
                                    PresentationLinkAdded?.Invoke(this, dict["id1"] as string, dict["id2"] as string);
                                    break;
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
                //throw new IncomingDataReaderException();
            }
        }

        public async Task<string> DuplicateLibraryElement(string libraryElementId)
        {
            return await Task.Run(async delegate
            {
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(GetUri("duplicate/" + libraryElementId));

                string data;
                using (var responseContent = response.Content)
                {
                    data = await responseContent.ReadAsStringAsync();
                }
                return data;
            });
        }

        public async Task<HashSet<PresentationLinkModel>> GetPresentationLinks(string collectionContentId)
        {
            return await Task.Run(async delegate
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                };
                var client =
                    new HttpClient(new HttpClientHandler {ClientCertificateOptions = ClientCertificateOption.Automatic});
                var response = await client.GetAsync(GetUri("getpresentationlinks/" + collectionContentId));

                string data;
                using (var content = response.Content)
                {
                    data = await content.ReadAsStringAsync();
                }
                try
                {
                    var list = JsonConvert.DeserializeObject<List<TupleIntermediate<string, string>>>(data, settings);
                    var returnSet = list.Select(tup => new PresentationLinkModel() {InElementId = tup.m_Item1, OutElementId = tup.m_Item2});
                    return new HashSet<PresentationLinkModel>(returnSet);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("error parsing presentation links returned from server for presentationlink fetching");
                    return null;
                }
            });
        }
        public async Task<bool> RemovePresentationLink(string id1, string id2)
        {
            return await Task.Run(async delegate
            {
                var dict = new Dictionary<string, object>();
                dict["id1"] = id1;
                dict["id2"] = id2;
                var data = await SendDictionaryToServer("removepresentationlink", dict);
                try
                {
                    var success = bool.Parse(data);
                    return success;
                }
                catch (Exception boolParsException)
                {
                    Debug.WriteLine("error parsing bool returned from server for presentationlink removing");
                }
                return false;
            });
        }
        public async Task<bool> AddPresentationLink(string contentId, string id1, string id2)
        {
            return await Task.Run(async delegate
            {
                var dict = new Dictionary<string, object>();
                dict["id1"] = id1;
                dict["id2"] = id2;
                dict["contentId"] = contentId;
                var data = await SendDictionaryToServer("addpresentationlink", dict);
                try
                {
                    var success = bool.Parse(data);
                    return success;
                }
                catch (Exception boolParsException)
                {
                    Debug.WriteLine("error parsing bool returned from server for presentationlink removing");
                }
                return false;
            });
        }

        public async Task<Dictionary<string, string>> GetRegionMapping(string contentId)
        {
            return await Task.Run(async delegate
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                };
                var client =
                    new HttpClient(new HttpClientHandler { ClientCertificateOptions = ClientCertificateOption.Automatic });
                var response = await client.GetAsync(GetUri("getregionmapping/" + contentId));

                string data;
                using (var content = response.Content)
                {
                    data = await content.ReadAsStringAsync();
                }
                try
                {
                    var list = JsonConvert.DeserializeObject<Dictionary<string, string>>(data, settings);
                    return list;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("error parsing region mapping returned from server");
                    return null;
                }
            });
        }
        public async Task<bool> AddRegionToContent(string contentId, Region region)
        {
            return await Task.Run(async delegate
            {
                var dict = new Dictionary<string, object>();
                dict["data"] = region;
                dict["id"] = region.LibraryElementId;
                dict["contentId"] = contentId;
                var data = await SendDictionaryToServer("addregion", dict);
                try
                {
                    var success = bool.Parse(data);
                    return success;
                }
                catch (Exception boolParsException)
                {
                    Debug.WriteLine("error parsing bool returned from server for region adding");
                }
                return false;
            });
        }
        public async Task<bool> RemoveRegionFromContent(Region region)
        {
            return await Task.Run(async delegate
            {
                var dict = new Dictionary<string, object>();
                dict["data"] = region;
                dict["id"] = region.LibraryElementId;
                var data = await SendDictionaryToServer("removeregion", dict);
                try
                {
                    var success = bool.Parse(data);
                    return success;
                }
                catch (Exception boolParsException)
                {
                    Debug.WriteLine("error parsing bool returned from server for region removal");
                }
                return false;
            });
        }
        public async Task<bool> UpdateRegion(Region region)
        {
            return await Task.Run(async delegate
            {
                var dict = new Dictionary<string, object>();
                dict["data"] = region;
                dict["id"] = region.LibraryElementId;
                var data = await SendDictionaryToServer("updateregion", dict);
                try
                {
                    var success = bool.Parse(data);
                    return success;
                }
                catch (Exception boolParsException)
                {
                    Debug.WriteLine("error parsing bool returned from server for region updating");
                }
                return false;
            });
        }
        public async Task<string> SendDictionaryToServer(string postName, Dictionary<string, object> dict)
        {
            dict["sessionID"] = WaitingRoomView.ServerSessionID;
            var serialized = JsonConvert.SerializeObject(dict, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii });
            var client = new HttpClient(new HttpClientHandler { ClientCertificateOptions = ClientCertificateOption.Automatic });
            var response = await client.PostAsync(GetUri(postName + "/"), new StringContent(serialized, Encoding.UTF8, "application/xml"));
            string data;
            using (var content = response.Content)
            {
                data = await content.ReadAsStringAsync();
            }
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(data);
                data = doc.ChildNodes[0].InnerText;
            }
            catch (Exception boolParseException)
            {
                Debug.WriteLine("error parsing string from sending dictionary to server");
            }
            return data;
        }
        public async Task<LoadContentEventArgs> GetContentWithoutData(string contentId)
        {
            try
            {
                return await Task.Run(async delegate
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                    var client = new HttpClient(new HttpClientHandler { ClientCertificateOptions = ClientCertificateOption.Automatic });
                    var response = await client.GetAsync(GetUri("getcontentwithoutdata/"+contentId));

                    string data;
                    using (var content = response.Content)
                    {
                        data = await content.ReadAsStringAsync();
                    }
                    try
                    {
                        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(data, settings);
                        var inks = dict.ContainsKey("inks") ? JsonConvert.DeserializeObject<HashSet<string>>(dict["inks"].ToString()) : null;
                        var args = new LoadContentEventArgs(null,inks);
                        return args;
                    }
                    catch (Exception boolParsException)
                    {
                        Debug.WriteLine("error parsing bool and serverSessionId returned from server");
                    }
                    return null;
                });

            }
            catch (Exception e)
            {
                //throw new Exception("couldn't connect to the server and get content info");
                return null;
            }
        }
        public async Task FetchLibraryElementData(string libraryId, int tries = 0)
        {
            try
            {
                if (libraryIdsUsed.Contains(libraryId))
                {
                    return;
                }
                if (tries > 30)
                {
                    return;
                }
                libraryIdsUsed.Add(libraryId);
                await Task.Run(async delegate
                {
                    SessionController.Instance.ContentController.GetLibraryElementController(libraryId)?.SetLoading(true);
                    HttpClient client = new HttpClient();
                    var response = await client.GetAsync(GetUri("getcontent/" + libraryId));

                    string data;
                    using (var responseContent = response.Content)
                    {
                        data = await responseContent.ReadAsStringAsync();
                    }

                    if (SessionController.Instance.ContentController.GetContent(libraryId) != null && SessionController.Instance.ContentController.GetContent(libraryId).Type == ElementType.Video)
                    {
                        if (data == "{}")
                        {
                            libraryIdsUsed.Remove(libraryId);
                            await FetchLibraryElementData(libraryId, tries++);
                            return;
                        }
                    }

                    JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(data, settings);

                    if (!dict.ContainsKey("data") || !dict.ContainsKey("id") || !dict.ContainsKey("type"))
                    {
                        NeededLibraryDataIDs.Add(libraryId);
                        return;
                    }
                    await ParseFetchedLibraryElement(dict, libraryId);
                });
            }
            catch (Exception e)
            {
                //TODO add in error handling
            }
        }

        private async Task ParseFetchedLibraryElement(Dictionary<string, object> dict, string libraryId)

        {
            JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            var contentData = (string)dict["data"] ?? "";

            var id = (string)dict["id"];

            var inks = dict.ContainsKey("inks") ? JsonConvert.DeserializeObject<HashSet<string>>(dict["inks"].ToString()) : null;

            if (NeededLibraryDataIDs.Contains(id))
            {
                NeededLibraryDataIDs.Remove(id);
            }

            if (dict.ContainsKey("inklist"))
            {
                HashSet<InkStroke> set = new HashSet<InkStroke>();
                var inklines = JsonConvert.DeserializeObject<List<string>>(dict["inklist"].ToString(), settings);
                var newInkLines = new HashSet<string>();
                foreach (var inkline in inklines)
                {
                    var inkdict = JsonConvert.DeserializeObject<Dictionary<string, object>>(inkline, settings);
                    //var inkpoints = JsonConvert.DeserializeObject<List<InkPoint>>(inkdict["inkpoints"].ToString());
                    //var inktype = inkdict["type"] as string;
                    var inkid = inkdict["id"] as string;
                    //var inkcolor = inkdict["color"];
                    //var builder = new InkStrokeBuilder();
                    //var inkstroke = builder.CreateStrokeFromInkPoints(inkpoints, Matrix3x2.Identity);


                    /*
                    var newWrapper = new InkWrapper(inkstroke, inktype);
                    InkStorage._inkStrokes.Add(inkid, newWrapper);
                    newInkLines.Add(inkid);    */
                    var m = new Message();
                    m["data"] = inkline;
                    m["id"] = inkid;
                    var model =
                        SessionController.Instance.ContentController.GetContent(libraryId) as
                            CollectionLibraryElementModel;
                    if (!model.InkLines.Contains(inkid))
                    {
                        model.InkLines.Add(inkid);
                        SessionController.Instance.NuSysNetworkSession.ExecuteRequestLocally(new AddInkRequest(m));
                    }
                }
            }

            LibraryElementModel content = SessionController.Instance.ContentController.GetContent(libraryId);
            if (content == null)
            {
                content = LibraryElementModelFactory.CreateFromMessage(new Message(dict));
            }
            if (content != null)
            {
                await UITask.Run(async delegate
                {
                    var args = new LoadContentEventArgs(contentData, inks);
                    SessionController.Instance.ContentController.GetLibraryElementController(content.LibraryElementId)
                        .Load(args);
                });
            }
        }
        public async Task SendDictionaryToServer(Dictionary<string, string> dict)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            var serialized = JsonConvert.SerializeObject(dict, settings);
            await SendToServer(serialized);
        }
        public async Task<HashSet<string>> SearchOverLibraryElements(string searchText)
        {
            return await Task.Run(async delegate
            {
                try
                {
                    HttpClient client = new HttpClient();
                    var response = await client.GetAsync(GetUri("search/" + searchText));

                    string data;
                    using (var responseContent = response.Content)
                    {
                        data = await responseContent.ReadAsStringAsync();
                    }
                    JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                    var list = JsonConvert.DeserializeObject<HashSet<string>>(data, settings);
                    return list;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error searching on server");
                    return null;
                }
            });
        }

        public async Task<List<SearchResult>> AdvancedSearchOverLibraryElements(Query searchQuery)
        {
            return await Task.Run(async delegate
            {
                try
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                    var dict = new Dictionary<string, object>();
                    dict["SEARCH"] = JsonConvert.SerializeObject(searchQuery,settings);
                    //HttpClient client = new HttpClient();
                    var data = await SendDictionaryToServer("advancedsearch", dict);
                    
                    try
                    {
                        var list = JsonConvert.DeserializeObject<List<SearchResult>>(data);
                        return list;
                    }
                    catch (Exception deserializeException)
                    {
                        Debug.WriteLine("error parsing list returned from server for advacned search");
                    }
                    return null;

                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error searching on server");
                    return null;
                }
            });
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
                throw new Exception("Exception caught during writing to server data writer.  Reason: "+e.Message);
            }
        }
        public async Task<Dictionary<string, Dictionary<string, object>>> GetRepo()
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
            var final = new Dictionary<string, Dictionary<string, object>>();
            foreach (var kvp in deserialized)
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(kvp.Value.ToString(), settings);
                if (WaitingRoomView.UserName.ToLower() != "rosemary" && WaitingRoomView.UserName.ToLower() != "gfxadmin" && WaitingRoomView.UserName.ToLower() != "rms")
                {
                    if (dict.ContainsKey("creator_user_id") && (dict["creator_user_id"].ToString().ToLower() == "rosemary" || dict["creator_user_id"].ToString().ToLower() == "rms"))
                    {
                        continue;
                    }
                }
                final[kvp.Key] = dict;
            }
            return final;
        }
        /// <summary>
        /// Returns the byte array that should be written directly into a file for docx saving and loading
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<byte[]> GetDocxBytes(string id)
        {
            var url = GetUri("getworddoc/" + id);
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            string data;
            using (var content = response.Content)
            {
                data = await content.ReadAsStringAsync();
            }
            var list = JsonConvert.DeserializeObject<List<string>>(data);
            var converted = Convert.FromBase64String(list[0]);
            return converted;
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

        /// <summary>
        /// Will send a dictionary to the server and manually wait for its return
        /// Later, another message will be called that will resumet this thread after placing the returned response in the _returnMessages dictionary
        /// THESE METHOD PAIRS SHOULD SIMULATE ACTUAL ASYNCHRONOUS CALLS
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<Message> WaitGetRequestAsync(Message message)
        {
            Debug.Assert(!message.ContainsKey(NusysConstants.ServerConstants.GET_REQUEST_ID_STRING));
            var mreId = SessionController.Instance.GenerateId();
            message[NusysConstants.ServerConstants.GET_REQUEST_ID_STRING] = mreId;
            var mre = new ManualResetEvent(false);
            _requestEventDictionary.TryAdd(mreId, mre);
            Task.Run(async delegate
            {
                SendMessageToServer(message);
            });
            mre.WaitOne();
            Debug.Assert(_returnMessages.ContainsKey(mreId));
            Message outMessage;
            _returnMessages.TryRemove(mreId, out outMessage);
            Debug.Assert(outMessage != null);
            return outMessage;
        }

        /// <summary>
        /// will be called when a message is recieved and is a get request
        /// will resume the waiting thread for the get request and place the message in the message dictionary
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task ReturnGetRequestAsync(Message message)
        {
            Debug.Assert(message.ContainsKey(NusysConstants.ServerConstants.GET_REQUEST_ID_STRING));
            var mreId = message.GetString(NusysConstants.ServerConstants.GET_REQUEST_ID_STRING);
            var mre = _requestEventDictionary[mreId];
            ManualResetEvent outMre;
            _requestEventDictionary.TryRemove(mreId, out outMre);
            _returnMessages.TryAdd(mreId, message);
            mre.Set();
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

        private class SearchIntermediate
        {
            public string Data { set; get; }

            public SearchIntermediate(string data)
            {
                Data = data;
            }
        }

        private class TupleIntermediate<S,T>
        {
            public S m_Item1 { get; set; }
            public T m_Item2 { get; set; }
        }
        public class IncomingDataReaderException : Exception
        {
            public IncomingDataReaderException(string s = "") : base("Error with incoming data reader message.  " + s) { }
        }
    }
}

