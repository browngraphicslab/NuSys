using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI;
using Newtonsoft.Json;
using NuSysApp.Network.Requests;
using NuSysApp.Network.Requests.SystemRequests;
using Buffer = System.Buffer;
namespace NuSysApp
{
    public class NuSysNetworkSession
    {
        #region Public Members
        public string LocalIP { get; private set; }

        public Dictionary<string, NetworkUser> NetworkMembers = new Dictionary<string, NetworkUser>();

        public delegate void NewUserEventHandler(NetworkUser user);
        public event NewUserEventHandler OnNewNetworkUser;
        public LockController LockController;
        #endregion Public Members
        #region Private Members
        private HashSet<string> NetworkMemberIPs
        {
            get { return new HashSet<string>(); }//_networkSession.NetworkMembers; }
        }
        private ConcurrentDictionary<string, ManualResetEvent> _requestEventDictionary = new ConcurrentDictionary<string, ManualResetEvent>();
        //private NetworkSession _networkSession;
        private string _hostIP;
        private ServerClient _serverClient;
        private HashSet<string> _regionUpdateDebounceList = new HashSet<string>();
        #endregion Private Members

        public async Task Init()
        {
            LocalIP = NetworkInformation.GetHostNames().FirstOrDefault(h => h.IPInformation != null && h.IPInformation.NetworkAdapter != null).RawName;
            _serverClient = new ServerClient();
            await _serverClient.Init();
            _serverClient.OnMessageRecieved += OnMessageRecieved;
            _serverClient.OnClientDrop += ClientDrop;
            _serverClient.OnContentAvailable += ContentAvailable;
            _serverClient.OnClientJoined += AddNetworkUser;
            _serverClient.OnRegionUpdated += RegionUpdated;
            _serverClient.OnContentUpdated += ContentUpdated;
            LockController = new LockController(_serverClient);
        }

        #region Requests

        public async Task ExecuteRequestLocally(Request request)
        {
            await request.CheckOutgoingRequest();
            var m = new Message(request.GetFinalMessage().GetSerialized());
            await ProcessIncomingRequest(m);
        }
        public async Task ExecuteRequest(Request request)
        {
            await Task.Run(async delegate {

                await request.CheckOutgoingRequest();
                Message message = request.GetFinalMessage();

                if (request.WaitForRequestReturn())
                {
                    ManualResetEvent mre = new ManualResetEvent(false);
                    string requestID = SessionController.Instance.GenerateId();
                    _requestEventDictionary[requestID] = mre;

                    message["system_local_request_id"] = requestID;

                    await _serverClient.SendMessageToServer(message);
                    if (_requestEventDictionary.ContainsKey(requestID))
                    {
                        mre.WaitOne();
                    }
                }
                else
                {
                    await _serverClient.SendMessageToServer(message);
                }
            });
        }

