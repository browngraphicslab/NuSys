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

        public override Message GetNewRegionMessage()
        {
            var m = new Message();
            m["rectangle_location"] = new Point(.25, .25);
            m["rectangle_width"] = .5;
            m["rectangle_height"] = .5;
            m["start"] = .25;
            m["end"] = .75;
            if (LibraryElementController is VideoRegionLibraryElementController)
            {
                var region = LibraryElementController.LibraryElementModel as VideoRegionModel;
                m["start"] = region.Start + (region.End - region.Start) * 0.25;
                m["end"] = region.Start + (region.End - region.Start) * 0.75;
            }

            return m;
        }
    }
}
