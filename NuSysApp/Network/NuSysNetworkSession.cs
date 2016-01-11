using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.System.Threading;
using NuSysApp.Network.Requests;

namespace NuSysApp
{
    public class NuSysNetworkSession
    {
        #region Public Members

        public string HostIP
        {
            get { return _hostIP; }
        }
        public bool IsHostMachine
        {
            get { return _networkSession.LocalIP == _hostIP; }
        }
        #endregion Public Members
        #region Private Members
        private string LocalIP
        {
            get { return _networkSession.LocalIP; }
        }
        
        private HashSet<string> NetworkMembers
        {
            get { return _networkSession.NetworkMembers; }
        }
        private ConcurrentDictionary<string, ManualResetEvent> _requestEventDictionary = new ConcurrentDictionary<string, ManualResetEvent>();
        private NetworkSession _networkSession;
        private string _hostIP;

        #endregion Private Members

        public async Task Init()
        {
            _networkSession = new NetworkSession(await GetNetworkIPs());
            await _networkSession.Init();

            if (NetworkMembers.Count <= 1)
            {
                _hostIP = LocalIP; //just makes this machine the host
                Debug.WriteLine("This machine made to be host");
                if (NetworkMembers.Count == 0)
                {
                    NetworkMembers.Add(LocalIP);
                }
            }
            else
            {
                await ExecuteSystemRequest(new AddClientSystemRequest(LocalIP));
            }

            _networkSession.OnPing += async () => {
                const string URL = "http://aint.ch/nusys/clients.php";
                var urlParameters = "?action=ping&ip=" + LocalIP;
                var client = new HttpClient { BaseAddress = new Uri(URL) };
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(urlParameters);
            };
            
            _networkSession.OnMessageRecieved += async (message, type, ip) =>
            {
                await ProcessIncomingRequest(message, type, ip);
            };
            
            _networkSession.OnClientDrop += async ip =>
            {
                await ExecuteSystemRequest(new RemoveClientSystemRequest(ip));
            };
        }
        #region Requests
        public async Task ExecuteRequest(Request request, NetworkClient.PacketType packetType = NetworkClient.PacketType.TCP)
        {
            await Task.Run(async delegate {

                await request.CheckOutgoingRequest();
                Message message = request.GetFinalMessage();
                if (packetType == NetworkClient.PacketType.TCP)
                {
                    ManualResetEvent mre = new ManualResetEvent(false);
                    string requestID = SessionController.Instance.GenerateId();
                    _requestEventDictionary[requestID] = mre;

                    message["local_request_id"] = requestID;

                    if (request.GetRequestType() == Request.RequestType.SystemRequest)
                    {
                        await SendSystemRequest(message);
                    }
                    else
                    {
                        await SendRequest(message, packetType);
                    }
                    if (_requestEventDictionary.ContainsKey(requestID))
                        mre.WaitOne();
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

        public async Task ExecuteSystemRequest(SystemRequest request, NetworkClient.PacketType packetType = NetworkClient.PacketType.TCP, ICollection < string> recieverIPs = null)
        {
            await request.CheckOutgoingRequest();
            await SendSystemRequest(request.GetFinalMessage(), recieverIPs);
        } 
        private async Task SendSystemRequest(Message message, ICollection<string> recieverIPs = null)
        {
            await _networkSession.SendRequestMessage(message, recieverIPs == null ? NetworkMembers : recieverIPs, NetworkClient.PacketType.TCP);
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
                    await _networkSession.SendRequestMessage(message, NetworkMembers, packetType);
                    break;
            }
        }

        private async Task ProcessIncomingRequest(Message message, NetworkClient.PacketType packetType, string ip = null)
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
                await ProcessIncomingSystemRequest(message, ip);
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
                case Request.RequestType.NewContentRequest:
                    request = new NewContentRequest(message);
                    break;
                case Request.RequestType.FinalizeInkRequest:
                    request = new FinalizeInkRequest(message);
                    break;
                case Request.RequestType.DuplicateNodeRequest:
                    request = new DuplicateNodeRequest(message);
                    break;
                default:
                    throw new InvalidRequestTypeException("The request type could not be found and made into a request instance");
            }

            await UITask.Run(async () =>
            {
                await request.ExecuteRequestFunction();//switches to UI thread
            });
            if(packetType == NetworkClient.PacketType.TCP)
                await ResumeWaitingRequestThread(message);
            if (IsHostMachine && packetType == NetworkClient.PacketType.TCP)
                await _networkSession.SendRequestMessage(message, NetworkMembers, NetworkClient.PacketType.TCP);
        }

        private async Task ResumeWaitingRequestThread(Message message)
        {
            if (message.ContainsKey("local_request_id"))
            {
                var local_id = message.GetString("local_request_id");
                if (_requestEventDictionary.ContainsKey(local_id))
                {
                    var mre = _requestEventDictionary[local_id];
                    ManualResetEvent outMre;
                    _requestEventDictionary.TryRemove(local_id, out outMre);
                    mre.Set();
                }
            }
        }
        private async Task ProcessIncomingSystemRequest(Message message, string ip)
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
                case SystemRequest.SystemRequestType.AddClient:
                    request = new AddClientSystemRequest(message);
                    break;
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
            await request.ExecuteSystemRequestFunction(this,_networkSession, ip);
        }
        #endregion Requests
        private async Task SendMessageToHost(Message message, NetworkClient.PacketType packetType, string ip = null)
        {
            if (IsHostMachine)
            {
                await ProcessIncomingRequest(new Message(message.GetSerialized()), NetworkClient.PacketType.TCP,ip);
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
            _hostIP = ip;
            Debug.WriteLine("Machine "+ip+" made to be host");
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
