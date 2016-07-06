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

        public delegate void TitleChangedEventHandler(object source, string title);
        public event TitleChangedEventHandler TitleChanged;
        protected HashSet<Region> _regionsToLoad; 
        public HashSet<Region> RegionsToLoad
        {
            get
            {
                return _regionsToLoad;
            }
            set
            {
                _regionsToLoad = value;
                SetExistingRegions();
            }
        }

        public bool Editable { set; get; }
        public DetailHomeTabViewModel(LibraryElementController controller, HashSet<Region> regionsToLoad)
        {

            _libraryElementController = controller;
            controller.TitleChanged += OnTitleChanged;
            controller.RegionAdded += AddRegion;
            controller.RegionRemoved += RemoveRegion;
            //Editable = true;
        }

        private void OnTitleChanged(object source, string title)
        {
            TitleChanged?.Invoke(source,title);
        }
        public virtual async Task Init() { }
    }
}
