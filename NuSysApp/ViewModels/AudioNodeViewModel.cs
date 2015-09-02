using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class AudioNodeViewModel: NodeViewModel
    {
        public AudioNodeViewModel(AudioModel model, WorkspaceViewModel vm) : base(model, vm)
        {
            AudioRecorder = new AudioCapture();
            AudioRecorder.OnAudioStopped += async delegate
            {
                await model.SendNetworkUpdate();
            };
            this.View = new AudioNodeView(this);
            this.Transform = new MatrixTransform();
            this.Width = 400;
            this.Height = 150;
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            this.NodeType = NodeType.Audio; 
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
