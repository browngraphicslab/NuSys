using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class AudioNodeViewModel: NodeViewModel
    {     
        public AudioNodeViewModel(AudioModel model, WorkspaceViewModel vm, string id) : base(model, vm, id)
        {
            AudioRecorder = new AudioCapture();
            Init();
        }

        private async Task Init()
        {
            await AudioRecorder.InitializeAudioRecording();
        }

        public StorageFile CurrentAudioFile
        {
            get { return ((AudioModel)Model).AudioFile; }
            set { ((AudioModel)Model).AudioFile = value; }
        }

        public string FileName
        {
            get { return ((AudioModel)Model).FileName; }
            set { ((AudioModel)Model).FileName = value; }
        }

        public AudioCapture AudioRecorder { get; set; }
    }
}
