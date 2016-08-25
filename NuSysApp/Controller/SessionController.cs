using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using NusysIntermediate;

namespace NuSysApp
{
    public class SessionController
    {
        public delegate void WorkspaceChangedHandler(object source, FreeFormViewerViewModel freeFormViewer);

        public delegate void ModeChangedEventHandler(object source, Options mode);

        /// <summary>
        /// this event will be fired whenever the sessionController enters a new collection via the EnterCollection method.
        /// To be specific, this will be fired before any actual collection fetching/entering occurs.  
        /// The passed string is the LibraryId of the newly entered collection.  
        /// </summary>
        public event EventHandler<string> EnterNewCollectionStarting;

        /// <summary>
        /// Be careful adding to this event, check that the handlers you want to take care can't be taken care of in a mode instance in the free form viewer
        /// </summary>
        public event ModeChangedEventHandler OnModeChanged;

        private static readonly object _syncRoot = new Object();
        private static SessionController _instance = new SessionController();
        private FreeFormViewerViewModel _activeFreeFormViewer;

        private ContentController _contentController = new ContentController();
        private RegionsController _regionsController = new RegionsController();
        private LinksController _linksController = new LinksController();
        private NuSysNetworkSession _nuSysNetworkSession;

        public Dictionary<string, ImageSource> Thumbnails = new Dictionary<string, ImageSource>();

        public double ScreenWidth { get; set; }
        public double ScreenHeight { get; set; }

        private SessionController()
        {
            IdToControllers = new ConcurrentDictionary<string, ElementController>();
            _nuSysNetworkSession = new NuSysNetworkSession();
        }

        public NuSysNetworkSession NuSysNetworkSession
        {
            get { return _nuSysNetworkSession; }
        }
        public string LocalUserID { set; get; }
        public ConcurrentDictionary<string, ElementController> IdToControllers { set; get; }

        public SessionView SessionView { get; set; }

        public ContentController ContentController
        {
            get { return _contentController; }
        }
        
        public RegionsController RegionsController
        {
            get { return _regionsController; }
        }
        public LinksController LinksController
        {
            get { return _linksController; }
        }
        public SpeechRecognizer Recognizer { get; set; }


        public bool IsRecording { get; set; }

        public string SpeechString { get; set; }

        public FreeFormViewerViewModel ActiveFreeFormViewer
        {
            get { return _activeFreeFormViewer; }
            set
           {
                _activeFreeFormViewer = value;
                WorkspaceChanged?.Invoke(this, _activeFreeFormViewer);
            }
        }

