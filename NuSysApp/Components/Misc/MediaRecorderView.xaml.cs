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

            Message m = new Message();
            var width = SessionController.Instance.SessionView.ActualWidth;
            var height = SessionController.Instance.SessionView.ActualHeight;
            var centerpoint = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(new Point(width / 2, height / 2));

            var contentId = SessionController.Instance.GenerateId();

            m["contentId"] = contentId;
            m["x"] = vm.Model.X;
            m["y"] = vm.Model.Y;
            m["width"] = vm.Width;
            m["height"] = vm.Height;
            m["title"] = "";
            m["type"] = type.ToString();
            m["autoCreate"] = true;
            m["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;

            if (type == NusysConstants.ElementType.Video)
            {
                var settings =
                    mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);
                if (settings.Count > 0)
                {
                    var maxX = 0;
                    var maxY = 0;
                    foreach (var settingInst in settings)
                    {
                        if ((settingInst as VideoEncodingProperties).Width > maxX)
                        {
                            maxX = (int) (settingInst as VideoEncodingProperties).Width;
                        }
                        if ((settingInst as VideoEncodingProperties).Height > maxY)
                        {
                            maxY = (int) (settingInst as VideoEncodingProperties).Height;
                        }
                    }
                    m["resolutionX"] = maxX;
                    m["resolutionY"] = maxY;
                }
            }



            // await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, Convert.ToBase64String(data), type.ToString()));
           await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, Convert.ToBase64String(data), type));
           await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));

            // var vm = (TextNodeViewModel) DataContext;
            // await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(vm.ContentId, Convert.ToBase64String(data)));
            RecordingStopped?.Invoke(this);
        }

        public FreeFormViewer FreeFormViewer { get; set; }

    }
}