using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoNodeView : UserControl
    {
        private MediaCapture _mediaCapture;
        private bool _isRecording;
        public VideoNodeView(VideoNodeViewModel vm)
        {
            this.InitializeComponent();
            InitializeCamera();
            this.DataContext = vm;
            InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();
            memoryStream.AsStreamForWrite().Write((vm.Model as VideoNodeModel).ByteArray, 0, (vm.Model as VideoNodeModel).ByteArray.Length);
            memoryStream.Seek(0);
            playbackElement.SetSource(memoryStream, "video/mp4");
            _isRecording = false;
          //  playbackElement.Play();
        }

        private async void InitializeCamera()
        {
            var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Front);
            _mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

            try
            {
                await _mediaCapture.InitializeAsync(settings);
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine("The app was denied access to the camera");
            }
            preview.Source = _mediaCapture;
            await _mediaCapture.StartPreviewAsync();

        }
        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {

            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Get the desired camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            // If there is no device mounted on the desired panel, return the first device found
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }


        private void OnStop_Click(object sender, TappedRoutedEventArgs e)
        {
       /*     if (_recording)
            {
                ToggleRecording(CurrentAudioFile.Name);
            }*/
            playbackElement.Stop();
     //       _stopped = true;
            e.Handled = true;
        }
        private async void OnRecord_Click(object sender, TappedRoutedEventArgs e)
        {
            var vm = (VideoNodeViewModel)this.DataContext;
            var model = (VideoNodeModel)vm.Model;
           
            if (!_isRecording)
            {
                model.Recording = new InMemoryRandomAccessStream();
                var encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);
                await _mediaCapture.StartRecordToStreamAsync(encodingProfile, model.Recording);
                playbackElement.Visibility = Visibility.Collapsed;
                preview.Visibility = Visibility.Visible;

            } else
            {
                await _mediaCapture.StopRecordAsync();
                playbackElement.SetSource(model.Recording, "video/mp4");
                playbackElement.Visibility = Visibility.Visible;
                preview.Visibility = Visibility.Collapsed;
                playbackElement.Play();
            }


            _isRecording = !_isRecording;
        }


        private async void OnPlay_Click(object sender, RoutedEventArgs e)
        {
         /*   if (_recording)
            {
               ToggleRecording(CurrentAudioFile.Name);
            }
            else
            {
               // pause.Opacity = 1;
                play.Opacity = .3;
                if (_stopped)
                {
                    _stopped = false;
                    if (CurrentAudioFile == null) return;
                    var stream = await CurrentAudioFile.OpenAsync(FileAccessMode.Read);
                    playbackElement.SetSource(stream, CurrentAudioFile.FileType);
                }
                playbackElement.MediaEnded += delegate(object o, RoutedEventArgs e2)
                {
                    play.Opacity = 1;
                };*/
                playbackElement.Play();
        }

        private void OnPause_Click(object sender, RoutedEventArgs e)
        {
            playbackElement.Pause();
        //    pause.Opacity = .3;
        }
        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (NodeViewModel) this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }
        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.Remove();
        }
    }
}
