using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;

namespace NuSysApp
{
    public class RecordingNode : DraggableWindowUIElement
    {
        /// <summary>
        /// Button which when selected changes the media type the recording node is currently set up to record
        /// </summary>
        private ButtonUIElement _mediaTypeSwitch;

        /// <summary>
        /// An enum of the media types the recording node supports
        /// </summary>
        private enum RecordingType
        {
            Audio,
            Video
        }

        /// <summary>
        /// The current media type the recording node is set up to record
        /// </summary>
        private RecordingType _currRecordingType;

        /// <summary>
        /// Button which when selected starts the recording process or pauses it
        /// </summary>
        private ButtonUIElement _recordPauseButton;

        /// <summary>
        /// Button which when selected stops the recording process
        /// </summary>
        private ButtonUIElement _stopButton;

        /// <summary>
        /// Text displaying what type of media the recording node is currently set up to record
        /// </summary>
        private TextboxUIElement _textDisplayOfRecordingType;

        /// <summary>
        /// Icon used to inform the user that we are going to record audio
        /// </summary>
        private CanvasBitmap _audioIcon;

        /// <summary>
        /// Icon used to inform the uesr that we are going to record video
        /// </summary>
        private CanvasBitmap _videoIcon;

        /// <summary>
        /// Icone used to inform the user that we are not recording
        /// </summary>
        private CanvasBitmap _recordIcon;

        /// <summary>
        /// Icon used to let the user pause the recording
        /// </summary>
        private CanvasBitmap _pauseIcon;

        /// <summary>
        /// Icon used to inform the user that we are recording
        /// </summary>
        private CanvasBitmap _stopIcon;

        /// <summary>
        /// Media capture element that actually does the heavy lifting of recording audio and video
        /// </summary>
        private MediaCapture _mediaCapture;

        /// <summary>
        /// Low lag media recording used for recording media to a file
        /// </summary>
        LowLagMediaRecording _mediaRecording;

        /// <summary>
        /// boolean value that is true if we are recording false otherwise, true if we are recording and paused.
        /// </summary>
        private bool _recording;

        /// <summary>
        /// boolean value that is true if we are recording and paused, false otherwise
        /// </summary>
        private bool _paused;

        /// <summary>
        /// File we are going to use to store our recorded data
        /// </summary>
        private StorageFile _file;
        

        public RecordingNode(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
            : base(parent, resourceCreator)
        {
            // set default ui values
            Background = Colors.Azure;
            Height = 400;
            Width = 400;
            BorderWidth = 3;
            Bordercolor = Colors.CadetBlue;
            TopBarColor = Colors.CadetBlue;

            // set default starting media type to audio
            _currRecordingType = RecordingType.Audio;

            // add the media type switch and set default ui values
            _mediaTypeSwitch = new ButtonUIElement(this, Canvas, new EllipseUIElement(this, Canvas))
            {
                Height = 50,
                Width = 50,
                Background = Colors.CadetBlue,
                Bordercolor = Colors.CadetBlue,
                SelectedBorder = Colors.LightGray,
                BorderWidth = 5,
            };
            _mediaTypeSwitch.ImageBounds = new Rect(_mediaTypeSwitch.Width/4, _mediaTypeSwitch.Height/4,
                _mediaTypeSwitch.Width/2, _mediaTypeSwitch.Height/2);
            AddChild(_mediaTypeSwitch);

            // add the record button and set default ui values
            _recordPauseButton = new ButtonUIElement(this, Canvas, new EllipseUIElement(this, Canvas))
            {
                Height = 50,
                Width = 50,
                Background = Colors.Red,
                Bordercolor = Colors.Red,
                SelectedBorder = Colors.LightGray,
                BorderWidth = 5
            };
            _recordPauseButton.ImageBounds = new Rect(_recordPauseButton.Width / 4, _recordPauseButton.Height / 4,
                   _recordPauseButton.Width / 2, _recordPauseButton.Height / 2);
            AddChild(_recordPauseButton);

            _stopButton = new ButtonUIElement(this, Canvas, new EllipseUIElement(this, Canvas))
            {
                Height = 50,
                Width = 50,
                Background = Colors.Red,
                Bordercolor = Colors.Red,
                SelectedBorder = Colors.LightGray,
                BorderWidth = 5
            };
            _stopButton.ImageBounds = new Rect(_stopButton.Width / 4, _stopButton.Height / 4,
                _stopButton.Width / 2, _stopButton.Height / 2);
            AddChild(_stopButton);

            // add the currMediaType to display and set default ui values
            _textDisplayOfRecordingType = new TextboxUIElement(this, Canvas)
            {
                Background = Colors.Azure,
                TextColor = Colors.Black,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                TextVerticalAlignment = CanvasVerticalAlignment.Center,
                Height = 25,
                Width = 250
            };
            AddChild(_textDisplayOfRecordingType);

            _mediaTypeSwitch.Tapped += MediaTypeSwitchOnTapped;
            _recordPauseButton.Tapped += Record_Pause_buttonOnTapped;
            _stopButton.Tapped += StopButtonOnTapped;
        }

        /// <summary>
        /// Called when the stop button is tapped, stops whatever recording is being done
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void StopButtonOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            StopRecording();
        }

