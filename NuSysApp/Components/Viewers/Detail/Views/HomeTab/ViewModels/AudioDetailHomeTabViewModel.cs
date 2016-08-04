using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;

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

        public override Message GetNewRegionMessage()
        {
            var m = new Message();
            m["start"] = .25;
            m["end"] = .75;
            if (LibraryElementController is AudioRegionLibraryElementController)
            {
                var region = LibraryElementController.LibraryElementModel as AudioRegionModel;
                m["start"] = region.Start + (region.End - region.Start) * 0.25;
                m["end"] = region.Start + (region.End - region.Start) * 0.75;
            }
            return m;
        }
    }
}
