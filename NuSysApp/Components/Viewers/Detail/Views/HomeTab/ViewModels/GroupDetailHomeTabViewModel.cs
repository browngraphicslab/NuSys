using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GroupDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public LibraryElementController Controller { get; }

        public LibraryElementModel Model { get; }
        public GroupDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            Controller = controller;
            Model = controller.LibraryElementModel;
        }
    }
}
