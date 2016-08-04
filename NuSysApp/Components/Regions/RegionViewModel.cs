﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

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

        public delegate void SizeChangedEventHandler(object sender, double width, double height);
        public event SizeChangedEventHandler ContainerSizeChanged;

        #endregion Public variables
        public RegionLibraryElementController RegionLibraryElementController;

        public RegionViewModel(Region model, RegionLibraryElementController regionLibraryElementController)
        {
            Model = model;
            RegionLibraryElementController = regionLibraryElementController;
        }

        public void ChangeSize(object sender, double width, double height)
        {
            ContainerSizeChanged?.Invoke(sender,width,height);
        }

        public abstract void Dispose(object sender, EventArgs e);

    }
}
