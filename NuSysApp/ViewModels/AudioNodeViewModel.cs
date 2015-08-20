using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class AudioNodeViewModel : NodeViewModel
    {
        public int AudioNodeId { get; set; }
        public AudioCapture AudioRecorder = new AudioCapture();
        public AudioNodeViewModel(WorkspaceViewModel workspaceViewModel, int id) : base(workspaceViewModel, id)
        {
            AudioNodeId = id;
            this.Model = new AudioNodeModel(id);
            this.View = new AudioNodeView(this);
            this.Transform = new MatrixTransform();
            this.IsSelected = false;
            this.Width = Constants.DefaultNodeSize;
            this.Height = Constants.DefaultNodeSize;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 255, 235, 205));
            this.NodeType = Constants.NodeType.Audio;
        }

        public async Task InitializeAudioNode()
        {
            await AudioRecorder.InitializeAudioRecording();
        }

        public StorageFile CurrentAudioFile
        {
            get { return ((AudioNodeModel)Model).AudioFile; }
            set { ((AudioNodeModel) Model).AudioFile = value; }
        }

        public string FileName
        {
            get { return ((AudioNodeModel) Model).FileName; }
            set { ((AudioNodeModel) Model).FileName = value; }
        }

        //public PdfNodeModel PdfNodeModel //TO DO: GET RID OF THIS PROPERTY. WHY DO WE HAVE TWO MODEL PROPERTIES?!!
        //{
        //    get { return (PdfNodeModel)Model; }
        //    set
        //    {
        //        if ((PdfNodeModel)Model == value)
        //        {
        //            return;//
        //        }
        //        this.Model = value;
        //        RaisePropertyChanged("PdfNodeModel");
        //    }
        //}
    }
}
