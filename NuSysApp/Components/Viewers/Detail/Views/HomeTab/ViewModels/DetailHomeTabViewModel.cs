using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class DetailHomeTabViewModel
    {
        private LibraryElementController _libraryElementController;

        public delegate void TitleChangedEventHandler(object source, string title);
        public event TitleChangedEventHandler TitleChanged;
        public DetailHomeTabViewModel(LibraryElementController controller)
        {
            _libraryElementController = controller;
            controller.TitleChanged += OnTitleChanged;
        }

        private void OnTitleChanged(object source, string title)
        {
            TitleChanged?.Invoke(source,title);
        }
    }
}
