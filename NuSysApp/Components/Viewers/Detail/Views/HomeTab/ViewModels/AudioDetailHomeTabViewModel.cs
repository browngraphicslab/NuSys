using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class AudioDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public LibraryElementController Controller { get; }
        public AudioDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            Controller = controller;
        }
    }
}
