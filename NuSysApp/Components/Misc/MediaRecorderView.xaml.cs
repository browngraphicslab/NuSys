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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media;
using NusysIntermediate;
using NAudio.Wave;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using WinRTXamlToolkit.Imaging;
using Windows.Storage;

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
            var thumbnails = new Dictionary<NusysConstants.ThumbnailSize, string>();
            // instantiate the variables for createNewLibraryElementRequestArgs that depend on the type
            string fileExtension;
            switch (type)
            {
                case NusysConstants.ElementType.Audio:
                    createNewLibraryElementRequestArgs.LibraryElementType = NusysConstants.ElementType.Audio;
                    createNewLibraryElementRequestArgs.Title = "Audio Recording";
                    fileExtension = Constants.RecordingNodeAudioFileType;
                    //Make thumbnails from waveform
                    var frameWorkWaveForm = GetWaveFormFrameWorkElement(data);
                    thumbnails = await GetThumbnailsFromFrameworkElement(frameWorkWaveForm);
                    createNewLibraryElementRequestArgs.Large_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Large];
                    createNewLibraryElementRequestArgs.Small_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Small];
                    createNewLibraryElementRequestArgs.Medium_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Medium];


                    break;
                case NusysConstants.ElementType.Video:
                    createNewLibraryElementRequestArgs.LibraryElementType = NusysConstants.ElementType.Video;
                    createNewLibraryElementRequestArgs.Title = "Video Recording";
                    fileExtension = Constants.RecordingNodeVideoFileType;

                    //TODO: make thumbnail for video
                    //Save file temporarily so we can create a thumbnail.
                    var tempFileName = "recordedvideo" + fileExtension;
                    var storageFile = await NuSysStorages.SaveFolder.CreateFileAsync(tempFileName, CreationCollisionOption.ReplaceExisting);
                    await Windows.Storage.FileIO.WriteBytesAsync(storageFile, data);
                    thumbnails = await MediaUtil.GetThumbnailDictionary(storageFile);
                    createNewLibraryElementRequestArgs.Large_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Large];
                    createNewLibraryElementRequestArgs.Small_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Small];
                    createNewLibraryElementRequestArgs.Medium_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Medium];

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


        /// <summary>
        /// Converts Audio into a
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private FrameworkElement GetWaveFormFrameWorkElement(Byte[] bytes)
        {
            MemoryStream s = new MemoryStream(bytes);
            var stream = s.AsRandomAccessStream();

            WaveStream waveStream = new MediaFoundationReaderUniversal(stream);
            int bytesPerSample = (waveStream.WaveFormat.BitsPerSample / 8) * waveStream.WaveFormat.Channels;
            waveStream.Position = 0;
            int bytesRead = 1;
            int samplesPerPixel = 1024;

            if (waveStream.TotalTime.TotalMinutes > 15)
            {
                samplesPerPixel = 65536;
            }
            else if (waveStream.TotalTime.TotalMinutes > 8)
            {
                samplesPerPixel = 32768;
            }
            else if (waveStream.TotalTime.TotalMinutes > 5)
            {
                samplesPerPixel = 16384;
            }
            else if (waveStream.TotalTime.TotalMinutes > 3)
            {
                samplesPerPixel = 8192;
            }
            else if (waveStream.TotalTime.TotalMinutes > 0.5)
            {
                samplesPerPixel = 2048;
            }

            byte[] waveData = new byte[samplesPerPixel * bytesPerSample];
            var visualGrid = new Grid();
            float x = 0;
            while (bytesRead != 0)
            {
                short low = 0;
                short high = 0;
                bytesRead = waveStream.Read(waveData, 0, samplesPerPixel * bytesPerSample);

                for (int n = 0; n < bytesRead; n += 2)
                {
                    short sample = BitConverter.ToInt16(waveData, n);
                    if (sample < low) low = sample;
                    if (sample > high) high = sample;
                }
                float lowPercent = ((((float)low) - short.MinValue) / ushort.MaxValue);
                float highPercent = ((((float)high) - short.MinValue) / ushort.MaxValue);

                Line line = new Line();
                line.X1 = x;
                line.X2 = x;
                line.Y1 = 100 * (highPercent);
                line.Y2 = 100 * (lowPercent);
                line.Stroke = new SolidColorBrush(Colors.Crimson);
                line.StrokeThickness = 1;
                x++;
                visualGrid.Children.Add(line);

            }
            visualGrid.Height = 100;
            visualGrid.Width = x;
            Line middleLine = new Line();
            middleLine.X1 = 0;
            middleLine.X2 = x;
            middleLine.Y1 = visualGrid.Height / 2;
            middleLine.Y2 = visualGrid.Height / 2;

            middleLine.Stroke = new SolidColorBrush(Colors.Crimson);
            middleLine.StrokeThickness = 1;
            visualGrid.Children.Add(middleLine);

            return visualGrid;
        }

        /// <summary>
        /// Returns the thumbnails from a Framework element
        /// </summary>
        /// <param name="frameWorkElement"></param>
        /// <returns></returns>
        private async Task<Dictionary<NusysConstants.ThumbnailSize, string>> GetThumbnailsFromFrameworkElement(FrameworkElement frameWorkElement)
        {
            // add the ui element to the canvas out of sight
            Canvas.SetTop(frameWorkElement, -frameWorkElement.Height * 2);
            SessionController.Instance.SessionView.MainCanvas.Children.Add(frameWorkElement);

            // render it
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(frameWorkElement, (int)frameWorkElement.Width, (int)frameWorkElement.Height);

            // remove the visual grid from the canvas
            SessionController.Instance.SessionView.MainCanvas.Children.Remove(frameWorkElement);

            // create a buffer from the rendered bitmap
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            byte[] pixels = pixelBuffer.ToArray();

            // create a WriteableBitmap with desired width and height
            var writeableBitmap = new WriteableBitmap(renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight);

            // write the pixels to the bitmap
            using (Stream bitmapStream = writeableBitmap.PixelBuffer.AsStream())
            {
                await bitmapStream.WriteAsync(pixels, 0, pixels.Length);
            }

            // save the writeable bitmap to a file
            var tempFile = await writeableBitmap.SaveAsync(NuSysStorages.SaveFolder);

            // get the thumbnails from the image file
            var thumbnails = await MediaUtil.GetThumbnailDictionary(tempFile);

            // delete the writeable bitmap file that we saved
            await tempFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

            return thumbnails;
        }
    }
}