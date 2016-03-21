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

        public Dictionary<string, ImageSource> Thumbnails = new Dictionary<string, ImageSource>();

        private SessionController()
        {
            IdToControllers = new ObservableDictionary<string, ElementController>();
            _nuSysNetworkSession = new NuSysNetworkSession();
        }

        public NuSysNetworkSession NuSysNetworkSession
        {
            get { return _nuSysNetworkSession; }
        }

        public ObservableDictionary<string, ElementController> IdToControllers { set; get; }

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

        public delegate void EnterNewCollectionEventHandler();
        public event EnterNewCollectionEventHandler OnEnterNewCollection;

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
        public async Task SaveWorkspace()
        {
            //TODO: refactor
            /*
            await _contentController.Save();

            var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "workspace.nusys");
            var lineTasks = IdToSendables.Values.Select(async s => await s.Stringify());
            var lines = await Task.WhenAll(lineTasks);
            await FileIO.WriteLinesAsync(file, lines);
            */
        }

        public void FireEnterNewCollection()
        {
            OnEnterNewCollection?.Invoke();
        }

        //private int _id = 0;
        public string GenerateId()
        {
            //return _id++.ToString();
            return Guid.NewGuid().ToString("N");
        }

        #region Speech Recognition

        public async Task InitializeRecog()
        {
            await Task.Run(async () =>
            {
              //  Recognizer = new SpeechRecognizer();
                // Compile the dictation grammar that is loaded by default. = ""; 
             //   await Recognizer.CompileConstraintsAsync();
            });
        }

        public async Task TranscribeVoice()
        {
            string spokenString = "";
            // Create an instance of SpeechRecognizer. 
            // Start recognition. 
            return;
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