using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class PdfDetailHomeTabViewModel
    {
        public LibraryElementController Controller { get; }
        public LibraryElementModel Model { get; }
        public PdfDetailHomeTabViewModel(LibraryElementController controller)
        {
            Controller = controller;
            Model = controller.LibraryElementModel;
        }
    }
}
