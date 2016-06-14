using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class TextDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public delegate void TextChangedHandler(object source, string text);
        public event TextChangedHandler TextChanged;

        public LibraryElementController Controller { get; }
        public TextDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            Controller = controller;
            controller.ContentChanged += ContentChanged;
        }

        private void ContentChanged(object source, string data)
        {
            TextChanged?.Invoke(source,data);
        }

    }
}
