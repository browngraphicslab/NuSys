using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public sealed partial class MediaRecorderView : UserControl
    {
        public delegate void RecordingActionHandler(object source);

        public event RecordingActionHandler RecordingStopped;

        private MediaCapture mediaCapture;
        private bool _recording;
        private InMemoryRandomAccessStream stream;
        private RecordingType _recordingType;

        public enum RecordingType
        {
            Video,
            Audio
        }
        public MediaRecorderView()
        {
            this.InitializeComponent();
            _recording = false;
            stream = new InMemoryRandomAccessStream();
            _recordingType = RecordingType.Audio;
        }
        private async void RecordButton_OnTapped(object sender, RoutedEventArgs e)
        {
            AudioVideoSwitch.IsHitTestVisible = !AudioVideoSwitch.IsHitTestVisible;
            if (AudioVideoSwitch.IsOn)
            {
                await OnStartRecordingVidClick();
            }
            else
            {
                await OnStartRecordingAudClick();
            }
            AudioVideoSwitch.Visibility = Visibility.Collapsed;
        }

        public void Show()
        {
            MediaGrid.Visibility = Visibility.Visible;
        }

        private void IsRecordingSwitch(bool boolean)
        {
            if (boolean)
            {
                RecordButton.Visibility = Visibility.Collapsed;
                StopButton.Visibility = Visibility.Visible;
            }
            else
            {
                RecordButton.Visibility = Visibility.Visible;
                StopButton.Visibility = Visibility.Collapsed;
            }
        }

        private async Task OnStartRecordingVidClick()
        {
            if (_recording)
            {
                await mediaCapture.StopRecordAsync();
                stream.Seek(0);
                byte[] fileBytes = new byte[stream.Size];
                await stream.AsStream().ReadAsync(fileBytes, 0, fileBytes.Length);
                Element.Source = null;
                await SendRequest(fileBytes, NusysConstants.ElementType.Video);
                _recording = false;
                mediaCapture.Dispose();
                this.IsRecordingSwitch(false);
            }
            else
            {
                try
                {
                    mediaCapture = new MediaCapture();
                    var settings = new MediaCaptureInitializationSettings();
                    settings.StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo;
                   //var prop = mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoRecord);
                    await mediaCapture.InitializeAsync(settings);
                    stream = new InMemoryRandomAccessStream();
                    await
                    mediaCapture.StartRecordToStreamAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Qvga),
                        stream);
                    Element.Source = mediaCapture;
                    await mediaCapture.StartPreviewAsync();
                    _recording = true;
                    this.IsRecordingSwitch(true);
                }
                catch (Exception exception)
                {
                    // Do Exception Handling
                }
            }
        }



        private async Task OnStartRecordingAudClick()
        {

            if (_recording)
            {

                await mediaCapture.StopRecordAsync();
                stream.Seek(0);
                byte[] fileBytes = new byte[stream.Size];
                await stream.AsStream().ReadAsync(fileBytes, 0, fileBytes.Length);
                Element.Source = null;

                await SendRequest(fileBytes, NusysConstants.ElementType.Audio);

                _recording = false;

                mediaCapture.Dispose();
                this.IsRecordingSwitch(false);
            }
            else
            {
                try
                {
                    mediaCapture = new MediaCapture();
                    var settings = new MediaCaptureInitializationSettings();
                    settings.StreamingCaptureMode = StreamingCaptureMode.Audio;
                    await mediaCapture.InitializeAsync(settings);
                    stream = new InMemoryRandomAccessStream();
                    await
                        mediaCapture.StartRecordToStreamAsync(
                            MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Low),
                            stream);
                    _recording = true;
                    this.IsRecordingSwitch(true);
                }
                catch (Exception exception)
                {
                    // Do Exception Handling
                }
            }
        }

        private async Task SendRequest(byte[] data, NusysConstants.ElementType type)
        {
            var vm = (RecordingNodeViewModel) DataContext;

            // instantiate the common variables for createNewLibraryElementRequestArgs
            var createNewLibraryElementRequestArgs = new CreateNewLibraryElementRequestArgs();
            createNewLibraryElementRequestArgs.ContentId = SessionController.Instance.GenerateId();
            // generated because we want to add this element to the collection using this later
            createNewLibraryElementRequestArgs.LibraryElementId = SessionController.Instance.GenerateId(); 

            // instantiate the variables for createNewLibraryElementRequestArgs that depend on the type
            string fileExtension;
            switch (type)
            {
                case NusysConstants.ElementType.Audio:
                    createNewLibraryElementRequestArgs.LibraryElementType = NusysConstants.ElementType.Audio;
                    createNewLibraryElementRequestArgs.Title = "Audio Recording";
                    fileExtension = Constants.RecordingNodeAudioFileType;
                    break;
                case NusysConstants.ElementType.Video:
                    createNewLibraryElementRequestArgs.LibraryElementType = NusysConstants.ElementType.Video;
                    createNewLibraryElementRequestArgs.Title = "Video Recording";
                    fileExtension = Constants.RecordingNodeVideoFileType;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Recording nodes do not support the given type yet");
            }

            // create a new content request args because the recording node creates a new instance of content
            var createNewContentRequestArgs = new CreateNewContentRequestArgs();
            createNewContentRequestArgs.LibraryElementArgs = createNewLibraryElementRequestArgs;
            createNewContentRequestArgs.DataBytes = Convert.ToBase64String(data);
            createNewContentRequestArgs.FileExtension = fileExtension;

            // execute the request
            var request = new CreateNewContentRequest(createNewContentRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddReturnedLibraryElementToLibrary();

            // try to get the library element controller from the library element id we assigned to it in the createNewLibraryElementRequestArgs
            var libraryElementController =
                SessionController.Instance.ContentController.GetLibraryElementController(
                    createNewLibraryElementRequestArgs.LibraryElementId);

            // if the libraryElementController exists then add it to the workspace at the view models position
            if (libraryElementController != null)
            {
                UITask.Run(() =>
                {
                    libraryElementController.AddElementAtPosition(vm.X, vm.Y);
                });
            }

            RecordingStopped?.Invoke(this);
        }

        public FreeFormViewer FreeFormViewer { get; set; }
    }
}