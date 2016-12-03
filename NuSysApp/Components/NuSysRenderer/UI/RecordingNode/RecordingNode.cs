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
        /// Button which when selected starts the recording process.
        /// </summary>
        private ButtonUIElement _recordButton;

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
        /// Icon used to inform the user that we are recording
        /// </summary>
        private CanvasBitmap _stopIcon;

        /// <summary>
        /// Media capture element that actually does the heavy lifting of recording audio and video
        /// </summary>
        private MediaCapture _mediaCapture;
        /// <summary>
        /// Random access stream for use with the mediaCapture
        /// </summary>
        private InMemoryRandomAccessStream _buffer;

        /// <summary>
        /// boolean value that is true if we are recording false otherwise
        /// </summary>
        private bool _recording;

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
            _recordButton = new ButtonUIElement(this, Canvas, new EllipseUIElement(this, Canvas))
            {
                Height = 50,
                Width = 50,
                Background = Colors.Red,
                Bordercolor = Colors.Red,
                SelectedBorder = Colors.LightGray,
                BorderWidth = 5
            };
            _recordButton.ImageBounds = new Rect(_recordButton.Width / 4, _recordButton.Height / 4,
                   _recordButton.Width / 2, _recordButton.Height / 2);
            AddChild(_recordButton);

            // add the currMediaType to display and set default ui values
            _textDisplayOfRecordingType = new TextboxUIElement(this, Canvas)
            {
                Background = Colors.Azure,
                TextColor = Colors.Black,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                TextVerticalAlignment = CanvasVerticalAlignment.Center,
                Height = 25,
                Width = 100
            };
            AddChild(_textDisplayOfRecordingType);

            _mediaTypeSwitch.Tapped += MediaTypeSwitchOnTapped;
            _recordButton.Tapped += RecordStopButtonOnTapped;
        }

        /// <summary>
        /// Fired when the record button is tapped, starts recording and changes the ui
        /// to reflect the current state. If we are already recording, stops the recording
        /// and disposes of the recording node
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private async void RecordStopButtonOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_recording)
            {
                // if we are currently recording
                switch (_currRecordingType)
                {
                    case RecordingType.Audio:
                        await StopRecordingAudio();
                        break;
                    case RecordingType.Video:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                RemoveFromParent();
            }
            else
            {
                // if we are not currently recording
                switch (_currRecordingType)
                {
                    case RecordingType.Audio:
                        StartRecordingAudio();
                        break;
                    case RecordingType.Video:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
        /// Call this when you want to stop recording audio, this will dispose of the media capture, and add the audio recording
        /// as an audio node to the collection below the point where the recording node is currently
        /// </summary>
        private async Task StopRecordingAudio()
        {
            // stop the recording
            await _mediaCapture.StopRecordAsync();
            _recording = false;
            
            // set the ui for the new state now that we are not recording
            SetUIForCurrentState();

            List<string> newLibraryElementIds;

            // read the data from the media capture stream to a buffer
            IRandomAccessStream audio = _buffer.CloneStream();
            Debug.Assert(audio!= null);


            // store the buffer data as a file
            var file = await NuSysStorages.NuSysTempFolder.CreateFileAsync("New Audio Recording.mp3", CreationCollisionOption.ReplaceExisting);
            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await RandomAccessStream.CopyAndCloseAsync(audio.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
                await audio.FlushAsync();
                audio.Dispose();
            }

            await UITask.Run(async() =>
            {
                // add the file to the library, getting the library elmeent controller of the newly added file
                var libController = (await LibraryListUIElement.AddFile(new List<StorageFile> { file })).FirstOrDefault();
                Debug.Assert(libController != null);
                // add the library element to the current collection
                StaticServerCalls.AddElementToCurrentCollection(Transform.LocalPosition, NusysConstants.ElementType.Audio, libController);
            });

            
            // dispose of the mediacapture and buffer and file
            _mediaCapture?.Dispose();
            _buffer?.Dispose();
            file.DeleteAsync();
        }

        /// <summary>
        /// Call this when you want to start recording audio, this will create a media capture and start audio recording
        /// </summary>
        private async void StartRecordingAudio()
        {
            // dispose of the current buffer and media capture if necessary
            _buffer?.Dispose();
            _buffer = new InMemoryRandomAccessStream();
            _mediaCapture?.Dispose();

            try {
                // initialize a new media capture for recording audio
                var settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio
                };
                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync(settings);

                // set a delegate to stop recording when we reach recording limit
                _mediaCapture.RecordLimitationExceeded += sender =>
                {
                    StopRecordingAudio();
                };

                // set a delegate to output information when the media capture fails
                _mediaCapture.Failed += (sender, errorEventArgs) =>
                {
                    _recording = false;
                    throw new Exception(string.Format("Code: {0}. {1}", errorEventArgs.Code, errorEventArgs.Message));
                };
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
            await _mediaCapture.StartRecordToStreamAsync(MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto), _buffer);
            _recording = true;

            // set the ui to reflect that we are currently recording
            SetUIForCurrentState();
        }

        public override void Dispose()
        {
            _mediaTypeSwitch.Tapped -= MediaTypeSwitchOnTapped;
            _recordButton.Tapped -= RecordStopButtonOnTapped;
            _mediaCapture?.Dispose();
            _buffer?.Dispose();
            _audioIcon?.Dispose(); // TODO not sure if disposing of these images is necessary
            _recordIcon?.Dispose();
            _stopIcon?.Dispose();
            _videoIcon?.Dispose();
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

            // set the ui for the _currMediatype, this should already be set in the constructor to audio
            SetUIForCurrentState();
            ArrangeElements();

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
            // put the record button in the bottom of the record ndoe
            _recordButton.Transform.LocalPosition = new Vector2(Width/2 - _recordButton.Width/2, Height - _recordButton.Height - 10);
        }


        /// <summary>
        /// Sets the ui for the current state of the recording node
        /// </summary>
        private void SetUIForCurrentState()
        {

            if (_recording)
            {
                _recordButton.Image = _stopIcon;
            }
            else
            {
                _recordButton.Image = _recordIcon;
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

        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            ArrangeElements();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
