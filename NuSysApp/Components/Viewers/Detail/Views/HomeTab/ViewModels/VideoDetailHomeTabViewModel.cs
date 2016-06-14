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
    }
}
