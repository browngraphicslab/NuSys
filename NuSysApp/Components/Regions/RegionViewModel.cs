using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using NusysIntermediate;

namespace NuSysApp
{
    public class RegionViewModel : BaseINPC
    {
        private bool _editable;

        #region Public variables
        public Sizeable ContainerViewModel;
        public Region Model { get; private set; }
        public bool Editable
        {
            set
            {

                _editable = value;

                RaisePropertyChanged("Editable");
            }
            get
            {
                return _editable;
            }
        }

        public delegate void SizeChangedEventHandler(object sender, double width, double height);
        public event SizeChangedEventHandler ContainerSizeChanged;

        #endregion Public variables
        public RegionLibraryElementController RegionLibraryElementController;

        public RegionViewModel(Region model, RegionLibraryElementController regionLibraryElementController, Sizeable sizeable)
        {
            Model = model;
            ContainerViewModel = sizeable;
            RegionLibraryElementController = regionLibraryElementController;
        }

        public void ChangeSize(object sender, double width, double height)
        {
            ContainerSizeChanged?.Invoke(sender,width,height);
        }
        
    }
}
