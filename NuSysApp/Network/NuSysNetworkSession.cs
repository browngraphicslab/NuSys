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

        /// <summary>
        /// all of the currently active NetworkMembers on nusys.  
        /// The string key is the UserId, the value is the NetworkUser object
        /// </summary>
        public ConcurrentDictionary<string, NetworkUser> NetworkMembers = new ConcurrentDictionary<string, NetworkUser>();

        /// <summary>
        /// this dictionary maps the UserId to the DisplayName for that user.
        /// This dictionary should track all users, even those who aren't online.  
        /// When displaying Information regarding users, (i.e. the creator of a libraryelement), this dictionary should be used to get the display name.
        /// </summary>
        public Dictionary<string,string> UserIdToDisplayNameDictionary = new Dictionary<string, string>();

        public delegate void NewUserEventHandler(NetworkUser user);
        public event NewUserEventHandler OnNewNetworkUser;

        public delegate void UserDroppedEventHandler(string userId);
        public event UserDroppedEventHandler OnNetworkUserDropped;

        public LockController LockController;
        #endregion Public Members
        #region Private Members

        //private NetworkSession _networkSession;
        private string _hostIP;
        private ServerClient _serverClient;
        private HashSet<string> _regionUpdateDebounceList = new HashSet<string>();
        #endregion Private Members

        public async Task Init()
        {
            _serverClient = new ServerClient();
            await _serverClient.Init();
            _serverClient.OnMessageRecieved += OnMessageRecieved;
            _serverClient.OnNewNotification += HandleNotification;
            LockController = new LockController(_serverClient);

            //asynchronously run a request that will be loading the user ID to display name dictionary 
            Task.Run(async delegate
            {
                var userIdDictionaryRequest = new GetUserIdToDisplayNameDictionaryRequest();
                await ExecuteRequestAsync(userIdDictionaryRequest);
                userIdDictionaryRequest.AddReturnedDictionaryToSession();
            });
        }

        #region Requests

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
        /// DEPRECATED
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

        /// <summary>
        /// the private event handler for notifications.
        /// Whenever the server notifies us, it will be routed here. 
        /// We should switch on the type of notification and handle them accordingly here;
        /// </summary>
        /// <param name="notificationMessag"></param>
        private void HandleNotification(Message notificationMessage)
        {
            Debug.Assert(notificationMessage.ContainsKey(NusysConstants.NOTIFICATION_TYPE_STRING_KEY));

            //get the notification type
            var type = notificationMessage.GetEnum<NusysConstants.NotificationType>(NusysConstants.NOTIFICATION_TYPE_STRING_KEY);

            NotificationHandler handler;
            switch (type)
            {
                case NusysConstants.NotificationType.AddUser:
                    handler = new AddUserNotificationHandler();
                    break;
                case NusysConstants.NotificationType.RemoveUser:
                    handler = new DropUserNotificationHandler();
                    break;
                default:
                    throw new Exception("we don't handle that notification type yet");
            }
            handler.HandleNotification(notificationMessage);
        }

        private async void OnMessageRecieved(Message m)
        {
            await ProcessIncomingRequest(m);
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
                case NusysConstants.RequestType.CreateNewPresentationLinkRequest:
                    request = new CreateNewPresentationLinkRequest(message);
                    break;
                case NusysConstants.RequestType.UpdateContentRequest:
                    request = new UpdateContentRequest(message);
                    break;
                case NusysConstants.RequestType.CreateNewMetadataRequest:
                    request = new CreateNewMetadataRequest(message);
                    break;
                case NusysConstants.RequestType.DeleteMetadataRequest:
                    request = new DeleteMetadataRequest(message);
                    break;
                case NusysConstants.RequestType.UpdateMetadataEntryRequest:
                    request = new UpdateMetadataEntryRequest(message);
                    break;
                default:
                    throw new InvalidRequestTypeException($"The request type, {requestType} could not be found and made into a request instance");
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
        }

        #endregion Requests

        public void FireAddNetworkUser(NetworkUser user)
        {
            OnNewNetworkUser?.Invoke(user);
        }

        public void FireClientDrop(string userId)
        {
            OnNetworkUserDropped?.Invoke(userId);
        }

        /// <summary>
        /// This method will send off a GetContentDataModelRequest for the passed in ContentDataModel Id;
        /// It will also add the returned contentDataModel to the contentController for you.
        /// Will return the local content data model of it already exists locally.
        /// returns the content data model from either the server call or the content controller;
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ContentDataModel> FetchContentDataModelAsync(string contentDataModelId)
        {
            //if the content data model is present, then it's loaded
            if (SessionController.Instance.ContentController.ContainsContentDataModel(contentDataModelId))
            {
                return SessionController.Instance.ContentController.GetContentDataModel(contentDataModelId);//return the already loaded content data model
            }
            var request = new GetContentDataModelRequest(contentDataModelId);//otherwise create a request to fetch the content data model
            await ExecuteRequestAsync(request);
            var model = request.GetReturnedContentDataModel();
            var succesfullAdd = SessionController.Instance.ContentController.AddContentDataModel(model);//add the returned content data model to the session's content
            if (succesfullAdd)
            {
                return model;
            }
            return null;
        }

        /// <summary>
        /// async method used to fetch an anlysis model asynchronously.  
        /// The content data model id is the id of the content data model whose analysis model you wish to fetch.  
        /// Will return null if it doesn't exist on the server.  
        /// As of 8/19/16, anything but image and pdfs will return null;
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <returns></returns>
        public async Task<AnalysisModel> FetchAnalysisModelAsync(string contentDataModelId )
        {
            Debug.Assert(!string.IsNullOrEmpty(contentDataModelId));
            if (SessionController.Instance.ContentController.HasAnalysisModel(contentDataModelId))//if it is already present locally
            {
                return SessionController.Instance.ContentController.GetAnalysisModel(contentDataModelId);//return it
            }
            var request = new GetAnalysisModelRequest(contentDataModelId);//otherwise make a reuqest
            await ExecuteRequestAsync(request);

            var returnedAnalysisModel = request.GetReturnedAnalysisModel();//get the returned analysis model

            SessionController.Instance.ContentController.AddAnalysisModel(returnedAnalysisModel, contentDataModelId);//add the new model to the session controller

            return returnedAnalysisModel;//return it
        }

        public async Task<IEnumerable<string>> SearchOverLibraryElements(string searchText)
        {
            return (await AdvancedSearchOverLibraryElements(QueryArgsBuilder.GetQueryArgs(searchText))).Select(q => q.LibraryElementId);
        }

        public async Task<List<SearchResult>> AdvancedSearchOverLibraryElements(QueryArgs searchQuery)
        {
            var request = new SearchRequest(searchQuery);
            await ExecuteRequestAsync(request);
            return request.GetReturnedResults();
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
        /// Get the DisplayName from the user id. 
        /// Returns null if the userId is not in the mappoing
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetDisplayNameFromUserId(string userId)
        {
            Debug.Assert(userId != null, "You probably don't want to be sending null ids");
            if (UserIdToDisplayNameDictionary.ContainsKey(userId))
            {
                return UserIdToDisplayNameDictionary[userId];
            }
            Debug.Fail("The userId should always exist");
            return null;
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
