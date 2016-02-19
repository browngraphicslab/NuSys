using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI;
using NuSysApp.Network.Requests;
using NuSysApp.Network.Requests.SystemRequests;
using Buffer = System.Buffer;

namespace NuSysApp
{
    public class NuSysNetworkSession
    {
        #region Public Members

        public string HostIP
        {
            get { return _hostIP; }
        }
        public string LocalIP
        {
            get { return _networkSession?.LocalIP; }
        }

        public bool IsHostMachine
        {
            get { return _networkSession.LocalIP == _hostIP; }
        }
        public Dictionary<string, NetworkUser> NetworkMembers;

        public delegate void NewUserEventHandler(NetworkUser user);
        public event NewUserEventHandler OnNewNetworkUser;

        public delegate void UserDroppedEventHandler(NetworkUser user);
        public event UserDroppedEventHandler OnNetworkUserDropped;

        public delegate void HostChangeEventHandler(NetworkUser newHost, NetworkUser oldHost);
        public event HostChangeEventHandler OnHostChange;

        #endregion Public Members
        #region Private Members
        private HashSet<string> NetworkMemberIPs
        {
            get { return _networkSession.NetworkMembers; }
        }
        private ConcurrentDictionary<string, ManualResetEvent> _requestEventDictionary = new ConcurrentDictionary<string, ManualResetEvent>();
        private NetworkSession _networkSession;
        private string _hostIP;
        private ServerClient _serverClient;
        #endregion Private Members

        public async Task Init()
        {
            _serverClient = new ServerClient();
            await _serverClient.Init();

            NetworkMembers = new Dictionary<string, NetworkUser>();
            _networkSession = new NetworkSession(await GetNetworkIPs());
            await _networkSession.Init();

            if (NetworkMemberIPs.Count <= 1)
            {
                SetHost(LocalIP); //just makes this machine the host
                Debug.WriteLine("This machine made to be host");
                if (NetworkMemberIPs.Count == 0)
                {
                    NetworkMemberIPs.Add(LocalIP);
                }
            }
            else
            {
                //await ExecuteSystemRequest(new AddClientSystemRequest(LocalIP));//TODO FIX
                //await ExecuteSystemRequest(new SendClientInfoSystemRequest());//TODO FIX
            }

            _networkSession.OnPing += async () => {
                const string URL = "http://aint.ch/nusys/clients.php";
                var urlParameters = "?action=ping&ip=" + LocalIP;
                var client = new HttpClient { BaseAddress = new Uri(URL) };
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        await client.GetAsync(urlParameters);
                    }
                    catch (Exception e)
                    {
                    // TODO: handle better
                        Debug.WriteLine("couldn't send ping");
                    }
            };
            
            _networkSession.OnMessageRecieved += async (message, type, ip) =>
            {
                //await ProcessIncomingRequest(message, type, ip);
            };
            _serverClient.OnMessageRecieved += async (m) => { ProcessIncomingRequest(m); };
            _networkSession.OnClientDrop += ClientDrop;
            /*
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient hclient = new HttpClient();
            //hclient.BaseAddress = new Uri("http://localhost:8080/");
            //var uri = new Uri("GET:test");

            //var htp = await hclient.PutAsync(new Uri("http://localhost:44584/api/values/1"), new StringContent("testinsert"));

            //var response = await hclient.GetAsync(new Uri("http://nurepo6916.azurewebsites.net/api/values/1"));
            var response = await hclient.GetAsync(new Uri("http://localhost:54764/api/values/"+SessionController.Instance.GenerateId()));
            using (var content = response.Content)
            {
                var s = await content.ReadAsStringAsync();
                Debug.WriteLine(s);
            }
            */
            //WebSocket socket = new 




            //var s = "trent test";
            //byte[] bytes = Convert.FromBase64String(s);
            //var buff = bytes.AsBuffer();
            //await socket.OutputStream.WriteAsync(buff);


        }
        #region Requests

        public async Task ExecuteRequestLocally(Request request)
        {
            await request.CheckOutgoingRequest();
            var m = new Message(request.GetFinalMessage().GetSerialized());
            await ProcessIncomingRequest(m, true);
        }
        public async Task ExecuteRequest(Request request, NetworkClient.PacketType packetType = NetworkClient.PacketType.TCP)
        {
            await Task.Run(async delegate {

                await request.CheckOutgoingRequest();
                Message message = request.GetFinalMessage();

                await _serverClient.SendMessageToServer(message);

                if (true)
                    return;
                if (request.WaitForRequestReturn())
                {
                    ManualResetEvent mre = new ManualResetEvent(false);
                    string requestID = SessionController.Instance.GenerateId();
                    _requestEventDictionary[requestID] = mre;

                    message["system_local_request_id"] = requestID;

                    if (request.GetRequestType() == Request.RequestType.SystemRequest)
                    {
                        await SendSystemRequest(message);
                    }
                    else
                    {
                        await SendRequest(message, packetType);
                    }
                    if (_requestEventDictionary.ContainsKey(requestID))
                    {
                        mre.WaitOne();
                    }
                }
                else
                {
                    if (request.GetRequestType() == Request.RequestType.SystemRequest)
                    {
                        await SendSystemRequest(message);
                    }
                    else
                    {
                        await SendRequest(message, packetType);
                    }
                }
            });
        }

