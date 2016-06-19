using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class VideoDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public LibraryElementController Controller { get; }
        public VideoDetailHomeTabViewModel(LibraryElementController controller) :  base(controller)
        {
            Controller = controller;
        }

        public override void AddRegion(object sender, Region region)
        {
            throw new NotImplementedException();
        }

        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            throw new NotImplementedException();
        }

        public override void SizeChanged(object sender, double width, double height)
        {
            throw new NotImplementedException();
        }
    }
}
