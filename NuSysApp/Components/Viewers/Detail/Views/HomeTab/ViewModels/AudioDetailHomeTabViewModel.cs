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

        public override CreateNewRegionLibraryElementRequestArgs GetNewCreateLibraryElementRequestArgs()
        {
            var args = new CreateNewTimeSpanRegionRequestArgs();
            args.RegionStart = .25;
            args.RegionEnd = .75;
            if (LibraryElementController is AudioRegionLibraryElementController)
            {
                var region = LibraryElementController.LibraryElementModel as AudioRegionModel;
                args.RegionStart = region.Start + (region.End - region.Start) * 0.25;
                args.RegionEnd = region.Start + (region.End - region.Start) * 0.75;
            }
            return args;
        }
    }
}
