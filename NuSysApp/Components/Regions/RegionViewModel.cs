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
    public abstract class RegionViewModel : BaseINPC
    {
        private bool _editable;

        #region Public variables
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


        #endregion Public variables
        public RegionLibraryElementController RegionLibraryElementController;

        public RegionViewModel(Region model, RegionLibraryElementController regionLibraryElementController)
        {
            Model = model;
            RegionLibraryElementController = regionLibraryElementController;
        }

        public abstract void Dispose(object sender, EventArgs e);

    }
}
