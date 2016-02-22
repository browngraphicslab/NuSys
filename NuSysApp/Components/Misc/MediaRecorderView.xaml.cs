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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public sealed partial class MediaRecorderView : UserControl
    {
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
                RecordText.Visibility = Visibility.Collapsed;
                StopButton.Visibility = Visibility.Visible;
                StopText.Visibility = Visibility.Visible;
            }
            else
            {
                RecordButton.Visibility = Visibility.Visible;
                RecordText.Visibility = Visibility.Visible;
                StopButton.Visibility = Visibility.Collapsed;
                StopText.Visibility = Visibility.Collapsed;
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
                await SendRequest(fileBytes, ElementType.Video);
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
                    await mediaCapture.InitializeAsync(settings);
                    stream = new InMemoryRandomAccessStream();
                    await
                    mediaCapture.StartRecordToStreamAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto),
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

                await SendRequest(fileBytes, ElementType.Audio);

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
                            MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto),
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

        private async Task SendRequest(byte[] data, ElementType type)
        {
            Message m = new Message();
            var width = SessionController.Instance.SessionView.ActualWidth;
            var height = SessionController.Instance.SessionView.ActualHeight;
            var centerpoint = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(new Point(width / 2, height / 2));

            var contentId = SessionController.Instance.GenerateId();

            m["contentId"] = contentId;
            m["x"] = centerpoint.X - 200;
            m["y"] = centerpoint.Y - 200;
            m["width"] = 400;
            m["height"] = 400;
            m["nodeType"] = type.ToString();
            m["autoCreate"] = true;
            m["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;

            if (type == ElementType.Video)
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



            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewNodeRequest(m));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewContentRequest(contentId, Convert.ToBase64String(data), type.ToString()));

            //await SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequest(new NewContentSystemRequest(contentId, Convert.ToBase64String(data)), NetworkClient.PacketType.TCP, null, true);
            this.Hide();
        }

        public void Hide()
        {
            MediaGrid.Visibility = Visibility.Collapsed;
        }

        public FreeFormViewer FreeFormViewer { get; set; }

    }
}