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
using Windows.UI.Xaml;
using Newtonsoft.Json;
using NusysIntermediate;
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
            _serverClient.OnContentUpdated += ContentUpdated;
            _serverClient.PresentationLinkAdded += PresentationLinkAdded;
            _serverClient.PresentationLinkRemoved += PresentationLinkRemoved;
            LockController = new LockController(_serverClient);
        }

        private void PresentationLinkRemoved(object sender, string id1, string id2)
        {
            UITask.Run(delegate
            {
                var presentationLinks =
                    SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                        atom => atom.DataContext is PresentationLinkViewModel);
                List<FrameworkElement> toRemove = new List<FrameworkElement>();
                foreach (var element in presentationLinks)
                {
                    var model = ((PresentationLinkViewModel)element.DataContext).Model;
                    if (model?.InElementId == id1 && model?.OutElementId == id2)
                    {
                        Debug.Assert(PresentationLinkViewModel.Models != null);
                        toRemove.Add(element);
                    }
                }
                foreach (FrameworkElement element in toRemove)
                {
                    var model = ((PresentationLinkViewModel)element.DataContext).Model;
                    PresentationLinkViewModel.Models.Remove(model);
                    ((PresentationLinkViewModel)element.DataContext).FireDisposed(this, EventArgs.Empty);
                }
            });
        }


        private void PresentationLinkAdded(object sender, string id1, string id2)
        {

            if (SessionController.Instance.IdToControllers.ContainsKey(id1) &&
                SessionController.Instance.IdToControllers.ContainsKey(id2))
            {
                UITask.Run(delegate
                {
                    var presentationlink = new PresentationLinkModel();
                    presentationlink.InElementId = id1;
                    presentationlink.OutElementId = id2;
                    var vm = new PresentationLinkViewModel(presentationlink);
                    Debug.Assert(PresentationLinkViewModel.Models != null, "this hashset of presentationlinkmodels should be statically instantiated");

                    // If there exists a presentation link between two element models, return and do not create a new one
                    if (PresentationLinkViewModel.Models.FirstOrDefault(item => item.InElementId == id1 && item.OutElementId == id2) != null ||
                        PresentationLinkViewModel.Models.FirstOrDefault(item => item.OutElementId == id1 && item.InElementId == id2) != null)
                    {
                        return;
                    }

                    // create a new presentation link
                    PresentationLinkViewModel.Models.Add(presentationlink);
                    new PresentationLinkView(vm);
                });
            }
        }

        #region Requests

        public async Task ExecuteRequestLocally(Request request)
        {
            await request.CheckOutgoingRequest();
            var m = new Message(request.GetFinalMessage().GetSerialized());
            await ProcessIncomingRequest(m);
        }
        /// <summary>
        /// Will execute a request and not return from this method until the server has processed the request and returned a confirmation message
        /// the message that is returned is the confirmation message
        /// when parsing the confirmation messsage, please use Constants in NusysIntermediate.NusysConstants instead of strings as the keys you're parsing
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// The message this returns will vary greatly based on the request type sent.  make sure you parse it using constants instead of arbitrary strings
        /// </returns>
        public async Task<Message> ExecuteRequestAsync(Request request)
        {
            return await Task.Run(async delegate
            {
                //if CheckOutgoingRequest created a valid thing
                await request.CheckOutgoingRequest();
                Message message = request.GetFinalMessage();
                var returnMessage = await _serverClient.WaitForRequestRequestAsync(message);
                request.SetReturnMessage(returnMessage);
                return returnMessage;
            });
        }

        /// <summary>
        /// this will simply spin off a new thread and execute the request you sent without waiting for server processing
        /// ONLY USE THIS IN SPECIAL OCCASIONS
        /// </summary>
        /// <param name="request"></param>
        public void ExecuteRequest(Request request)
        {
            Task.Run(async delegate {
                //if CheckOutgoingRequest created a valid thing
                await request.CheckOutgoingRequest();
                Message message = request.GetFinalMessage();
                await _serverClient.SendMessageToServer(message);
            });
        }

        private async void ContentAvailable(Dictionary<string, object> dict)
        {
            if (dict.ContainsKey("id"))
            {
                var id = (string)dict["id"];
                string title = null;
                NusysConstants.ElementType type = NusysConstants.ElementType.Text;
                var metadata = new Dictionary<string, MetadataEntry>();
                if (dict.ContainsKey("title"))
                {
                    title = (string)dict["title"];
                }
                if (dict.ContainsKey("type"))
                {
                    type = (NusysConstants.ElementType)Enum.Parse(typeof(NusysConstants.ElementType), (string)dict["type"], true);
                }
                if (dict.ContainsKey("metadata"))
                {
                    metadata = JsonConvert.DeserializeObject<Dictionary<string, MetadataEntry>>(dict["metadata"].ToString());
                }

                UITask.Run(async delegate {
                    if (SessionController.Instance.ContentController.GetLibraryElementModel(id) != null)
                    {
                        var controller = SessionController.Instance.ContentController.GetLibraryElementController(id);
                        //Debug.Assert(title != null);
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
                            await FetchContentDataModelAsync(id);
                            ServerClient.NeededLibraryDataIDs.Remove(id);
                        });

                    }
                    if (dict.ContainsKey("favorited"))
                    {
                        bool favorited = bool.Parse(dict["favorited"].ToString());
                        var model = SessionController.Instance.ContentController.GetLibraryElementModel(id);
                        if (model != null)
                        {
                            model.Favorited = favorited;
                        }
                    }
                    var message = new Message(dict);
                    SessionController.Instance.ContentController.GetLibraryElementController(id).UnPack(message);
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
            NusysConstants.RequestType requestType;
            if (!message.ContainsKey("request_type"))
            {
                throw new NoRequestTypeException();
            }
            try
            {
                requestType = (NusysConstants.RequestType)Enum.Parse(typeof(NusysConstants.RequestType), message.GetString("request_type"));
            }
            catch (Exception e)
            {
                throw new InvalidRequestTypeException();
            }
            if (requestType == NusysConstants.RequestType.SystemRequest)
            {
                await ProcessIncomingSystemRequest(message);
                return;
            }
            //switch statement used to switch on the element type and create a request from it.
            //NOT ALL REQUEST TYPES SHOULD BE HERE
            //BE CERTAIN THAT IT SHOULD BE HERE BEFORE YOU BLINDLY ADD IT
            switch (requestType)
            {
                case NusysConstants.RequestType.DeleteElementRequest:
                    request = new DeleteElementRequest(message);
                    break;
                case NusysConstants.RequestType.NewElementRequest:
                    request = new NewElementRequest(message);
                    break;
                case NusysConstants.RequestType.NewLinkRequest:
                    request = new NewLinkRequest(message);
                    break;
                case NusysConstants.RequestType.ElementUpdateRequest:
                    request = new ElementUpdateRequest(message);
                    break;
                case NusysConstants.RequestType.FinalizeInkRequest:
                    request = new FinalizeInkRequest(message);
                    break;
                case NusysConstants.RequestType.DuplicateNodeRequest:
                    request = new DuplicateNodeRequest(message);
                    break;
                case NusysConstants.RequestType.UpdateLibraryElementModelRequest:
                    request = new UpdateLibraryElementModelRequest(message);
                    break;
                case NusysConstants.RequestType.SetTagsRequest:
                    request = new SetTagsRequest(message);
                    break;
                case NusysConstants.RequestType.CreateNewLibraryElementRequest:
                    request = new CreateNewLibraryElementRequest(message);
                    break;
                case NusysConstants.RequestType.DeleteLibraryElementRequest:
                    request = new DeleteLibraryElementRequest(message);
                    break;
                case NusysConstants.RequestType.AddInkRequest:
                    request = new AddInkRequest(message);
                    break;
                case NusysConstants.RequestType.RemoveInkRequest:
                    request = new RemoveInkRequest(message);
                    break;
                case NusysConstants.RequestType.ChatRequest:
                    request = new ChatRequest(message);
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

        /// <summary>
        /// This method will send off a GetContentDataModelRequest for the passed in ContentDataModel Id;
        /// It will also add the returned contentDataModel to the contentController for you.
        /// returns whether it was successfully added
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> FetchContentDataModelAsync(string contentDataModelId)
        {
            //if the content data model is present, then it's loaded
            if (SessionController.Instance.ContentController.ContainsContentDataModel(contentDataModelId))
            {
                return true;
            }
            var request = new GetContentDataModelRequest(contentDataModelId);
            await ExecuteRequestAsync(request);
            var model = request.GetReturnedContentDataModel();
            return SessionController.Instance.ContentController.AddContentDataModel(model);
        }
        public async Task<IEnumerable<string>> SearchOverLibraryElements(string searchText)
        {
            return (await _serverClient.AdvancedSearchOverLibraryElements(QueryArgsBuilder.GetQueryArgs(searchText))).Select(q => q.LibraryElementId);
        }

        public async Task<List<SearchResult>> AdvancedSearchOverLibraryElements(QueryArgs searchQuery)
        {
            var request = new SearchRequest(searchQuery);
            await ExecuteRequestAsync(request);
            return request.GetReturnedResults();
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
                throw new Exception("couldn't write to file because " + e.Message);
            }
            return path;
        }
        public async Task<IEnumerable<LibraryElementModel>> GetAllLibraryElements()
        {
            var request = new GetAllLibraryElementsRequest();
            await ExecuteRequestAsync(request);
            var libraryElementModels = request.GetReturnedLibraryElementModels();
            return libraryElementModels;
        }

        /// <summary>
        /// Returns a mapping of regionID to LibraryElement ContentId of its parent
        /// </summary>
        /// <param name="collectionContentId"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetRegionMapping(string collectionContentId)
        {
            return await _serverClient.GetRegionMapping(collectionContentId);
        }

        /// <summary>
        ///   Will add a presentation link to the server.  
        ///   Will return true if successful, false if not
        ///  The id1 and id2 are ElementModel ID's, not LibraryElementModelId's
        ///  The contentId is the collection of the workspace that both of the nodes should be on
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public async Task<bool> AddPresentationLink(string id1, string id2, string contentId)
        {
            return await _serverClient.AddPresentationLink(contentId, id1, id2);
        }
        /// <summary>
        ///  Will remove a presentation link from the server
        ///  Will return true if successful, false if not
        ///  The id1 and id2 are ElementModel ID's, not LibraryElementModelId's
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <returns></returns>
        public async Task<bool> RemovePresentationLink(string id1, string id2)
        {
            return await _serverClient.RemovePresentationLink(id1, id2);
        }
        /// <summary>
        /// Will fetch and return a hashset of presentation links for a given collection
        /// the presentation links ID's will be elementModel ContentId's
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public async Task<HashSet<PresentationLinkModel>> GetPresentationLinks(string contentId)
        {
            return await _serverClient.GetPresentationLinks(contentId);
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
            if (region == null || _regionUpdateDebounceList.Contains(region.LibraryElementId))
            {
                return;
            }
            _regionUpdateDebounceList.Add(region.LibraryElementId);
            await Task.Delay(300);
            _regionUpdateDebounceList.Remove(region.LibraryElementId);
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
