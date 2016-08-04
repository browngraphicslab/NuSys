using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GroupDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public LibraryElementController LibraryElementController { get; }

        public LibraryElementModel Model { get; }
        public GroupDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            LibraryElementController = controller;
            Model = controller.LibraryElementModel;
            
        }

        // not implemented cause we don't have regions in collections
        public override Message GetNewRegionMessage()
        {
            throw new NotImplementedException();
        }
    }
}
