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

namespace NuSysApp
{
    public class NuSysNetworkSession
    {
        #region Public Members
        #endregion Public Members
        #region Private Members
        private string LocalIP
        {
            get { return _networkSession.LocalIP; }
        }
        private bool IsHostMachine
        {
            get { return _networkSession.LocalIP == _hostIP; }
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
                await ExecuteRequest(new AddClientSystemRequest(LocalIP));
            }

            _networkSession.OnPing += async () => {
                const string URL = "http://aint.ch/nusys/clients.php";
                var urlParameters = "?action=ping&ip=" + LocalIP;
                var client = new HttpClient { BaseAddress = new Uri(URL) };
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(urlParameters);
            };
            _networkSession.OnMessageRecieved += MessageRecieved;
            _networkSession.OnClientDrop += async ip =>
            {
                await ExecuteRequest(new RemoveClientSystemRequest(ip));
            };
        }
        private void MessageRecieved(Message message, NetworkClient.PacketType type, string ip)
        {
            ProcessIncomingRequest(message, type, ip);
        }
        #region Requests
        public async Task ExecuteRequest(Request request, NetworkClient.PacketType packetType = NetworkClient.PacketType.TCP)
        {
            await ThreadPool.RunAsync(async delegate
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                string requestID = Guid.NewGuid().ToString("N");
                _requestEventDictionary[requestID] = mre;

                await request.CheckRequest();
                Message message = request.GetFinalMessage();
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
            });
        }

        public async Task ExecuteSystemRequest(SystemRequest request, NetworkClient.PacketType packetType = NetworkClient.PacketType.TCP, ICollection < string> recieverIPs = null)
        {
            await request.CheckRequest();
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
            }
            switch (packetType)
            {
                case NetworkClient.PacketType.TCP:
                    await SendMessageToHost(message, packetType);
                    break;
                case NetworkClient.PacketType.UDP:
                    await ProcessIncomingRequest(message,packetType);
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
            switch (requestType)
            {
                case Request.RequestType.DeleteSendableRequest:
                    request = new DeleteSendableRequest(message);
                    break;
                case Request.RequestType.NewNodeRequest:
                    request = new NewNodeRequest(message);
                    break;
                case Request.RequestType.SystemRequest:
                    await ProcessIncomingSystemRequest(message, ip);
                    return;
                default:
                    throw new InvalidRequestTypeException("The request type could not be found and made into a request instance");
            }

            await UITask.Run(async () =>
            {
                await request.ExecuteRequestFunction();
            });
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
                default:
                    throw new InvalidRequestTypeException("The system request type could not be found and made into a request instance");
            }
            await request.ExecuteSystemRequestFunction(this,_networkSession, ip);
            await ResumeWaitingRequestThread(message);

        }
        #endregion Requests
        private async Task SendMessageToHost(Message message, NetworkClient.PacketType packetType, string ip = null)
        {
            if (IsHostMachine)
            {
                await ProcessIncomingRequest(message, NetworkClient.PacketType.TCP,ip);
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
