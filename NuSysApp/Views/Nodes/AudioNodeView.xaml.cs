using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioNodeView : UserControl
    {
        //private readonly string _rootFilePath = NuSysStorages.Media.Path + @"\";
        private readonly AudioNodeViewModel _anvm;
        private bool _recording;
        private bool _playing;
        public AudioNodeView(AudioNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            _anvm = vm;
            _recording = false;
            _playing = false;
            CurrentAudioFile = null;
            playbackButton.IsEnabled = false;
            recordButton.IsEnabled = true;
            playbackStatus.Text = "no audio saved";
            recordingStatus.Text = "click to record";
        }
        private string FileName => _anvm.FileName;

        private StorageFile CurrentAudioFile
        {
            get { return _anvm.CurrentAudioFile; }
            set { _anvm.CurrentAudioFile = value; }
        }

        private async void RecordButton_Click(object sender, TappedRoutedEventArgs e)
        {
            await ToggleRecording(FileName + ".mp3");
        }
        private async void PlaybackButton_Click(object sender, TappedRoutedEventArgs e)
        {
            await TogglePlayback();
        }

        private async Task ToggleRecording(string fileName)
        {
            if (!_recording)
            {
                nodeTitle.IsEnabled = false;
                playbackButton.IsEnabled = false;
                await _anvm.AudioRecorder.InitializeAudioRecording();
                CurrentAudioFile = await _anvm.AudioRecorder.CaptureAudio(fileName);
                _recording = true;
                recordingStatus.Text = "recording... (click to stop)";
            }
            else
            {
                await _anvm.AudioRecorder.StopCapture();
                _recording = false;
                playbackButton.IsEnabled = true;
                nodeTitle.IsEnabled = true;
                recordingStatus.Text = "click to record";
                playbackStatus.Text = "click to play";
            }
        }

        private async Task TogglePlayback()
        {
            if (CurrentAudioFile == null) return;
            var stream = await CurrentAudioFile.OpenAsync(FileAccessMode.Read);
            playbackElement.SetSource(stream, CurrentAudioFile.FileType);
            if (!_playing)
            {
                nodeTitle.IsEnabled = false;
                recordButton.IsEnabled = false;
                playbackElement.Play();
                _playing = true;
                playbackStatus.Text = "playing...";
            }
            else
            {
                playbackElement.Stop();
                _playing = false;
                playbackStatus.Text = "click to play";
                recordButton.IsEnabled = true;
                nodeTitle.IsEnabled = true;
            }
        }
    }
}
