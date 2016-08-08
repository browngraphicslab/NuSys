using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class TextDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public delegate void TextChangedHandler(object source, string text);
        public event TextChangedHandler TextChanged;

        public LibraryElementController LibraryElementController { get; }
        public TextDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            LibraryElementController = controller;
            controller.ContentChanged += ContentChanged;
        }

        private void ContentChanged(object source, string data)
        {
            TextChanged?.Invoke(source,data);
        }

        public override Message GetNewRegionMessage()
        {
            throw new NotImplementedException();
        }
    }
}
