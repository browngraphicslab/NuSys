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

        public override CreateNewRegionLibraryElementRequestArgs GetNewCreateLibraryElementRequestArgs()
        {
            var args = new CreateNewTimeSpanRegionRequestArgs();
            args.RegionStart = .25;
            args.RegionEnd = .75;
            if (LibraryElementController is VideoRegionLibraryElementController)
            {
                var region = LibraryElementController.LibraryElementModel as VideoRegionModel;
                args.RegionStart = region.Start + (region.End - region.Start) * 0.25;
                args.RegionEnd = region.Start + (region.End - region.Start) * 0.75;
            }

            return args;
        }
    }
}