        public async Task ExecuteSystemRequest(SystemRequest request, NetworkClient.PacketType packetType = NetworkClient.PacketType.TCP, ICollection < string> recieverIPs = null, bool sendOnlyToHost = false)
        {
            await request.CheckOutgoingRequest();
            if (sendOnlyToHost)
            {
                if (IsHostMachine)
                {
                    await ProcessIncomingRequest(request.GetFinalMessage());
                }
                else
                {
                    await SendSystemRequest(request.GetFinalMessage(), new List<string>() {HostIP});
                }
            }
            else
            {
                await SendSystemRequest(request.GetFinalMessage(), recieverIPs);
            }
        }

        public async Task ExecuteSystemRequestLocally(SystemRequest request)
        {
            await request.CheckOutgoingRequest();
            await ProcessIncomingSystemRequest(request.GetFinalMessage());
        }

        private async Task SendSystemRequest(Message message, ICollection<string> recieverIPs = null)
        {
            await _networkSession.SendRequestMessage(message, recieverIPs ?? NetworkMemberIPs, NetworkClient.PacketType.TCP);
        }

        private async Task SendRequest(Message message,NetworkClient.PacketType packetType, ICollection<string> recieverIPs = null)
        {
            if (recieverIPs != null)
            {
                await _networkSession.SendRequestMessage(message, recieverIPs, packetType);
                return;
            }
            switch (packetType)
            {
                case NetworkClient.PacketType.TCP:
                    await SendMessageToHost(message, packetType);
                    break;
                case NetworkClient.PacketType.UDP:
                    await _networkSession.SendRequestMessage(message, NetworkMemberIPs, packetType);
                    break;
            }
        }

        private async Task ProcessIncomingRequest(Message message, bool local = false)
        {
            Request request;
            Request.RequestType requestType;
            if (!message.ContainsKey("request_type"))
            {
                throw new NoRequestTypeException();
            }
            try
            {
                requestType = (Request.RequestType) Enum.Parse(typeof (Request.RequestType), message.GetString("request_type"));
            }
            catch(Exception e)
            {
                throw new InvalidRequestTypeException();
            }
            if (requestType == Request.RequestType.SystemRequest)
            {
                await ProcessIncomingSystemRequest(message);
                return;
            }
            switch (requestType)
            {
                case Request.RequestType.DeleteSendableRequest:
                    request = new DeleteSendableRequest(message);
                    break;
                case Request.RequestType.NewNodeRequest:
                    request = new NewNodeRequest(message);
                    break;
                case Request.RequestType.NewLinkRequest:
                    request = new NewLinkRequest(message);
                    break;
                case Request.RequestType.NewGroupRequest:
                    request = new NewGroupRequest(message);
                    break;
                case Request.RequestType.NewThumbnailRequest:
                    request = new NewThumbnailRequest(message);
                    break;
                case Request.RequestType.SendableUpdateRequest:
                    request = new SendableUpdateRequest(message);
                    break;
                case Request.RequestType.FinalizeInkRequest:
                    request = new FinalizeInkRequest(message);
                    break;
                case Request.RequestType.DuplicateNodeRequest:
                    request = new DuplicateNodeRequest(message);
                    break;
                case Request.RequestType.ChangeContentRequest:
                    request = new ChangeContentRequest(message);
                    break;
                case Request.RequestType.SetTagsRequest:
                    request = new SetTagsRequest(message);
                    break;
                case Request.RequestType.ChatDialogRequest:
                    request = new ChatDialogRequest(message);
                    break;
                case Request.RequestType.CreateNewContentRequest:
                    request = new CreateNewContentRequest(message);
                    break;
                default:
                    throw new InvalidRequestTypeException("The request type could not be found and made into a request instance");
            }
            var systemDict = new Dictionary<string, object>();
            var systemIP = (string)(message.ContainsKey("system_sender_ip") ? message["system_sender_ip"] : LocalIP);
            systemDict["system_sender_ip"] = systemIP;
            if (systemDict.ContainsKey(systemIP))
            {
                systemDict["system_sender_networkuser"] = NetworkMembers[systemIP];
            }
            request.SetSystemProperties(systemDict);
            await UITask.Run(async () =>
            {
                await request.ExecuteRequestFunction();//switches to UI thread
            });
            await ResumeWaitingRequestThread(message);
        }

