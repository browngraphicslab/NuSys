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
            var a = new AudioCapture();
        }
    }
}
