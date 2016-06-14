using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.Components.Viewers.Detail.Views.HomeTab.ViewModels
{
    public class TextDetailHomeTabViewModel
    {
        internal Action<object, string> TextBindingChanged;

        public LibraryElementController Controller { get; }
        public LibraryElementModel Model { get; }
        public TextDetailHomeTabViewModel(LibraryElementController controller)
        {
            Controller = controller;
            Model = controller.LibraryElementModel;
        }

    }
}
