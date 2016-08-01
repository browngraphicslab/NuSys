using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public abstract class DetailHomeTabViewModel : Regionable<Region>
    {
        private LibraryElementController _libraryElementController;
        private bool _editable;

        public delegate void TitleChangedEventHandler(object source, string title);
        public event TitleChangedEventHandler TitleChanged;

        public bool Editable
        {
            get { return _editable; }
            set
            {
                _editable = value;
                RaisePropertyChanged("Editable");
            }
        }
        public DetailHomeTabViewModel(LibraryElementController controller)
        {

            _libraryElementController = controller;
            controller.TitleChanged += OnTitleChanged;
            //Editable = true;
        }

        private void OnTitleChanged(object source, string title)
        {
            TitleChanged?.Invoke(source,title);
        }
        public virtual async Task Init() { }

        
    }
}