        public static SessionController Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new SessionController();
                        }
                    }
                }
                return _instance;
            }
        }



        public event WorkspaceChangedHandler WorkspaceChanged;

        private async Task LoadThumbs()
        {
            Thumbnails.Clear();
            var thumbs = await NuSysStorages.Thumbs.GetFilesAsync();
            foreach (var thumbFile in thumbs)
            {
                if (thumbFile == null)
                    continue;
                var buffer = await FileIO.ReadBufferAsync(thumbFile);
                var id = Path.GetFileNameWithoutExtension(thumbFile.Path);
                var img = await MediaUtil.ByteArrayToBitmapImage(buffer.ToArray());
                Thumbnails[id] = img;
            }
        }

        public async Task SaveThumb(string id, RenderTargetBitmap image)
        {
            Thumbnails[id] = image;
            var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.Thumbs, id + ".png");
            var img = await MediaUtil.RenderTargetBitmapToByteArray(image);
            FileIO.WriteBytesAsync(file, img);
        }

        public async Task SaveThumb(string id, byte[] byteArray)
        {
            var image = await MediaUtil.ByteArrayToBitmapImage(byteArray);
            Thumbnails[id] = image;
            var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.Thumbs, id + ".png");
            FileIO.WriteBytesAsync(file, byteArray);
        }

        //private int _id = 0;
        public string GenerateId()
        {
            //return _id++.ToString();
            return NusysConstants.GenerateId();
        }

        /// <summary>
        /// returns the library element model of the current collection that is in full screen mode;
        /// </summary>
        public LibraryElementModel CurrentCollectionLibraryElementModel
        {
            get { return ContentController.GetLibraryElementModel(ActiveFreeFormViewer.LibraryElementId); }
        }

        /// <summary>
        /// Use this method to switch the mode of the entire workspace.
        /// </summary>
        /// <param name="mode"></param>
        public void SwitchMode(Options mode)
        {
            OnModeChanged?.Invoke(this, mode);
        }

        /// <summary>
        /// adds an element to the its parentCollection and adds its controller to the ID to controller's list.
        /// This also will create the controller class for that model.
        /// This method will fetch the contentDataModel for the element if it doesn't exist locally;
        /// Returns true if the adding was successful;
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> AddElementAsync(ElementModel model)
        {
            var parentLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(model.ParentCollectionId);
            if (parentLibraryElementController == null) //if the parent collection that this node will be in is null
            {
                return false;///could happen naturally if someone adds an public element to a private collection
            }

            if (IdToControllers.ContainsKey(model.Id))
            {
                return false;
            }

            var elementLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(model.LibraryId);
            if (!ContentController.ContainsContentDataModel(elementLibraryElementController.LibraryElementModel.ContentDataModelId))//if the content data model isn't present
            {
                await NuSysNetworkSession.FetchContentDataModelAsync(elementLibraryElementController.LibraryElementModel.ContentDataModelId);//load the content
            }

            var controller = ElementControllerFactory.CreateFromModel(model);


            SessionController.Instance.IdToControllers[model.Id] = controller;

            await UITask.Run(async delegate
            {

                var parentCollectionLibraryElementController = (CollectionLibraryElementController) parentLibraryElementController;
                parentCollectionLibraryElementController.AddChild(model.Id);

                if (model.ElementType == NusysConstants.ElementType.Collection)
                {
                    //TODO have this code somewhere but not stack overflow.  aka: add in a level checker so we don't recursively load 
                    var existingChildren = ((CollectionLibraryElementModel) (controller.LibraryElementModel))?.Children;
                    foreach (var childId in existingChildren ?? new HashSet<string>())
                    {
                        if (SessionController.Instance.IdToControllers.ContainsKey(childId))
                        {
                            ((ElementCollectionController) controller).AddChild(
                                SessionController.Instance.IdToControllers[childId]);
                        }
                    }
                }
            });

            return true;

        }

        #region Speech Recognition

        public async Task InitializeRecog()
        {

            await Task.Run(async () =>
            {
                if (WaitingRoomView.IS_HUB)
                {
                    return;
                }
                Recognizer = new SpeechRecognizer();
                // Compile the dictation grammar that is loaded by default. = ""; 
                await Recognizer.CompileConstraintsAsync();
            });
        }




        public async Task<String> TranscribeVoice()
        {
            string spokenString = "";
            // Create an instance of SpeechRecognizer. 
            // Start recognition. 
            //return;
            try
            {
                Debug.WriteLine("Trying to record!");
                // this.RecordVoice.Click += stopTranscribing;
                IsRecording = true;
                SpeechRecognitionResult speechRecognitionResult = await Recognizer.RecognizeAsync();
                IsRecording = false;
                //  this.RecordVoice.Click -= stopTranscribing;
                // If successful, display the recognition result. 
                if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
                {
                    spokenString =  speechRecognitionResult.Text;
                }
            }
            catch (Exception ex)
            {
                const int privacyPolicyHResult = unchecked((int) 0x80045509);
                const int networkNotAvailable = unchecked((int) 0x80045504);

                if (ex.HResult == privacyPolicyHResult)
                {
                    // User has not accepted the speech privacy policy
                    string error =
                        "In order to use dictation features, we need you to agree to Microsoft's speech privacy policy. To do this, go to your Windows 10 Settings and go to Privacy - Speech, inking, & typing, and enable data collection.";
                    var messageDialog = new Windows.UI.Popups.MessageDialog(error);
                    messageDialog.ShowAsync();
                }
                else if (ex.HResult == networkNotAvailable)
                {
                    string error = "In order to use dictation features, NuSys requires an internet connection";
                    var messageDialog = new Windows.UI.Popups.MessageDialog(error);
                    messageDialog.ShowAsync();
                }
            }
            //_recognizer.Dispose();
            // this.mdTextBox.Text = spokenString;

            Debug.WriteLine(spokenString);

            //var vm = (TextNodeViewModel)DataContext;
            //(vm.Model as TextNodeModel).Text = spokenString;
            SpeechString = spokenString;
            return spokenString;
        }

        public async Task<String> StopTranscribing()
        {
            if (IsRecording)
            {
                await Recognizer.StopRecognitionAsync();
            }
            return SpeechString;

        }

        /// <summary>
        /// method to enter a collection from anywhere.  
        /// The id is the libraryElementId of the collection you want to enter. 
        /// 
        /// THis method will take care of all the clearing and crap for you, just call it with the id you want to use.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task EnterCollection(string collectionLibraryId)
        {
            EnterNewCollectionStarting?.Invoke(this,collectionLibraryId);
            //creates a new request to get the new workspace
            var request = new GetEntireWorkspaceRequest(collectionLibraryId);

            //awaits the requests return after execution
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);

            //gets the element mdoels from the returned requst
            var elementModels = request.GetReturnedElementModels();

            foreach (var controller in SessionController.Instance.IdToControllers.Values)
            {
                controller.Dispose();
            }

            //unload all the content data models by deleting them, and clear the element controllers
            SessionController.Instance.ContentController.ClearAllContentDataModels();
            SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Clear();
            SessionController.Instance.IdToControllers.Clear();//TODO actually unload all three of these.  very important

            //for each returned contentDataMofdel, add it to the session
            request.GetReturnedContentDataModels().ForEach(contentDataModel => SessionController.Instance.ContentController.AddContentDataModel(contentDataModel));

            //TODO put back in for collction entering
            await SessionController.Instance.SessionView.LoadWorkspaceFromServer(collectionLibraryId, elementModels);
        }

        #endregion
    }
}