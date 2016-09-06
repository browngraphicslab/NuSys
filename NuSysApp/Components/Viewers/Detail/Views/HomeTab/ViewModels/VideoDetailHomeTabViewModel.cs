using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using NusysIntermediate;

namespace NuSysApp
{
    public class VideoDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public LibraryElementController LibraryElementController { get; }
        
        public VideoDetailHomeTabViewModel(LibraryElementController controller) :  base(controller)
        {
            LibraryElementController = controller;
        }

        public override CreateNewLibraryElementRequestArgs GetNewCreateLibraryElementRequestArgs()
        {
            var videoModel = LibraryElementController?.LibraryElementModel as VideoLibraryElementModel;

            Debug.Assert(videoModel != null);

            var args = new CreateNewVideoLibraryElementRequestArgs();
            args.StartTime = videoModel.NormalizedStartTime + videoModel.NormalizedDuration * .25;
            args.Duration = videoModel.NormalizedDuration * .5; ;

            return args;
        }
    }
}
