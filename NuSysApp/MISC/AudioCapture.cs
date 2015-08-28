using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Core;
using NuSysApp.MISC;

namespace NuSysApp
{
    public class AudioCapture
    {
        private MediaCapture _mediaCaptureManager;
        private StorageFile _recordStorageFile;
        private bool _recording;
        public static int NumInstances;
        private readonly StorageFolder _rootFolder = NuSysStorages.Media;
       
        private CoreDispatcher _dispatcher;

        public AudioCapture()
        {
            _recording = false;
            NumInstances++;
        }

        public async Task InitializeAudioRecording()
        {
            _mediaCaptureManager = new MediaCapture();
            _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            var settings = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Audio,
                MediaCategory = MediaCategory.Other, // MediaCategory.Speech? 
                AudioProcessing = AudioProcessing.Default
            };
            await _mediaCaptureManager.InitializeAsync(settings);
            _mediaCaptureManager.RecordLimitationExceeded += RecordLimitationExceeded;
            _mediaCaptureManager.Failed += Failed;
            Debug.WriteLine("Device initialised successfully");
        }

        public async void RecordLimitationExceeded(MediaCapture currentCaptureObject)
        {
            try
            {
                if (_recording)
                {
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        try
                        {
                            Debug.WriteLine("Stopping Record on exceeding max record duration");
                            await _mediaCaptureManager.StopRecordAsync();
                            _recording = false;
                            Debug.WriteLine("Stopped record on exceeding max record duration:" + _recordStorageFile.Path);
                        }
                        catch (Exception e)
                        {
                            ShowExceptionMessage(e);
                        }

                    });
                }
            }
            catch (Exception e)
            {
                ShowExceptionMessage(e);
            }
        }

        public void Failed(MediaCapture currentCaptureObject, MediaCaptureFailedEventArgs currentFailure)
        {
            try
            {
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        Debug.WriteLine("Fatal error" + currentFailure.Message);

                    }
                    catch (Exception e)
                    {
                        ShowExceptionMessage(e);
                    }
                });
            }
            catch (Exception e)
            {
                ShowExceptionMessage(e);
            }
        }

        private static void ShowExceptionMessage(Exception e)
        {
            Debug.WriteLine("AUDIO EXCEPTION: " + e.Message);
        }

        public async Task<StorageFile> CaptureAudio(string fileName)
        {
            if (_recording) return null;
            try
            {
                Debug.WriteLine("Starting audio capture");
                _recordStorageFile =
                    await _rootFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                Debug.WriteLine("Created audio file successfully");
                var recordProfile = MediaEncodingProfile.CreateM4a(AudioEncodingQuality.Auto);
                await _mediaCaptureManager.StartRecordToStorageFileAsync(recordProfile, _recordStorageFile);
                Debug.WriteLine("Start audio capture successfully");
                _recording = true;
                return _recordStorageFile;
            }
            catch
            {
                Debug.WriteLine("Failed to capture audio");
                return null;
            }
        }
        /// <summary>
        /// Stop recording and play it back
        /// </summary>
        public async Task StopCapture()
        {
            if (!_recording) return;
            Debug.WriteLine("Stopping audio capture");
            await _mediaCaptureManager.StopRecordAsync();
            Debug.WriteLine("Stop audio capture successful");
            _recording = false;
        }
    }
}
