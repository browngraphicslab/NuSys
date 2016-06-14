using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.Components.Viewers.Detail.Views
{
    public class ImageDetailHomeTabViewModel
    {
        public LibraryElementController Controller { get; }
        public LibraryElementModel Model { get; }
        public ImageDetailHomeTabViewModel(LibraryElementController controller)
        {
            Controller = controller;
            Model = controller.LibraryElementModel;
        }
    }
}