        private async Task ResumeWaitingRequestThread(Message message)
        {
            if (message.ContainsKey("system_local_request_id"))
            {
                var local_id = message.GetString("system_local_request_id");
                if (_requestEventDictionary.ContainsKey(local_id))
                {
                    var mre = _requestEventDictionary[local_id];
                    ManualResetEvent outMre;
                    _requestEventDictionary.TryRemove(local_id, out outMre);
                    mre.Set();
                }
            }
        }
        private async Task ProcessIncomingSystemRequest(Message message)
        {
            SystemRequest request;
            SystemRequest.SystemRequestType requestType;
            if (!message.ContainsKey("system_request_type"))
            {
                throw new NoRequestTypeException("No system request type was found for the system request");
            }
            try
            {
                requestType = (SystemRequest.SystemRequestType)Enum.Parse(typeof(SystemRequest.SystemRequestType), message.GetString("system_request_type"));
            }
            catch (Exception e)
            {
                throw new InvalidRequestTypeException();
            }
            switch (requestType)
            {
                case SystemRequest.SystemRequestType.RemoveClient:
                    request = new RemoveClientSystemRequest(message);
                    break;
                case SystemRequest.SystemRequestType.SetHost:
                    request = new SetHostSystemRequest(message);
                    break;
                case SystemRequest.SystemRequestType.SendWorkspace:
                    request = new SendWorkspaceRequest(message);
                    break;
                default:
                    throw new InvalidRequestTypeException("The system request type could not be found and made into a request instance");
            }
            await request.ExecuteSystemRequestFunction(this,_networkSession, _serverClient);
        }
        #endregion Requests
        private async Task SendMessageToHost(Message message, NetworkClient.PacketType packetType, string ip = null)
        {
            if (IsHostMachine)
            {
                await ProcessIncomingRequest(new Message(message.GetSerialized()));
            }
            else
            {
                await _networkSession.SendRequestMessage(message, _hostIP, packetType);
            }
        }
        /*
       * adds self to php script list of IP's 
       * called once at the beginning to get the list of other IP's on the network
       */
        private async Task<HashSet<string>> GetNetworkIPs()
        {
            if (WaitingRoomView.IsLocal)
            {
                HashSet<string> list = new HashSet<string>();
                return list;
            }
            else
            {
                const string URL = "http://aint.ch/nusys/clients.php";
                var urlParameters = "?action=add&ip=" + NetworkInformation.GetHostNames()
                    .FirstOrDefault(h => h.IPInformation != null && h.IPInformation.NetworkAdapter != null)
                    .RawName; 

                var client = new HttpClient { BaseAddress = new Uri(URL) };

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var people = "";

                var response = await client.GetAsync(urlParameters);
                if (response.IsSuccessStatusCode)
                {
                    var d = response.Content.ReadAsStringAsync().Result; //gets the response from the php script
                    people = d;
                }
                else
                {
                    Debug.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                }
                var split = people.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                return new HashSet<string>(split.ToList());
            }
        }

        public void SetHost(string ip)
        {
            NetworkUser oldHost = null;
            if (_hostIP != null && NetworkMembers.ContainsKey(_hostIP))
            {
                oldHost = NetworkMembers[_hostIP];
            }
            _hostIP = ip;
            OnHostChange?.Invoke(NetworkMembers.ContainsKey(ip) ? NetworkMembers[ip] : null, oldHost);
            Debug.WriteLine("Machine "+ip+" made to be host");
        }

        public void AddNetworkUser(NetworkUser user)
        {
            var add = NetworkMembers.ContainsKey(user.IP);
            if (!add)
            {
                NetworkMembers[user.IP] = user;
                OnNewNetworkUser?.Invoke(user);
            }
        }
        public async Task DropNetworkUser(string ip)
        {
            if (ip != null)
            {
                if (NetworkMembers.ContainsKey(ip))
                {
                    var user = NetworkMembers[ip];
                    NetworkMembers.Remove(ip);
                    await UITask.Run(async delegate {
                                                        OnNetworkUserDropped?.Invoke(user);
                    });
                }
                _networkSession.RemoveIP(ip);
            }
        }

        public async void ClientDrop(string ip)
        {
            if (ip != null)
            {
                await DropNetworkUser(ip);
                if (ip == _hostIP)
                {
                    SetHost(LocalIP);
                }
                await ExecuteSystemRequest(new SetHostSystemRequest(LocalIP));
                await ExecuteSystemRequest(new RemoveClientSystemRequest(ip));
            }
        }

        public async Task FetchContent(string id)
        {
            await _serverClient.GetContent(id);
        }

        public async Task<Dictionary<string,Dictionary<string,object>>> GetAllLibraryElements()
        {
            return await _serverClient.GetRepo();
        }
    }
    public class NoRequestTypeException : Exception
    {
        public NoRequestTypeException(string message) : base(message) { }
        public NoRequestTypeException() : base("No Request Type was found") { }
    }
    public class InvalidRequestTypeException : Exception
    {
        public InvalidRequestTypeException(string message) : base(message) { }
        public InvalidRequestTypeException() : base("The Request Type was invalid, maybe it isn't contained in the RequestType definition") { }
    }
}