        public async Task ExecuteSystemRequestLocally(SystemRequest request)
        {
            await request.CheckOutgoingRequest();
            await ProcessIncomingSystemRequest(request.GetFinalMessage());
        }
        private async void ContentAvailable(Dictionary<string, object> dict)
        {
            if (dict.ContainsKey("id"))
            {
                var id = (string)dict["id"];
                string title = null;
                ElementType type = ElementType.Text;
                var metadata = new Dictionary<string, MetadataEntry>();
                if (dict.ContainsKey("title"))
                {
                    title = (string)dict["title"];
                }
                if (dict.ContainsKey("type"))
                {
                    type = (ElementType)Enum.Parse(typeof(ElementType), (string)dict["type"], true);
                }
                if (dict.ContainsKey("metadata"))
                {
                    metadata = JsonConvert.DeserializeObject<Dictionary<string, MetadataEntry>>(dict["metadata"].ToString());
                }

                UITask.Run(async delegate {
                    if (SessionController.Instance.ContentController.GetContent(id) != null)
                    {
                        var controller = SessionController.Instance.ContentController.GetLibraryElementController(id);
                        controller.SetTitle(title);//TODO make sure no other variables, like timestamp, need to be set here
                    }
                    else
                    {/*
                        if (type == ElementType.Collection)
                        {
                            SessionController.Instance.ContentController.Add(
                                new CollectionLibraryElementModel(id, metadata, title));
                        }
                        else
                        {
                            SessionController.Instance.ContentController.Add(
                                new LibraryElementModel(id, type, metadata, title));
                        }*/
                        var request = new CreateNewLibraryElementRequest(new Message(dict));
                        await ExecuteRequestLocally(request);
                    }
                    if (ServerClient.NeededLibraryDataIDs.Contains(id))
                    {
                        Task.Run(async () =>
                        {
                            await FetchLibraryElementData(id);
                            ServerClient.NeededLibraryDataIDs.Remove(id);
                        });

                    }
                    if (dict.ContainsKey("favorited"))
                    {
                        bool favorited = bool.Parse(dict["favorited"].ToString());
                        var model = SessionController.Instance.ContentController.GetContent(id);
                        if (model != null)
                        {
                            model.Favorited = favorited;
                        }
                    }
                    var message = new Message(dict);
                    await SessionController.Instance.ContentController.GetContent(id).UnPack(message);
                });
            }
        }
        private async void OnMessageRecieved(Message m)
        {
            try
            {
                await ProcessIncomingRequest(m);
            }
            catch (Exception)
            {

            }
        }
        private async Task ProcessIncomingRequest(Message message)
        {
            Request request;
            Request.RequestType requestType;
            if (!message.ContainsKey("request_type"))
            {
                throw new NoRequestTypeException();
            }
            try
            {
                requestType = (Request.RequestType)Enum.Parse(typeof(Request.RequestType), message.GetString("request_type"));
            }
            catch (Exception e)
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
                    request = new NewElementRequest(message);
                    break;
                case Request.RequestType.NewLinkRequest:
                    request = new NewLinkRequest(message);
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
                case Request.RequestType.CreateNewLibrayElementRequest:
                    request = new CreateNewLibraryElementRequest(message);
                    break;
                case Request.RequestType.DeleteLibraryElementRequest:
                    request = new DeleteLibraryElementRequest(message);
                    break;
                case Request.RequestType.AddInkRequest:
                    request = new AddInkRequest(message);
                    break;
                case Request.RequestType.RemoveInkRequest:
                    request = new RemoveInkRequest(message);
                    break;
                case Request.RequestType.NewPresentationLinkRequest:
                    request = new NewPresentationLinkRequest(message);
                    break;
                default:
                    throw new InvalidRequestTypeException("The request type could not be found and made into a request instance");
            }
            var systemDict = new Dictionary<string, object>();
            var systemIP = (string)((message.ContainsKey("system_sender_ip") ? message["system_sender_ip"] : ""));
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
                default:
                    throw new InvalidRequestTypeException("The system request type could not be found and made into a request instance");
            }
            await request.ExecuteSystemRequestFunction(this, _serverClient);
        }
        #endregion Requests
        private void ContentUpdated(object sender, LibraryElementController controller, Message message)
        {
            controller.UnPack(message);
        }
        private void RegionUpdated(string id, Region region)
        {
            UITask.Run(delegate
            {
                var controller = SessionController.Instance.RegionsController.GetRegionController(id);
                controller?.UnPack(region);
            });
        }
        public async Task<List<Message>> GetCollectionAsElementMessages(string id)
        {
            return await _serverClient.GetWorkspaceAsElementMessages(id);
        }
        public void AddNetworkUser(NetworkUser user)
        {
            var add = !NetworkMembers.ContainsKey(user.ID);
            if (add)
            {
                NetworkMembers[user.ID] = user;
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
                    user.Remove();
                }
            }
        }

        public async void ClientDrop(string id)
        {
            await DropNetworkUser(id);
        }
        public async Task FetchLibraryElementData(string id)
        {
            if (SessionController.Instance.ContentController.GetContent(id).Type == ElementType.PDF && false)
            {
                bool fileExists = await CachePDF.isFilePresent(id);

                if (fileExists) // exists in cache
                {
                    var cacheData = await CachePDF.readFile(id);
                    await UITask.Run(
                        async delegate
                        {
                            SessionController.Instance.ContentController.GetLibraryElementController(id).Load(new LoadContentEventArgs(cacheData));
                            await SessionController.Instance.NuSysNetworkSession.FetchLibraryElementWithoutData(id);
                        });
                }
                else
                {
                    await _serverClient.FetchLibraryElementData(id);
                    var data = SessionController.Instance.ContentController.GetContent(id).Data;

                    CachePDF.createWriteFile(id, data); //save the data
                }
            }
            else
            {
                await _serverClient.FetchLibraryElementData(id);
            }

        }
        public async Task<HashSet<string>> SearchOverLibraryElements(string searchText)
        {
            return await _serverClient.SearchOverLibraryElements(searchText);
        }

        public async Task<List<SearchResult>> AdvancedSearchOverLibraryElements(Query searchQuery)
        {
            return await _serverClient.AdvancedSearchOverLibraryElements(searchQuery);
        }

        /// <summary>
        /// Basically just to Fetch regions so we dont have to get the entire data
        /// </summary>
        /// <param name="contentIds"></param>
        /// <returns></returns>
        public async Task FetchLibraryElementWithoutData(string contentId)
        {
            await _serverClient.GetContentWithoutData(contentId);
        }

        public async Task<string> DuplicateLibraryElement(string libraryElementId)
        {
            return await _serverClient.DuplicateLibraryElement(libraryElementId);
        }

        /// <summary>
        /// Downloads a docx for the specified library ID and returns the temporary docx file path,
        /// null if an error occurred like the document doesn't exist;
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> DownloadDocx(string id)
        {
            var bytes = await _serverClient.GetDocxBytes(id);
            if (bytes == null)
            {
                return null;
            }
            var path = NuSysStorages.SaveFolder.Path + "\\" + id + ".docx";
            try
            {
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                {
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                }
            }
            catch (UnauthorizedAccessException unAuth)
            {
                throw new UnauthorizedAccessException("Couldn't write to file most likely because it is already open");
            }
            catch (Exception e)
            {
                throw new Exception("couldn't write to file because "+e.Message);
            }
            return path;
        }
        public async Task<Dictionary<string, Dictionary<string, object>>> GetAllLibraryElements()
        {
            return await _serverClient.GetRepo();
        }
        public async Task<bool> AddRegionToContent(string contentId, Region region)
        {
            if (contentId == null || region == null)
            {
                return false;
            }
            return await _serverClient.AddRegionToContent(contentId, region);
        }
        public async Task<bool> RemoveRegionFromContent(Region region)
        {
            if (region == null)
            {
                return false;
            }
            return await _serverClient.RemoveRegionFromContent(region);
        }

        public async Task UpdateRegion(Region region)
        {
            if (region == null ||_regionUpdateDebounceList.Contains(region.Id))
            {
                return;
            }
            _regionUpdateDebounceList.Add(region.Id);
            await Task.Delay(300);
            _regionUpdateDebounceList.Remove(region.Id);
            await _serverClient.UpdateRegion(region);
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
