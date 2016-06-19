using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        protected LibraryElementController Controller;

        public RegionViewModel(Region model, LibraryElementController controller, Sizeable sizeable)
        {
            Model = model;
            Controller = controller;
            ContainerViewModel = sizeable;
        }
        public void ChangeSize(object sender, double width, double height)
        {
            ContainerSizeChanged?.Invoke(sender,width,height);
        }
    }
}
