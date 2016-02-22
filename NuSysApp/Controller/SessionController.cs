using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class SessionController
    {
        public delegate void WorkspaceChangedHandler(object source, FreeFormViewerViewModel freeFormViewer);

        private static readonly object _syncRoot = new Object();
        private static SessionController _instance = new SessionController();
        private FreeFormViewerViewModel _activeFreeFormViewer;

        private ContentController _contentController = new ContentController();

        private NuSysNetworkSession _nuSysNetworkSession;

        public Dictionary<string, List<Tuple<ElementInstanceModel, LoadNodeView>>> LoadingNodeDictionary = new Dictionary<string, List<Tuple<ElementInstanceModel, LoadNodeView>>>();

        public Dictionary<string, ImageSource> Thumbnails = new Dictionary<string, ImageSource>();

        private SessionController()
        {
            IdToSendables = new ObservableDictionary<string, Sendable>();
            _nuSysNetworkSession = new NuSysNetworkSession();
        }

        public NuSysNetworkSession NuSysNetworkSession
        {
            get { return _nuSysNetworkSession; }
        }

        public ObservableDictionary<string, Sendable> IdToSendables { set; get; }

        public SessionView SessionView { get; set; }

        public ContentController ContentController
        {
            get { return _contentController; }
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

        public LibraryView Library
        {
            get { return SessionView.Library; }
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

        public async Task RecursiveCreate(ElementInstanceModel elementInstance)
        {
            await RecursiveCreateInner(elementInstance, new List<ElementInstanceModel>());
            
        }

        private async Task RecursiveCreateInner(ElementInstanceModel elementInstance, List<ElementInstanceModel> addedModels)
        {

            //TODO: refactor
            /*
            if (!String.IsNullOrEmpty(elementInstance.Creator))
            {
                var creatorModel = (NodeContainerModel) IdToSendables[elementInstance.Creator];
                if (!addedModels.Contains(creatorModel))
                {
                    await RecursiveCreateInner(creatorModel, addedModels);
                }
                await creatorModel.AddChild(elementInstance);
                addedModels.Add(elementInstance);
            }
            */
        }


        public void DisposeInq()
        {
            var wvm = (WorkspaceModel) Instance.ActiveFreeFormViewer.Model;
            var cm = (InqCanvasModel) wvm.InqCanvas;
            cm.DisposeInq();
        }

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
                await SendThumbnail(thumbFile, id);
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

        private async Task SendThumbnail(StorageFile storageFile, string id)
        {
            var fileBytes = await MediaUtil.StorageFileToByteArray(storageFile);
            var s = Convert.ToBase64String(fileBytes);
            var request = new NewThumbnailRequest(s, id);
        }

        public async Task SaveWorkspace()
        {
            await _contentController.Save();

            var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "workspace.nusys");
            var lineTasks = IdToSendables.Values.Select(async s => await s.Stringify());
            var lines = await Task.WhenAll(lineTasks);
            await FileIO.WriteLinesAsync(file, lines);
        }

        public async Task LoadWorkspace()
        {
            await LoadThumbs();

            var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "workspace.nusys");
            var lines = await FileIO.ReadLinesAsync(file);
            ;
            await SessionView.LoadWorkspace(lines);
            await _contentController.Load();
        }

        public string GenerateId()
        {
            return Guid.NewGuid().ToString("N");
        }

        #region Speech Recognition

        public async Task InitializeRecog()
        {
            await Task.Run(async () =>
            {
                Recognizer = new SpeechRecognizer();
                // Compile the dictation grammar that is loaded by default. = ""; 
                await Recognizer.CompileConstraintsAsync();
            });
        }

        public async Task TranscribeVoice()
        {
            string spokenString = "";
            // Create an instance of SpeechRecognizer. 
            // Start recognition. 

            try
            {
                // this.RecordVoice.Click += stopTranscribing;
                IsRecording = true;
                SpeechRecognitionResult speechRecognitionResult = await Recognizer.RecognizeAsync();
                IsRecording = false;
                //  this.RecordVoice.Click -= stopTranscribing;
                // If successful, display the recognition result. 
                if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
                {
                    spokenString = speechRecognitionResult.Text;
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
        }

        private async void stopTranscribing(object o, RoutedEventArgs e)
        {
            Recognizer.StopRecognitionAsync();
            IsRecording = false;
            // this.RecordVoice.Click -= stopTranscribing;
        }

        #endregion
    }
}