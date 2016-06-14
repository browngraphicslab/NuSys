using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class AudioDetailHomeTabViewModel
    {
        public LibraryElementController Controller { get; }
        public AudioDetailHomeTabViewModel(LibraryElementController controller)
        {
            Controller = controller;
        }
    }
}
