using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp2
{
    public class RegionViewModel : BaseINPC
    {
        #region Public variables
        public Sizeable ContainerViewModel;
        public Region Model { get; private set; }


        public delegate void SizeChangedEventHandler(object sender, double width, double height);
        public event SizeChangedEventHandler ContainerSizeChanged;

        #endregion Public variables
        public RegionController LibraryElementController;

        public RegionViewModel(Region model, RegionController controller, Sizeable sizeable)
        {
            Model = model;
            LibraryElementController = controller;
            ContainerViewModel = sizeable;//TODO GET RID OF SIZEABLE
        }

        public void ChangeSize(object sender, double width, double height)
        {
            ContainerSizeChanged?.Invoke(sender,width,height);
        }
        
    }
}
