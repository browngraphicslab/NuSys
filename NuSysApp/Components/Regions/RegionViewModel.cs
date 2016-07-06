using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class RegionViewModel : BaseINPC
    {
        #region Public variables
        public Sizeable ContainerViewModel;
        public Region Model { get; private set; }


        public delegate void SizeChangedEventHandler(object sender, double width, double height);
        public event SizeChangedEventHandler ContainerSizeChanged;

        #endregion Public variables
        public LibraryElementController LibraryElementController;
        public RegionController RegionController;

        public RegionViewModel(Region model, LibraryElementController controller, RegionController regionController, Sizeable sizeable)
        {
            Model = model;
            LibraryElementController = controller;
            ContainerViewModel = sizeable;
            RegionController = regionController;
        }

        public void ChangeSize(object sender, double width, double height)
        {
            ContainerSizeChanged?.Invoke(sender,width,height);
        }
        
    }
}