        /// <summary>
        /// Cleans up and stops an audio recording, adding the recording audio to the collection
        /// </summary>
        private async void StopRecording()
        {
            await _mediaRecording.StopAsync();
            await _mediaRecording.FinishAsync();

            await UITask.Run(async () =>
            {
                // add the file to the library, getting the library elmeent controller of the newly added file
                var libController = (await LibraryListUIElement.AddFile(new List<StorageFile> {_file})).FirstOrDefault();
                Debug.Assert(libController != null);
                // add the library element to the current collection
                StaticServerCalls.AddElementToCurrentCollection(Transform.LocalPosition,
                    _currRecordingType == RecordingType.Audio
                        ? NusysConstants.ElementType.Audio
                        : NusysConstants.ElementType.Video, libController);
            });

            RemoveFromParent();
        }

        /// <summary>
        /// Starts recording if we are not recording, pauses or resumes recording if we are recording
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private async void Record_Pause_buttonOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_recording)
            {
                // if paused, then unpause, otherwise pause the recording
                if (_paused)
                {
                    ResumeRecording();
                }
                else
                {
                    PauseRecording();
                }
            }
            else
            {
                // if we are not currently recording
                StartRecording();
            }
        }


        /// <summary>
        /// Removes the recording node from the parent which fires disposed automatically
        /// </summary>
        private void RemoveFromParent()
        {
            Parent.RemoveChild(this);
        }

        /// <summary>
        /// Resume recording if we were paused
        /// </summary>
        private async void ResumeRecording()
        {
            // resume the recording
            await _mediaRecording.ResumeAsync();
            _paused = false;

            // set the ui for the new state now that we are no longer paused
            SetUIForCurrentState();
        }

        /// <summary>
        /// Pauses recording if we were currently recording
        /// </summary>
        private async void PauseRecording()
        {
            // pause the recording
            await _mediaRecording.PauseAsync(MediaCapturePauseBehavior.RetainHardwareResources);
            _paused = true;

            // set the ui for the new state now that we are paused
            SetUIForCurrentState();
        }

        /// <summary>
        /// Starts recording audio or video setting up everthing safely
        /// </summary>
        private async void StartRecording()
        {
            // dispose of the current _mediaCapture if necessary
            _mediaCapture?.Dispose();


            try
            {
                // initialize a new media capture for recording audio
                var settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = _currRecordingType == RecordingType.Audio ? StreamingCaptureMode.Audio : StreamingCaptureMode.AudioAndVideo
                };
                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync(settings);

                // set a method to stop recording when we reach recording limit
                _mediaCapture.RecordLimitationExceeded += OnMediaCaptureRecordLimitationExceeded;

                // set a delegate to output information when the media capture fails
                _mediaCapture.Failed += (sender, errorEventArgs) =>
                {
                    _recording = false;
                    throw new Exception(string.Format("Code: {0}. {1}", errorEventArgs.Code, errorEventArgs.Message));
                };

                // create a file we are goign to record into
                var fileName = _currRecordingType == RecordingType.Audio
                    ? "New Audio Recording.mp3"
                    : "New Video Recording.mp4";
                _file = await NuSysStorages.NuSysTempFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                // prepare a low lag media recording object to record to the file
                var profile = _currRecordingType == RecordingType.Audio
                    ? MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High)
                        : MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);
                _mediaRecording = await _mediaCapture.PrepareLowLagRecordToStorageFileAsync(profile, _file);
            }
            catch (Exception ex)
            {
                // catch exceptions
                if (ex.InnerException != null && ex.InnerException.GetType() == typeof(UnauthorizedAccessException))
                {
                    throw ex.InnerException;
                }
                throw;
            }

            // start recording to the stream
            await _mediaRecording.StartAsync();
            _recording = true;

            // set the ui to reflect that we are currently recording
            SetUIForCurrentState();
        }

        /// <summary>
        /// Stop recording audio when we have exceeded the record limitation
        /// </summary>
        /// <param name="sender"></param>
        private void OnMediaCaptureRecordLimitationExceeded(MediaCapture sender)
        {
            StopRecording();
        }

        public override void Dispose()
        {
            _mediaTypeSwitch.Tapped -= MediaTypeSwitchOnTapped;
            _recordPauseButton.Tapped -= Record_Pause_buttonOnTapped;
            _stopButton.Tapped -= StopButtonOnTapped;
            _mediaCapture.RecordLimitationExceeded -= OnMediaCaptureRecordLimitationExceeded;
            _mediaCapture?.Dispose();
            _audioIcon?.Dispose(); // TODO not sure if disposing of these images is necessary
            _recordIcon?.Dispose();
            _stopIcon?.Dispose();
            _videoIcon?.Dispose();
            _file?.DeleteAsync();
            base.Dispose();
        }

        /// <summary>
        /// Fired when the media type switch is tapped, changes the ui to reflect the new media type
        /// that is going to be recorded
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void MediaTypeSwitchOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            switch (_currRecordingType)
            {
                case RecordingType.Audio:
                    _currRecordingType = RecordingType.Video;
                    break;
                case RecordingType.Video:
                    _currRecordingType = RecordingType.Audio;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SetUIForCurrentState();
        }

        /// <summary>
        /// Get any resources that we have to load async
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            // load all the images async
            _recordIcon = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/icon_audionode_record.png"));
            _stopIcon = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/icon_audionode_stop.png"));
            _videoIcon = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/node icons/icon_video.png"));
            _audioIcon = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/node icons/record.png"));
            _pauseIcon = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/icon_audionode_pause.png"));

            // set the ui for the _currMediatype, this should already be set in the constructor to audio
            SetUIForCurrentState();

            base.Load();
        }

        /// <summary>
        /// Arranges all the elements on the recording node properly. called first after load, since some resources have to be loaded async
        /// </summary>
        private void ArrangeElements()
        {
            _mediaTypeSwitch.Transform.LocalPosition = new Vector2(Width/2 - _mediaTypeSwitch.Width/2, Height/2 - _mediaTypeSwitch.Height/2);
            // put the _currMediaTypeDisplay just below the center of the record node
            _textDisplayOfRecordingType.Transform.LocalPosition = new Vector2(Width/2 - _textDisplayOfRecordingType.Width/2, Height/2 - _textDisplayOfRecordingType.Height/2 + _mediaTypeSwitch.Height/2 + 20);

            // put the record button in the bottom of the record node
            _recordPauseButton.Transform.LocalPosition = new Vector2(Width/2 - _recordPauseButton.Width/2, Height - _recordPauseButton.Height - 10);
            // put the stop button in the bottom of the record node
            _stopButton.Transform.LocalPosition = new Vector2(Width/2 - _stopButton.Width/2, Height - _stopButton.Height - 10);

            // if we are currently recording shift the stop and record buttons so they are next to eachother
            if (_recording)
            {
                _stopButton.Transform.LocalPosition -= new Vector2(_stopButton.Width, 0);
                _recordPauseButton.Transform.LocalPosition += new Vector2(_recordPauseButton.Width, 0);
            }
        }


        /// <summary>
        /// Sets the ui for the current state of the recording node
        /// </summary>
        private void SetUIForCurrentState()
        {
            if (_recording)
            {
                _stopButton.IsVisible = true;
                _recordPauseButton.Image = _paused ? _recordIcon : _pauseIcon;
                _stopButton.Image = _stopIcon;
                _textDisplayOfRecordingType.Text = _paused ? "Press Record to Continue, Stop to Finish" : "Press Pause to Pause, Stop to Finish";
            }
            else
            {
                _stopButton.IsVisible = false;
                _recordPauseButton.Image = _recordIcon;
                switch (_currRecordingType)
                {
                    case RecordingType.Audio:
                        _mediaTypeSwitch.Image = _audioIcon;
                        _textDisplayOfRecordingType.Text = "Record Audio";
                        break;
                    case RecordingType.Video:
                        _mediaTypeSwitch.Image = _videoIcon;
                        _textDisplayOfRecordingType.Text = "Record Video";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_currRecordingType), _currRecordingType, null); // we don't support recording for that media type yet
                }
            }

            // arrange the elements for the current state
            ArrangeElements();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            ArrangeElements();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
