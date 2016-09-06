using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;
using NusysIntermediate;

namespace NuSysApp
{
    public class AudioDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public double Duration { set; get; }
        public LibraryElementController LibraryElementController { get; }

        public AudioDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            LibraryElementController = controller;           
        }

        public override CreateNewLibraryElementRequestArgs GetNewCreateLibraryElementRequestArgs()
        {
            var audioModel = LibraryElementController?.LibraryElementModel as AudioLibraryElementModel;

            Debug.Assert(audioModel != null);

            var args = new CreateNewAudioLibraryElementRequestArgs();
            args.StartTime = audioModel.NormalizedStartTime + (audioModel.NormalizedDuration) * .25;
            args.Duration = audioModel.NormalizedDuration * .5; ;

            return args;
        }
    }
}
