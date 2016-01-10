using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public sealed partial class MediaRecorderView : UserControl
    {
        private MediaCapture mediaCapture;
        private bool _audioRecording, _videoRecording;
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
            _audioRecording = false;
            _videoRecording = false;
            stream = new InMemoryRandomAccessStream();
            _recordingType = RecordingType.Audio;
        }
        private async void RecordButton_OnTapped(object sender, RoutedEventArgs e)
        {
            if (_recordingType == RecordingType.Audio)
            {
                await OnStartRecordingAudClick(sender, e);
            }
            else
            {
                await OnStartRecordingVidClick(sender, e);
            }
        }
        private async Task OnStartRecordingVidClick(object sender, RoutedEventArgs e)
        {
            if (_audioRecording || _videoRecording)
            {

                await mediaCapture.StopRecordAsync();
                stream.Seek(0);
                byte[] fileBytes = new byte[stream.Size];
                await stream.AsStream().ReadAsync(fileBytes, 0, fileBytes.Length);
                Element.Source = null;
                if (_audioRecording)
                {
                    await SendRequest(fileBytes, NodeType.Audio);
                }
                else
                {
                    await SendRequest(fileBytes, NodeType.Video);
                }
                _videoRecording = false;
                _audioRecording = false;
                mediaCapture.Dispose();
            }
            else
            {
                try
                {
                    mediaCapture = new MediaCapture();
                    var settings = new MediaCaptureInitializationSettings();
                    settings.StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo;
                    await mediaCapture.InitializeAsync(settings);
                    stream = new InMemoryRandomAccessStream();
                    await
                    mediaCapture.StartRecordToStreamAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto),
                        stream);
                    Element.Source = mediaCapture;
                    await mediaCapture.StartPreviewAsync();
                    _videoRecording = true;
                }
                catch (Exception exception)
                {
                    // Do Exception Handling
                }
            }
        }


        private async Task OnStartRecordingAudClick(object sender, RoutedEventArgs e)
        {

            if (_audioRecording || _videoRecording)
            {

                await mediaCapture.StopRecordAsync();
                stream.Seek(0);
                byte[] fileBytes = new byte[stream.Size];
                await stream.AsStream().ReadAsync(fileBytes, 0, fileBytes.Length);
                Element.Source = null;

                await SendRequest(fileBytes, NodeType.Audio);

                _videoRecording = false;
                _audioRecording = false;
                mediaCapture.Dispose();
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
                            MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto),
                            stream);
                    _audioRecording = true;
                }
                catch (Exception exception)
                {
                    // Do Exception Handling
                }
            }
        }

        private async Task SendRequest(byte[] data, NodeType type)
        {
            Message m = new Message();
            var width = SessionController.Instance.SessionView.ActualWidth;
            var height = SessionController.Instance.SessionView.ActualHeight;
            var centerpoint = SessionController.Instance.ActiveWorkspace.CompositeTransform.Inverse.TransformPoint(new Point(width/2, height/2));

            var contentId = SessionController.Instance.GenerateId();

            m["contentId"] = contentId;
            m["x"] = centerpoint.X - 200;
            m["y"] = centerpoint.Y - 200;
            m["width"] = 400;
            m["height"] = 400;
            m["nodeType"] = type.ToString();
            m["autoCreate"] = true;
            m["creators"] = new List<string>() {SessionController.Instance.ActiveWorkspace.Id};
            await
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewContentRequest(contentId,
                    Convert.ToBase64String(data)));

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewNodeRequest(m));
        }

        private async void OnStopRecordingBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                String fileName;
                await mediaCapture.StopRecordAsync();
            }
            catch (Exception exception)
            {
                // Do Exception Handling...
            }
        }


        public WorkspaceView WorkspaceView { get; set; }

    }
}