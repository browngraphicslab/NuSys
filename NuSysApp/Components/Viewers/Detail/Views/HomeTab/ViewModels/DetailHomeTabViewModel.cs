using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public abstract class DetailHomeTabViewModel : BaseINPC
    {
        private bool _editable;

        public delegate void TitleChangedEventHandler(object source, string title);
        public event TitleChangedEventHandler TitleChanged;

        public LibraryElementController LibraryElementController { get; }

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

            LibraryElementController = controller;
            controller.TitleChanged += OnTitleChanged;
        }

        private void OnTitleChanged(object source, string title)
        {
            TitleChanged?.Invoke(source,title);
        }
        public virtual async Task Init() { }
        
        public abstract CreateNewRegionLibraryElementRequestArgs GetNewCreateLibraryElementRequestArgs();



    }
}
