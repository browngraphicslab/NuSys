using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class AudioNodeViewModel: NodeViewModel
    {
        public AudioNodeViewModel(AudioNodeModel model) : base(model)
        {
            AudioRecorder = new AudioCapture();
            AudioRecorder.OnAudioStopped += async delegate
            {
                await model.SendNetworkUpdate();
            };
            Width = 400;
            Height = 150;
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
        }

        private async Task Init()
        {
            await AudioRecorder.InitializeAudioRecording();
        }

        public StorageFile CurrentAudioFile
        {
            get { return ((AudioNodeModel)Model).AudioFile; }
            set { ((AudioNodeModel)Model).AudioFile = value; }
        }

        public string FileName
        {
            get { return ((AudioNodeModel)Model).FileName; }
            set { ((AudioNodeModel)Model).FileName = value; }
        }

        public AudioCapture AudioRecorder { get; set; }
    }
}
