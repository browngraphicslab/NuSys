using Windows.Storage;

namespace NuSysApp
{
    public class AudioNodeModel : Node
    {
        public AudioNodeModel(int id) : base(id)
        {
            ID = id;
            FileName = "AudioCapture" + AudioCapture.NumInstances;
        }
        public StorageFile AudioFile { get; set; }
        public string FileName { get; set; }
    }
}
