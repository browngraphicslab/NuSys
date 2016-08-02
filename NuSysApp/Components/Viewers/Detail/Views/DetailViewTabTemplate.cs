using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class DetailViewTabTemplate : BaseINPC
    {
        private string _title;
        private LibraryElementController _controller;

        public string LibraryElementId { get; private set; }
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        public DetailViewTabTemplate(LibraryElementController controller)
        {
            _controller = controller;
            Title = _controller.Title;
            _controller.TitleChanged += Controller_TitleChanged;
            LibraryElementId = _controller.LibraryElementModel.LibraryElementId;
        }

        private void Controller_TitleChanged(object sender, string newTitle)
        {
            Title = newTitle;
        }

        public void Dispose()
        {
            _controller.TitleChanged -= Controller_TitleChanged;
        }
    }
}
